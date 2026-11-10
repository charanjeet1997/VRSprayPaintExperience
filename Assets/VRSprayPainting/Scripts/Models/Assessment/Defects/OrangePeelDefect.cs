using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Defects/Orange Peel", fileName = "Defect_OrangePeel")]
    public class OrangePeelDefect : DefectDefinition
    {
        [Tooltip("Share of spray time too close that counts as full severity.")]
        [SerializeField, Range(0.05f, 1f)] private float worstShare = 0.35f;

        private static float Share(SkillContext ctx) => ctx.spray.tooCloseTime / ctx.OnSurface;

        public override float Severity(SkillContext ctx) => Mathf.Clamp01(Share(ctx) / worstShare);

        public override string Detail(SkillContext ctx) => $"{SkillContext.Pct(Share(ctx))} too close";

        public override string Feedback(SkillContext ctx) =>
            "Orange peel: spraying too close blasts wet paint into a bumpy, dimpled surface like orange skin instead of letting it level out. " +
            $"Hold the sprayer {ctx.Range} away.";
    }
}
