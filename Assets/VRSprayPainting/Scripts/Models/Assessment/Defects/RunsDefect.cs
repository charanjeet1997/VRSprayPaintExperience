using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Defects/Runs and Sags", fileName = "Defect_Runs")]
    public class RunsDefect : DefectDefinition
    {
        [Tooltip("Runs that count as full severity.")]
        [SerializeField, Min(1)] private int worstRuns = 4;

        public override float Severity(SkillContext ctx) =>
            Mathf.Clamp01(ctx.runCount / (float)worstRuns + ctx.coat.thickFraction * 2f);

        public override string Detail(SkillContext ctx) =>
            ctx.runCount > 0 ? $"{ctx.runCount} run{(ctx.runCount == 1 ? "" : "s")}" : $"{ctx.coat.thickFraction * 100f:0}% heavy";

        public override string Feedback(SkillContext ctx) =>
            ctx.spray.longestDwell >= 1f
                ? $"Paint ran: you held the spray on one spot for {ctx.spray.longestDwell:0.0} s. Too much paint in one place runs and sags. Keep moving while the trigger is pressed."
                : "Paint ran where the coat got too heavy. Move a little faster or step back, and build the colour with thin coats.";
    }
}
