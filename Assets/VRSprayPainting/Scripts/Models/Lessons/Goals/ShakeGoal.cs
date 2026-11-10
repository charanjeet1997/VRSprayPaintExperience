using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Done when the spray can is mixed. Starts from a settled can.</summary>
    [Serializable]
    public class ShakeGoal : PracticeGoal
    {
        [Range(0.1f, 1f)] public float targetMix = 0.9f;
        [Tooltip("Start from a settled (unmixed) can.")]
        public bool resetMix = true;

        public override void Begin(ILessonRunner runner)
        {
            if (resetMix)
                runner.Shaker.ResetMix();
        }

        public override float Progress(ILessonRunner runner) =>
            runner.Shaker.RequiresShaking ? runner.Shaker.Mix / targetMix : 1f;
    }
}
