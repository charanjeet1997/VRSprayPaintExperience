using System.Collections.Generic;
using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Generic lesson runner: plays any LessonData's step list in order. Steps do their own work through
    /// ILessonRunner; this class only drives the lesson panel, routes events to the current step, records
    /// results and shows the pass/fail result after an assessment. It knows no lesson or step in particular.
    /// </summary>
    public class LessonController : MonoBehaviour, ILessonRunner
    {
        [SerializeField] private LessonCatalog catalog;
        [SerializeField] private SprayDemoController demo;
        [SerializeField] private SprayTracker tracker;
        [SerializeField] private SprayPainter sprayPainter;
        [SerializeField] private AssessmentController assessment;
        [SerializeField] private PaintResetInput paintReset;
        [SerializeField] private SprayCanShaker shaker;
        [SerializeField] private PaintableSurface target;
        [SerializeField] private SafetyGear safety;

        private IPropertyManager propertyManagerCache;
        private IObserverManager observerManagerCache;
        // Lazy: Stop() can be called by another controller before this one's Start.
        private IPropertyManager propertyManager => propertyManagerCache ??= ServiceLocator.Current.Get<IPropertyManager>();
        private IObserverManager observerManager => observerManagerCache ??= ServiceLocator.Current.Get<IObserverManager>();
        private IProgressStore Progress => ServiceLocator.Current.Get<IProgressStore>();

        private LessonData lesson;
        private int stepIndex = -1;
        private LessonStep step;

        // Result handling after an assessment step.
        private enum ResultState { None, Measuring, Report, Shown }
        private ResultState resultState;
        private AssessmentResult lastResult;
        private int assessStepIndex;

        public LessonCatalog Catalog => catalog;
        public bool IsRunning => lesson != null;

        public LessonData Lesson => lesson;
        public SprayDemoController Demo => demo;
        public SprayTracker Tracker => tracker;
        public PaintResetInput PaintReset => paintReset;
        public SprayCanShaker Shaker => shaker;
        public PaintableSurface Target => target;
        public SafetyGear Safety => safety;

        private void Start()
        {
            observerManager.GetOrCreateObserver(ObserverNameConstants.LessonPrimaryPressed).Bind(this, OnPrimary);
            observerManager.GetOrCreateObserver(ObserverNameConstants.LessonSecondaryPressed).Bind(this, OnSecondary);
            observerManager.GetOrCreateObserver(ObserverNameConstants.AssessmentClosed).Bind(this, OnReportClosed);
            observerManager.GetOrCreateObserver(ObserverNameConstants.AssessmentRequested).Bind(this, () =>
            {
                if (step != null && resultState == ResultState.None)
                    step.OnFinishRequested(this);
            });

            demo.CaptionChanged += OnDemoCaption;
            demo.Finished += OnDemoFinished;
            Set(PropertyNameConstants.LessonVisible, false);
            Set<CoachReading>(PropertyNameConstants.CoachReading, null);
            SetCuesAllowed(true);
        }

        private void OnDestroy()
        {
            if (demo == null)
                return;
            demo.CaptionChanged -= OnDemoCaption;
            demo.Finished -= OnDemoFinished;
        }

        private void Update()
        {
            if (step != null && resultState == ResultState.None)
                step.Tick(this);
        }

        // ---- Lesson lifetime ----

        public void Begin(LessonData lessonData)
        {
            Stop();
            if (lessonData == null || lessonData.steps.Count == 0)
                return;

            lesson = lessonData;
            paintReset.ResetAll();
            GoTo(0);
        }

        /// <summary>Leaves the lesson: ends the current step, hides the report, the panel and the live reading.</summary>
        public void Stop()
        {
            ExitStep();
            lesson = null;
            stepIndex = -1;
            resultState = ResultState.None;
            if (demo.IsPlaying)
                demo.Stop();
            assessment.Close();
            SetCoachReading(null);
            SetCuesAllowed(true);
            HidePanel();
        }

        private void GoTo(int index)
        {
            ExitStep();
            resultState = ResultState.None;
            if (lesson == null)
                return;

            if (index >= lesson.steps.Count)
            {
                FinishLesson();
                return;
            }

            stepIndex = index;
            step = lesson.steps[index];
            if (step == null)
                GoTo(index + 1);
            else
                step.Enter(this);
        }

        private void ExitStep()
        {
            LessonStep leaving = step;
            step = null;
            leaving?.Exit(this);
        }

        // All steps done (or a lesson without an assessment): next lesson in the catalog, or the menu.
        private void FinishLesson()
        {
            // A lesson without an assessment (e.g. safety and prep) is passed by completing it.
            if (!lesson.steps.Exists(s => s is AssessStep))
                Progress.Record(lesson.lessonId, 1f, true);

            LessonData next = NextLesson();
            if (next != null && catalog.IsUnlocked(next, Progress, CurrentSprayer))
                Begin(next);
            else
                GoToMenu();
        }

        // Course for the sprayer in use (some lessons are gun- or can-only).
        private SprayerProfile CurrentSprayer => propertyManager.GetOrCreateProperty<SprayerProfile>(PropertyNameConstants.SprayerCurrent).Value;
        private List<LessonData> CurrentLessons => catalog.LessonsFor(CurrentSprayer);

        private LessonData NextLesson()
        {
            List<LessonData> lessons = CurrentLessons;
            for (int i = 0; i < lessons.Count - 1; i++)
                if (lessons[i] == lesson)
                    return lessons[i + 1];
            return null;
        }

        // ---- ILessonRunner ----

        public void ShowPanel(string stage, string title, string body, float progress, string primaryLabel, string secondaryLabel)
        {
            Set(PropertyNameConstants.LessonStepLabel, $"LESSON {LessonNumber} · {stage}");
            Set(PropertyNameConstants.LessonTitle, title);
            Set(PropertyNameConstants.LessonBody, body);
            Set(PropertyNameConstants.LessonProgress, progress);
            Set(PropertyNameConstants.LessonPrimaryLabel, primaryLabel);
            Set(PropertyNameConstants.LessonSecondaryLabel, secondaryLabel);
            Set(PropertyNameConstants.LessonSucceeded, false);
            Set(PropertyNameConstants.LessonVisible, true);
        }

        public void HidePanel() => Set(PropertyNameConstants.LessonVisible, false);
        public void SetBody(string body) => Set(PropertyNameConstants.LessonBody, body);
        public void SetProgress(float progress) => Set(PropertyNameConstants.LessonProgress, progress);
        public void SetCoachReading(CoachReading reading) => Set(PropertyNameConstants.CoachReading, reading);
        public void SetCuesAllowed(bool allowed) => Set(PropertyNameConstants.CuesAllowed, allowed);

        public string Format(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text
                .Replace("{sweetMin}", $"{sprayPainter.SweetMin * 100f:0}")
                .Replace("{sweetMax}", $"{sprayPainter.SweetMax * 100f:0}")
                .Replace("{maxDistance}", $"{sprayPainter.MaxDistance * 100f:0}")
                .Replace("{maxTilt}", $"{tracker.MaxGoodTilt:0}");
        }

        public void Assess(IReadOnlyList<SkillDefinition> skills, IReadOnlyList<DefectDefinition> defects)
        {
            resultState = ResultState.Measuring;
            assessStepIndex = stepIndex;
            assessment.Assess(skills, defects, lesson.title, result =>
            {
                if (resultState != ResultState.Measuring)
                    return;
                lastResult = result;
                lastResult.passed = result.hasData && result.passed;
                if (result.hasData)
                    Progress.Record(lesson.lessonId, result.overall, lastResult.passed);
                resultState = ResultState.Report; // report is open; the result follows when it closes
            });
        }

        public void Next()
        {
            if (lesson != null)
                GoTo(stepIndex + 1);
        }

        public bool GoBackTo<T>() where T : LessonStep
        {
            for (int i = stepIndex - 1; i >= 0; i--)
            {
                if (lesson.steps[i] is T)
                {
                    GoTo(i);
                    return true;
                }
            }
            return false;
        }

        public bool HasEarlier<T>() where T : LessonStep
        {
            for (int i = stepIndex - 1; i >= 0; i--)
                if (lesson.steps[i] is T)
                    return true;
            return false;
        }

        public void GoToMenu() => observerManager.GetOrCreateObserver(ObserverNameConstants.MenuRequested).Notify();

        // ---- Result after an assessment ----

        private void OnReportClosed()
        {
            if (resultState == ResultState.Report)
                ShowResult();
        }

        private void ShowResult()
        {
            resultState = ResultState.Shown;
            bool passed = lastResult != null && lastResult.passed;
            int score = lastResult != null ? Mathf.RoundToInt(lastResult.overall * 100f) : 0;
            bool moreSteps = assessStepIndex + 1 < lesson.steps.Count;
            LessonData next = NextLesson();

            string body;
            if (lastResult == null || !lastResult.hasData)
                body = "Not enough spraying to judge. Spray the panel until the bar is full.";
            else if (passed)
                body = $"You scored {score}.\n" + (moreSteps ? "On to the next part." : next != null ? $"Next: {next.title}." : "You've finished all the lessons. Try Free Paint!");
            else
                body = $"You scored {score}. Read the tips in the report, then try again.";

            string primary = passed ? (moreSteps || next != null ? "Continue" : "Main menu") : "Try again";
            string secondary = passed ? "Try again" : "Main menu";
            ShowPanel("RESULT", passed ? "Lesson passed!" : "Not passed yet", body, -1f, primary, secondary);
            Set(PropertyNameConstants.LessonSucceeded, passed);
        }

        private void RetryPractice()
        {
            // Back to the practice right before this assessment.
            for (int i = assessStepIndex - 1; i >= 0; i--)
            {
                if (lesson.steps[i] is PracticeStep)
                {
                    GoTo(i);
                    return;
                }
            }
            GoTo(0);
        }

        // ---- Input routing ----

        private void OnPrimary()
        {
            if (lesson == null)
                return;
            if (resultState == ResultState.Shown)
            {
                if (lastResult != null && lastResult.passed) GoTo(assessStepIndex + 1);
                else RetryPractice();
                return;
            }
            step?.OnPrimary(this);
        }

        private void OnSecondary()
        {
            if (lesson == null)
                return;
            if (resultState == ResultState.Shown)
            {
                if (lastResult != null && lastResult.passed) RetryPractice();
                else GoToMenu();
                return;
            }
            step?.OnSecondary(this);
        }

        private void OnDemoCaption(string caption)
        {
            if (step != null && resultState == ResultState.None)
                step.OnDemoCaption(this, caption);
        }

        private void OnDemoFinished()
        {
            if (step != null && resultState == ResultState.None)
                step.OnDemoFinished(this);
        }

        private int LessonNumber
        {
            get
            {
                List<LessonData> lessons = CurrentLessons;
                for (int i = 0; i < lessons.Count; i++)
                    if (lessons[i] == lesson)
                        return i + 1;
                return 0;
            }
        }

        private void Set<T>(string key, T value) => propertyManager.GetOrCreateProperty<T>(key).Value = value;
    }
}
