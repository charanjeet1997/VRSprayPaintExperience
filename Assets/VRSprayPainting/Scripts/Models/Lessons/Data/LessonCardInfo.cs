namespace VRSprayPainting
{
    /// <summary>What the main menu shows for one lesson.</summary>
    public struct LessonCardInfo
    {
        public int index;
        public string title;
        public string summary;
        public bool unlocked;
        /// <summary>Lesson to pass first when locked.</summary>
        public string lockedBy;
        public bool passed;
        /// <summary>Best score 0..1, negative when never attempted.</summary>
        public float bestScore;
    }
}
