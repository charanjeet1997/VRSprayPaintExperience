using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>The real sprayer demonstrates by itself; its captions show on the panel. Continues when it ends.</summary>
    [Serializable]
    public class DemoStep : LessonStep
    {
        [Tooltip("Captions accept the same placeholders as text steps.")]
        public DemoMove[] moves = new DemoMove[0];

        public override void Enter(ILessonRunner runner)
        {
            runner.ShowPanel("WATCH", runner.Lesson.title, "Watch the sprayer...", -1f, "Skip demo", "Main menu");
            // Can't fly a sprayer that is in a hand: move on.
            if (!runner.Demo.Play(moves))
                runner.Next();
        }

        public override void Exit(ILessonRunner runner)
        {
            if (runner.Demo.IsPlaying)
                runner.Demo.Stop();
        }

        // Skip: the sprayer flies home, then Finished moves on.
        public override void OnPrimary(ILessonRunner runner) => runner.Demo.Stop();
        public override void OnDemoCaption(ILessonRunner runner, string caption) => runner.SetBody(runner.Format(caption));
        public override void OnDemoFinished(ILessonRunner runner) => runner.Next();
    }
}
