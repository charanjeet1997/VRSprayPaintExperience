using System;

namespace VRSprayPainting
{
    /// <summary>
    /// One stage of a lesson (explain, demo, practice, assess...). A lesson is an ordered list of steps, stored by
    /// reference in LessonData, so lessons can mix and repeat steps freely. New kind of stage = new subclass;
    /// the runner calls these hooks and knows no step in particular.
    /// </summary>
    [Serializable]
    public abstract class LessonStep
    {
        public abstract void Enter(ILessonRunner runner);
        public virtual void Tick(ILessonRunner runner) { }
        public virtual void Exit(ILessonRunner runner) { }

        public virtual void OnPrimary(ILessonRunner runner) => runner.Next();
        public virtual void OnSecondary(ILessonRunner runner) => runner.GoToMenu();

        /// <summary>The palette's "Check my work" was pressed during this step.</summary>
        public virtual void OnFinishRequested(ILessonRunner runner) { }
        public virtual void OnDemoCaption(ILessonRunner runner, string caption) { }
        public virtual void OnDemoFinished(ILessonRunner runner) { }
    }
}
