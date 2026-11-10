using System.Collections.Generic;

namespace VRSprayPainting
{
    public struct SkillScore
    {
        public string label;
        /// <summary>0..1</summary>
        public float score;
        /// <summary>Short measured value, e.g. "avg 12°".</summary>
        public string detail;
    }

    public struct DefectScore
    {
        public string label;
        public string detail;
        /// <summary>Points lost (out of 100).</summary>
        public float points;
        /// <summary>0..1</summary>
        public float severity;
    }

    /// <summary>
    /// Outcome of one assessment: technique scores, defects with deductions, film thickness against the target,
    /// per-coat technique, the technique timeline and plain-language feedback.
    /// </summary>
    public class AssessmentResult
    {
        public readonly List<SkillScore> skills = new List<SkillScore>();
        public readonly List<DefectScore> defects = new List<DefectScore>();
        /// <summary>Technique score 0..1 for each coat, in order.</summary>
        public readonly List<float> coatScores = new List<float>();
        /// <summary>Distance / angle over time, for the technique graph.</summary>
        public readonly List<TechniqueSample> samples = new List<TechniqueSample>();
        public CoatStats coat = new CoatStats();

        /// <summary>0..1, after deductions.</summary>
        public float overall;
        /// <summary>0..1, technique before deductions.</summary>
        public float techniqueScore;
        public float passMark;
        public bool passed;
        public string headline;
        /// <summary>"What went wrong and why", most important first.</summary>
        public readonly List<string> feedback = new List<string>();
        public bool hasData;

        // Targets, for drawing.
        public float sweetMin;
        public float sweetMax;
        public float maxGoodTilt;
    }
}
