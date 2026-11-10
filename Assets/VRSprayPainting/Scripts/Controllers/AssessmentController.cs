using System;
using System.Collections.Generic;
using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// "Check my work": combines the recorded gun handling (SprayTracker) with the measured paint on the target
    /// panel (CoatAnalyzer), scores it and publishes the result for the report screen. Also switches the
    /// thickness heat map on and off.
    /// </summary>
    public class AssessmentController : MonoBehaviour
    {
        [SerializeField] private SprayTracker tracker;
        [SerializeField] private SprayPainter sprayPainter;
        [SerializeField] private SprayCanShaker shaker;
        [Tooltip("Surface whose coat is measured.")]
        [SerializeField] private PaintableSurface targetSurface;
        [Tooltip("Skills scored by Free Paint's \"Check my work\". Lessons pass their own.")]
        [SerializeField] private List<SkillDefinition> freePaintSkills = new List<SkillDefinition>();
        [Tooltip("Defects checked by Free Paint's \"Check my work\". Lessons pass their own.")]
        [SerializeField] private List<DefectDefinition> freePaintDefects = new List<DefectDefinition>();
        [SerializeField] private SprayGun sprayer;
        [SerializeField] private PaintRunSimulator runs;
        [SerializeField] private SafetyGear safety;

        private IPropertyManager propertyManagerCache;
        // Lazy: Close()/Assess() can be called by other controllers before this one's Start.
        private IPropertyManager propertyManager => propertyManagerCache ??= ServiceLocator.Current.Get<IPropertyManager>();
        private bool measuring;

        private void Start()
        {
            IObserverManager observers = ServiceLocator.Current.Get<IObserverManager>();
            observers.GetOrCreateObserver(ObserverNameConstants.AssessmentRequested).Bind(this, OnAssessRequested);
            observers.GetOrCreateObserver(ObserverNameConstants.AssessmentClosed).Bind(this, Close);
            observers.GetOrCreateObserver(ObserverNameConstants.AssessmentThicknessToggled).Bind(this, ToggleThickness);

            SetThicknessView(false);
            Set(PropertyNameConstants.AssessmentVisible, false);
        }

        private void OnDestroy() => Shader.SetGlobalFloat(ShaderIDs.ThicknessView, 0f);

        // Palette "Check my work". During a lesson the LessonController owns assessment (it treats the button as
        // "finish now"), so the free assessment stays out of its way.
        private void OnAssessRequested()
        {
            if (propertyManager.GetOrCreateProperty<AppMode>(PropertyNameConstants.AppMode).Value != AppMode.Lesson)
                Assess(freePaintSkills, freePaintDefects, "Free paint");
        }

        /// <summary>
        /// Scores the handling recorded so far plus the paint on the target (skills, then defect deductions) and
        /// shows the report. The pass mark is the instructor setting. onDone receives the result as well.
        /// </summary>
        public void Assess(IReadOnlyList<SkillDefinition> skills, IReadOnlyList<DefectDefinition> defects, string activity, Action<AssessmentResult> onDone = null)
        {
            if (measuring)
                return;

            measuring = true;
            SprayStats spray = tracker.Snapshot();
            List<SprayStats> coats = tracker.CoatSnapshots();
            List<TechniqueSample> samples = tracker.SamplesSnapshot();
            CoatAnalyzer.Measure(targetSurface, Film, coat =>
            {
                measuring = false;
                if (this == null)
                    return;

                var ctx = new SkillContext
                {
                    spray = spray,
                    coat = coat,
                    maxGoodTilt = tracker.MaxGoodTilt,
                    sweetMin = sprayPainter.SweetMin,
                    sweetMax = sprayPainter.SweetMax,
                    requiresShaking = shaker != null && shaker.RequiresShaking,
                    runCount = runs != null ? runs.RunCount : 0,
                    coats = coats,
                    respiratorWorn = safety == null || safety.RespiratorWorn,
                    gogglesWorn = safety == null || safety.GogglesWorn,
                    fanOn = safety == null || safety.FanOn,
                };
                AssessmentResult result = AssessmentCalculator.Evaluate(ctx, skills, defects, TrainingSettings.PassMark);
                result.samples.AddRange(samples);
                AttemptHistory.Record(result, activity, sprayer != null && sprayer.Profile != null ? sprayer.Profile.displayName : "");
                Set(PropertyNameConstants.AssessmentResult, result);
                Set(PropertyNameConstants.AssessmentVisible, true);
                onDone?.Invoke(result);
            });
        }

        /// <summary>Hides the report (and the heat map).</summary>
        public void Close()
        {
            SetThicknessView(false);
            Set(PropertyNameConstants.AssessmentVisible, false);
        }

        private void ToggleThickness() =>
            SetThicknessView(!propertyManager.GetOrCreateProperty<bool>(PropertyNameConstants.AssessmentThicknessView).Value);

        private FilmSpec Film => sprayer != null && sprayer.Profile != null ? sprayer.Profile.film : FilmSpec.Default;

        private void SetThicknessView(bool on)
        {
            // Heat map bands follow the film target: below = thin (blue), in target = green, above = heavy (red).
            FilmSpec film = Film;
            Shader.SetGlobalFloat(ShaderIDs.FilmLow, film.TargetMinVolume);
            Shader.SetGlobalFloat(ShaderIDs.FilmHigh, film.TargetMaxVolume);
            Shader.SetGlobalFloat(ShaderIDs.ThicknessView, on ? 1f : 0f);
            Set(PropertyNameConstants.AssessmentThicknessView, on);
        }

        private void Set<T>(string key, T value) => propertyManager.GetOrCreateProperty<T>(key).Value = value;
    }
}
