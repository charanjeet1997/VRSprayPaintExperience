using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// One assessed skill (gun angle, distance, coverage...). Each skill is an asset of a subclass that knows how
    /// to score itself and explain a low score. New skill = new subclass + asset; the assessment code is untouched.
    /// </summary>
    public abstract class SkillDefinition : ScriptableObject
    {
        [SerializeField] private string label = "Skill";
        [Tooltip("Share of the overall score when several skills are assessed together.")]
        [SerializeField, Min(0f)] private float weight = 1f;
        [Tooltip("Runs and sags come from this skill (distance, speed, coat): the run warning is included when it is assessed.")]
        [SerializeField] private bool judgesRuns;

        public string Label => label;
        public float Weight => weight;
        public bool JudgesRuns => judgesRuns;

        /// <summary>False when this skill doesn't apply to the attempt (e.g. shaking with a spray gun); it is then left out.</summary>
        public virtual bool Applies(SkillContext ctx) => true;

        /// <summary>0..1 (clamped by the caller).</summary>
        public abstract float Score(SkillContext ctx);

        /// <summary>Short measured value shown next to the bar, e.g. "avg 12°".</summary>
        public abstract string Detail(SkillContext ctx);

        /// <summary>Plain-language feedback for a low score: the mistake, its real-world effect, the fix.</summary>
        public abstract string Feedback(SkillContext ctx);
    }
}
