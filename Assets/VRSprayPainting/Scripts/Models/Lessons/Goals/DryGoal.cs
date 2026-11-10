using System;

namespace VRSprayPainting
{
    /// <summary>Done when the paint on the target has dried (TrainingSettings.DryTime after the last spray).</summary>
    [Serializable]
    public class DryGoal : PracticeGoal
    {
        public override float Progress(ILessonRunner runner) =>
            runner.Target != null ? 1f - runner.Target.Wetness : 1f;
    }
}
