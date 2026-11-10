using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Text on the lesson panel: what to do and why. Continues on the primary button.</summary>
    [Serializable]
    public class ExplainStep : LessonStep
    {
        [Tooltip("Empty = the lesson title.")]
        public string title;
        [TextArea(3, 8), Tooltip("Placeholders: {sweetMin} {sweetMax} {maxDistance} {maxTilt}")]
        public string body;
        public string buttonLabel = "Continue";

        public override void Enter(ILessonRunner runner) =>
            runner.ShowPanel("INTRO", string.IsNullOrEmpty(title) ? runner.Lesson.title : title, runner.Format(body), -1f, buttonLabel, "Main menu");
    }
}
