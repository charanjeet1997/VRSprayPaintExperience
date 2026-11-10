using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// The user practises on a clean panel with an optional live reading on the sprayer. Ends when its goal is
    /// reached, on "Finish now", or on the palette's "Check my work".
    /// </summary>
    [Serializable]
    public class PracticeStep : LessonStep
    {
        [TextArea(2, 5), Tooltip("Placeholders: {sweetMin} {sweetMax} {maxDistance} {maxTilt}")]
        public string instruction;
        [Tooltip("Live reading on the sprayer while practising (empty = none).")]
        public CoachReading reading;
        [SerializeReference, SubclassSelector] public PracticeGoal goal = new SprayTimeGoal();
        [Tooltip("Wipe the panel and start a fresh recording when the step starts.")]
        public bool cleanPanel = true;
        [Tooltip("Show the aim ring and live readings (off for tests).")]
        public bool showCues = true;

        public override void Enter(ILessonRunner runner)
        {
            if (cleanPanel)
                runner.PaintReset.ResetAll();
            goal?.Begin(runner);
            runner.ShowPanel("YOUR TURN", runner.Lesson.title, runner.Format(instruction), 0f, "Finish now",
                runner.HasEarlier<DemoStep>() ? "Watch demo again" : "Main menu");
            runner.SetCoachReading(reading);
            runner.SetCuesAllowed(showCues);
        }

        public override void Tick(ILessonRunner runner)
        {
            float progress = goal != null ? Mathf.Clamp01(goal.Progress(runner)) : 0f;
            runner.SetProgress(progress);
            if (progress >= 1f)
                runner.Next();
        }

        public override void Exit(ILessonRunner runner)
        {
            runner.SetCoachReading(null);
            runner.SetCuesAllowed(true);
        }

        public override void OnSecondary(ILessonRunner runner)
        {
            if (!runner.GoBackTo<DemoStep>())
                runner.GoToMenu();
        }

        public override void OnFinishRequested(ILessonRunner runner) => runner.Next();
    }
}
