using System.Collections.Generic;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Scores an attempt: technique = weighted average of the skills; defects (runs, dry spray, ...) then deduct
    /// points scaled by the instructor's strictness. Adds per-coat technique, film thickness and plain-language
    /// feedback (defects first, then the weakest skills). Knows no skill or defect in particular.
    /// </summary>
    public static class AssessmentCalculator
    {
        public const float PassMark = 0.7f;
        /// <summary>Less trigger time than this is not enough to judge.</summary>
        private const float MinSprayTime = 1.5f;
        private const float FeedbackBelow = 0.8f;
        private const float DefectFeedbackAbove = 0.25f;
        private const int MaxFeedback = 4;

        public static AssessmentResult Evaluate(SkillContext ctx, IReadOnlyList<SkillDefinition> skills,
            IReadOnlyList<DefectDefinition> defects, float passMark)
        {
            var result = new AssessmentResult
            {
                coat = ctx.coat,
                passMark = passMark,
                sweetMin = ctx.sweetMin,
                sweetMax = ctx.sweetMax,
                maxGoodTilt = ctx.maxGoodTilt,
            };

            if (ctx.spray.sprayTime < MinSprayTime || ctx.spray.onSurfaceTime <= 0f)
            {
                result.headline = "Spray the panel first";
                result.feedback.Add("Hold the trigger and paint the grey panel, then check your work again.");
                return result;
            }
            result.hasData = true;

            // Technique.
            float total = 0f, weights = 0f;
            var scoredSkills = new List<(SkillDefinition skill, float score)>();
            if (skills != null)
            {
                foreach (SkillDefinition skill in skills)
                {
                    if (skill == null || !skill.Applies(ctx))
                        continue;
                    float score = Mathf.Clamp01(skill.Score(ctx));
                    result.skills.Add(new SkillScore { label = skill.Label, score = score, detail = skill.Detail(ctx) });
                    scoredSkills.Add((skill, score));
                    total += score * skill.Weight;
                    weights += skill.Weight;
                }
            }
            result.techniqueScore = weights > 0f ? total / weights : 0f;

            // Defects.
            float deducted = 0f;
            var scoredDefects = new List<(DefectDefinition defect, float severity)>();
            if (defects != null)
            {
                foreach (DefectDefinition defect in defects)
                {
                    if (defect == null || !defect.Applies(ctx))
                        continue;
                    float severity = Mathf.Clamp01(defect.Severity(ctx));
                    float points = severity * defect.MaxDeduction * TrainingSettings.StrictnessScale;
                    result.defects.Add(new DefectScore { label = defect.Label, detail = defect.Detail(ctx), points = points, severity = severity });
                    scoredDefects.Add((defect, severity));
                    deducted += points;
                }
            }

            result.overall = Mathf.Clamp01(result.techniqueScore - deducted / 100f);
            result.passed = result.overall >= passMark;
            result.headline = result.overall >= 0.9f ? "Excellent spraying!" : result.passed ? "Good job, you passed" : "Keep practising";

            foreach (SprayStats coat in ctx.coats)
                result.coatScores.Add(CoatTechnique(coat));

            // Feedback: the worst defects first, then the weakest skills.
            scoredDefects.Sort((a, b) => b.severity.CompareTo(a.severity));
            foreach ((DefectDefinition defect, float severity) in scoredDefects)
            {
                if (result.feedback.Count >= 2 || severity < DefectFeedbackAbove)
                    break;
                result.feedback.Add(defect.Feedback(ctx));
            }

            scoredSkills.Sort((a, b) => a.score.CompareTo(b.score));
            foreach ((SkillDefinition skill, float score) in scoredSkills)
            {
                if (result.feedback.Count >= MaxFeedback || score >= FeedbackBelow)
                    break;
                string line = skill.Feedback(ctx);
                if (!string.IsNullOrEmpty(line))
                    result.feedback.Add(line);
            }

            if (result.feedback.Count == 0)
                result.feedback.Add("Steady, straight, at the right distance and no defects. Well done!");
            return result;
        }

        /// <summary>Angle, distance and speed together for one coat, 0..1.</summary>
        public static float CoatTechnique(SprayStats coat)
        {
            float onSurface = Mathf.Max(coat.onSurfaceTime, 1e-4f);
            float angle = coat.angleOkTime / onSurface;
            float distance = coat.sweetTime / onSurface;
            float speed = coat.speedSampleTime > 0f ? coat.speedOkTime / coat.speedSampleTime : 0f;
            return Mathf.Clamp01((angle + distance + speed) / 3f);
        }
    }
}
