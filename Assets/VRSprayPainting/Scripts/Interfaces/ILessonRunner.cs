using System.Collections.Generic;

namespace VRSprayPainting
{
    /// <summary>
    /// What a lesson step can use while it runs: the lesson panel, the shared services (demo, tracker, assessment,
    /// paint reset, live reading) and flow control. Implemented by LessonController.
    /// </summary>
    public interface ILessonRunner
    {
        LessonData Lesson { get; }
        SprayDemoController Demo { get; }
        SprayTracker Tracker { get; }
        PaintResetInput PaintReset { get; }
        SprayCanShaker Shaker { get; }
        /// <summary>The surface being painted (wetness, coats).</summary>
        PaintableSurface Target { get; }
        /// <summary>Respirator, goggles and booth fan.</summary>
        SafetyGear Safety { get; }

        /// <summary>Fills the lesson panel and shows it. progress &lt; 0 hides the bar; empty labels hide buttons.</summary>
        void ShowPanel(string stage, string title, string body, float progress, string primaryLabel, string secondaryLabel);
        void HidePanel();
        void SetBody(string body);
        void SetProgress(float progress);
        /// <summary>Live reading shown on the sprayer; null hides it.</summary>
        void SetCoachReading(CoachReading reading);
        /// <summary>Allows or hides the training cues (aim ring, live readings) for this step.</summary>
        void SetCuesAllowed(bool allowed);

        /// <summary>Replaces {sweetMin}, {sweetMax}, {maxDistance}, {maxTilt} with the current sprayer's values.</summary>
        string Format(string text);

        /// <summary>Scores the attempt with these skills and shows the report; the runner handles the result.</summary>
        void Assess(IReadOnlyList<SkillDefinition> skills, IReadOnlyList<DefectDefinition> defects);

        /// <summary>Goes to the next step (or finishes the lesson).</summary>
        void Next();
        /// <summary>Goes back to the closest earlier step of type T. False if there is none.</summary>
        bool GoBackTo<T>() where T : LessonStep;
        /// <summary>Is there an earlier step of type T in this lesson?</summary>
        bool HasEarlier<T>() where T : LessonStep;
        void GoToMenu();
    }
}
