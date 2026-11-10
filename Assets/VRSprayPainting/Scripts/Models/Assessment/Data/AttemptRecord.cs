using System;
using System.Collections.Generic;

namespace VRSprayPainting
{
    /// <summary>One saved assessment, for the history screen and the instructor's CSV export.</summary>
    [Serializable]
    public class AttemptRecord
    {
        public string timestamp;   // ISO 8601, local time
        public string activity;    // lesson title or "Free paint"
        public string sprayer;
        public int score;          // 0..100 after deductions
        public int technique;      // 0..100 before deductions
        public int passMark;
        public bool passed;
        public int coats;
        public int averageFilm;    // µm
        public int onTargetPercent;
        public string defects;     // "Runs and sags -6; Dry spray -3"
        public string skills;      // "Sprayer angle 85; Distance 72"
    }

    [Serializable]
    public class AttemptHistoryData
    {
        public List<AttemptRecord> attempts = new List<AttemptRecord>();
    }
}
