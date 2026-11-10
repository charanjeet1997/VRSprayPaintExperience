using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Skills/Coverage", fileName = "Skill_Coverage")]
    public class CoverageSkill : SkillDefinition
    {
        [Tooltip("Share of the surface that must be fully covered for a full score.")]
        [SerializeField, Range(0.1f, 1f)] private float targetCoverage = 0.9f;

        public override float Score(SkillContext ctx) => ctx.coat.coveredFraction / targetCoverage;

        public override string Detail(SkillContext ctx) => $"{ctx.coat.coveredFraction * 100f:0}% covered";

        public override string Feedback(SkillContext ctx) =>
            $"Only {ctx.coat.coveredFraction * 100f:0}% of the panel is fully covered. Overlap each pass by about half and paint from edge to edge.";
    }
}
