using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Scores the attempt just made with the chosen skills and shows the report. The runner then shows the result:
    /// pass = continue / next lesson, fail = back to the practice step. The pass mark is the instructor setting.
    /// </summary>
    [Serializable]
    public class AssessStep : LessonStep
    {
        public List<SkillDefinition> skills = new List<SkillDefinition>();
        [Tooltip("Defects that cost points in this assessment.")]
        public List<DefectDefinition> defects = new List<DefectDefinition>();

        public override void Enter(ILessonRunner runner)
        {
            runner.HidePanel();
            runner.Assess(skills, defects);
        }
    }
}
