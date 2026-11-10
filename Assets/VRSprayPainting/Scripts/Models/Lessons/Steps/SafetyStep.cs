using System;
using System.Text;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Safety and prep before spraying: put on the respirator and goggles (bring each to your face) and switch the
    /// booth fan on. A live checklist on the panel ticks each one; the step ends when all are done.
    /// </summary>
    [Serializable]
    public class SafetyStep : LessonStep
    {
        [TextArea(2, 5)] public string instruction = "Get ready like a real painter:";
        [Tooltip("Take the gear off first so the trainee puts it on.")]
        public bool resetGear = true;

        [NonSerialized] private string lastBody;

        public override void Enter(ILessonRunner runner)
        {
            if (resetGear)
                runner.Safety.ResetGear();
            runner.Safety.GuideActive = true;
            lastBody = null;
            runner.ShowPanel("SAFETY", runner.Lesson.title, Checklist(runner), 0f, "", "Main menu");
        }

        public override void Tick(ILessonRunner runner)
        {
            SafetyGear gear = runner.Safety;
            string body = Checklist(runner);
            if (body != lastBody)
            {
                lastBody = body;
                runner.SetBody(body);
            }

            int total = gear.FullFace ? 2 : 3;
            int done = (gear.RespiratorWorn ? 1 : 0) + (!gear.FullFace && gear.GogglesWorn ? 1 : 0) + (gear.FanOn ? 1 : 0);
            runner.SetProgress((float)done / total);
            if (gear.AllSet)
                runner.Next();
        }

        public override void Exit(ILessonRunner runner) => runner.Safety.GuideActive = false;

        private string Checklist(ILessonRunner runner)
        {
            SafetyGear gear = runner.Safety;
            var text = new StringBuilder(instruction).Append('\n');
            if (gear.FullFace)
            {
                text.Append(Line(gear.RespiratorWorn, "Full-face respirator on: pick it up from the small table on your right and bring it to your face. Its visor protects your eyes too"));
            }
            else
            {
                text.Append(Line(gear.RespiratorWorn, "Respirator on: pick it up from the small table on your right and bring it to your face"));
                text.Append(Line(gear.GogglesWorn, "Safety goggles on: pick them up and bring them to your eyes"));
            }
            text.Append(Line(gear.FanOn, "Booth fan on: point at the red BOOTH FAN sign above the table and pull the trigger"));
            text.Append("\n<color=#ffa030>Follow the orange arrow on your right.</color>");
            return text.ToString();
        }

        private static string Line(bool done, string text) =>
            done ? $"<color=#4fd16a>● {text}</color>\n" : $"○ {text}\n";
    }
}
