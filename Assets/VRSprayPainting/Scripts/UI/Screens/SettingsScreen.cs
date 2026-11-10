using System;
using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// Instructor settings: pass mark, how hard defects count, drying time between coats, training cues.
    /// Saved on the device (TrainingSettings) and used by every assessment and lesson.
    /// </summary>
    public class SettingsScreen : UISystem.Screen
    {
        [SerializeField] private SettingRowView passMarkRow;
        [SerializeField] private SettingRowView strictnessRow;
        [SerializeField] private SettingRowView dryTimeRow;
        [SerializeField] private SettingRowView cuesRow;
        [SerializeField] private Button backButton;

        private void Start()
        {
            IObserverManager observers = ServiceLocator.Current.Get<IObserverManager>();
            Property<bool> visible = ServiceLocator.Current.Get<IPropertyManager>().GetOrCreateProperty<bool>(PropertyNameConstants.SettingsVisible);
            visible.Bind(this, v =>
            {
                if (v) { Refresh(); Show(); }
                else Hide();
            });
            if (visible.Value) { Refresh(); Show(); } else Hide();

            passMarkRow.Stepped += step => TrainingSettings.PassMark = Step(TrainingSettings.PassMarks, TrainingSettings.PassMark, step);
            strictnessRow.Stepped += step =>
                TrainingSettings.Strictness = (DefectStrictness)Mathf.Clamp((int)TrainingSettings.Strictness + step, 0, (int)DefectStrictness.Large);
            dryTimeRow.Stepped += step => TrainingSettings.DryTime = Step(TrainingSettings.DryTimes, TrainingSettings.DryTime, step);
            cuesRow.Stepped += _ => TrainingSettings.CuesEnabled = !TrainingSettings.CuesEnabled;
            TrainingSettings.Changed += Refresh;

            backButton.onClick.AddListener(() => observers.GetOrCreateObserver(ObserverNameConstants.MenuRequested).Notify());
        }

        private void OnDestroy() => TrainingSettings.Changed -= Refresh;

        private void Refresh()
        {
            passMarkRow.Show("Pass mark", $"{Mathf.RoundToInt(TrainingSettings.PassMark * 100f)}", "Overall score needed to pass any lesson.");
            strictnessRow.Show("Defect strictness", TrainingSettings.Strictness.ToString(), "How many points runs, dry spray, orange peel and missed spots cost.");
            dryTimeRow.Show("Drying time", $"{TrainingSettings.DryTime:0} s", "Time a coat takes to dry before the next (real paint: minutes to hours).");
            cuesRow.Show("Training cues", TrainingSettings.CuesEnabled ? "On" : "Off", "Aim ring and live readings on the sprayer. Off = test conditions.");
        }

        // Next / previous value from a fixed list (nearest to the current one).
        private static float Step(float[] values, float current, int step)
        {
            int nearest = 0;
            for (int i = 1; i < values.Length; i++)
                if (Mathf.Abs(values[i] - current) < Mathf.Abs(values[nearest] - current))
                    nearest = i;
            return values[Mathf.Clamp(nearest + step, 0, values.Length - 1)];
        }
    }
}
