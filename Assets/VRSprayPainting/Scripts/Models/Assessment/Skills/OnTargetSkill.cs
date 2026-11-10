using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Skills/Paint On Target", fileName = "Skill_OnTarget")]
    public class OnTargetSkill : SkillDefinition
    {
        public override float Score(SkillContext ctx) =>
            ctx.spray.onSurfaceTime / Mathf.Max(ctx.spray.onSurfaceTime + ctx.spray.missedTime, 1e-4f);

        public override string Detail(SkillContext ctx) => $"{(1f - Mathf.Clamp01(Score(ctx))) * 100f:0}% wasted";

        public override string Feedback(SkillContext ctx) =>
            $"{SkillContext.Pct(ctx.spray.missedTime / Mathf.Max(ctx.spray.sprayTime, 1e-4f))} of your spray missed the panel (wasted paint). " +
            "Start each pass just before the edge and stop just after it.";
    }
}
