using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Defects/Dry Spray", fileName = "Defect_DrySpray")]
    public class DrySprayDefect : DefectDefinition
    {
        [Tooltip("Share of spray time too far / too fast that counts as full severity.")]
        [SerializeField, Range(0.05f, 1f)] private float worstShare = 0.35f;

        private static float Share(SkillContext ctx) =>
            (ctx.spray.tooFarTime + ctx.spray.tooFastTime * 0.5f) / ctx.OnSurface;

        public override float Severity(SkillContext ctx) => Mathf.Clamp01(Share(ctx) / worstShare);

        public override string Detail(SkillContext ctx) => $"{SkillContext.Pct(Share(ctx))} of spraying";

        public override string Feedback(SkillContext ctx) =>
            "Dry spray: the paint dried in the air before landing, so it went on rough and sandy. " +
            $"It comes from spraying too far or sweeping too fast. Stay {ctx.Range} and keep a steady pace.";
    }
}
