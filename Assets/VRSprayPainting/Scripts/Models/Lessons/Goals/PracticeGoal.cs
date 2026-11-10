using System;

namespace VRSprayPainting
{
    /// <summary>When a practice step is done. New kind of goal (passes, coverage...) = new subclass.</summary>
    [Serializable]
    public abstract class PracticeGoal
    {
        /// <summary>Called when the practice step starts.</summary>
        public virtual void Begin(ILessonRunner runner) { }

        /// <summary>0..1; the step ends at 1.</summary>
        public abstract float Progress(ILessonRunner runner);
    }
}
