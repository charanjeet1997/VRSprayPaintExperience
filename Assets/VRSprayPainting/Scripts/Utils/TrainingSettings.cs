using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>How hard defects count against the score.</summary>
    public enum DefectStrictness
    {
        Off,
        Small,
        Medium,
        Large,
    }

    /// <summary>
    /// Instructor settings, saved on the device: pass mark, defect strictness, drying time between coats and
    /// whether training cues (aim ring, live readings) are shown. Read by assessment, lessons and cues.
    /// </summary>
    public static class TrainingSettings
    {
        private const string PassMarkKey = "VRSP.Settings.PassMark";
        private const string StrictnessKey = "VRSP.Settings.DefectStrictness";
        private const string DryTimeKey = "VRSP.Settings.DryTime";
        private const string CuesKey = "VRSP.Settings.Cues";

        public static readonly float[] PassMarks = { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f };
        public static readonly float[] DryTimes = { 10f, 20f, 30f, 60f };

        /// <summary>Raised after any setting changes.</summary>
        public static event Action Changed;

        /// <summary>0..1 overall score needed to pass any assessment.</summary>
        public static float PassMark
        {
            get => PlayerPrefs.GetFloat(PassMarkKey, 0.7f);
            set => Save(() => PlayerPrefs.SetFloat(PassMarkKey, Mathf.Clamp(value, 0.3f, 1f)));
        }

        public static DefectStrictness Strictness
        {
            get => (DefectStrictness)PlayerPrefs.GetInt(StrictnessKey, (int)DefectStrictness.Medium);
            set => Save(() => PlayerPrefs.SetInt(StrictnessKey, (int)value));
        }

        /// <summary>Seconds for a coat to dry (sped up from the real minutes-to-hours).</summary>
        public static float DryTime
        {
            get => PlayerPrefs.GetFloat(DryTimeKey, 20f);
            set => Save(() => PlayerPrefs.SetFloat(DryTimeKey, Mathf.Max(1f, value)));
        }

        /// <summary>Aim ring and live readings on the sprayer.</summary>
        public static bool CuesEnabled
        {
            get => PlayerPrefs.GetInt(CuesKey, 1) == 1;
            set => Save(() => PlayerPrefs.SetInt(CuesKey, value ? 1 : 0));
        }

        /// <summary>Multiplier on every defect's maximum deduction.</summary>
        public static float StrictnessScale => Strictness switch
        {
            DefectStrictness.Off => 0f,
            DefectStrictness.Small => 0.5f,
            DefectStrictness.Large => 1.5f,
            _ => 1f,
        };

        private static void Save(Action write)
        {
            write();
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
