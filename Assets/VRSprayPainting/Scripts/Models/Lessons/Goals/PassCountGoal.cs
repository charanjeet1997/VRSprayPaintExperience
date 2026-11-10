using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Done after this many full sweeps across the surface, counted from the start of the step.</summary>
    [Serializable]
    public class PassCountGoal : PracticeGoal
    {
        [Min(1)] public int passes = 4;

        [NonSerialized] private int startCount;

        public override void Begin(ILessonRunner runner) => startCount = runner.Tracker.PassCount;

        public override float Progress(ILessonRunner runner) => (float)(runner.Tracker.PassCount - startCount) / passes;
    }
}
