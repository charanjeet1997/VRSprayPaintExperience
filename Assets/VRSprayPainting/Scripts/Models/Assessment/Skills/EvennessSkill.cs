using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Skills/Even Coat", fileName = "Skill_Evenness")]
    public class EvennessSkill : SkillDefinition
    {
        [Tooltip("Coat variation (std-dev / mean) that scores zero.")]
        [SerializeField] private float worstVariation = 0.5f;
        [Tooltip("Score lost per unit of too-thick area fraction.")]
        [SerializeField] private float thickPenalty = 3f;

        public override float Score(SkillContext ctx) =>
            1f - ctx.coat.variation / Mathf.Max(worstVariation, 1e-3f) - ctx.coat.thickFraction * thickPenalty;

        public override string Detail(SkillContext ctx) =>
            ctx.coat.thickFraction > 0.01f ? $"{ctx.coat.thickFraction * 100f:0}% too thick" : "";

        public override string Feedback(SkillContext ctx) =>
            "The coat is patchy: some areas thick, some thin. Keep the same speed and distance on every pass.";
    }
}
