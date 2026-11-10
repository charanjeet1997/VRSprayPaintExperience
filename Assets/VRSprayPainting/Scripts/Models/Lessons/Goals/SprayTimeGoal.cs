using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Done after this many seconds of paint landing on a surface.</summary>
    [Serializable]
    public class SprayTimeGoal : PracticeGoal
    {
        [Min(1f)] public float seconds = 10f;

        public override float Progress(ILessonRunner runner) => runner.Tracker.OnSurfaceTime / seconds;
    }
}
