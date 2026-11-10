using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Bare or see-through areas on a job that should be fully painted.</summary>
    [CreateAssetMenu(menuName = "VR Spray Painting/Defects/Missed Spots", fileName = "Defect_Missed")]
    public class MissedSpotsDefect : DefectDefinition
    {
        [Tooltip("Share of the surface bare or thin that counts as full severity.")]
        [SerializeField, Range(0.05f, 1f)] private float worstShare = 0.4f;

        private static float Share(SkillContext ctx) => ctx.coat.missingFraction + ctx.coat.thinFraction * 0.5f;

        public override float Severity(SkillContext ctx) => Mathf.Clamp01(Share(ctx) / worstShare);

        public override string Detail(SkillContext ctx) =>
            $"{ctx.coat.missingFraction * 100f:0}% bare, {ctx.coat.thinFraction * 100f:0}% thin";

        public override string Feedback(SkillContext ctx) =>
            $"Missed spots: {ctx.coat.missingFraction * 100f:0}% of the panel has no paint and {ctx.coat.thinFraction * 100f:0}% is see-through. " +
            "Overlap each pass by half, paint edge to edge, and put on a second thin coat once the first is dry.";
    }
}
