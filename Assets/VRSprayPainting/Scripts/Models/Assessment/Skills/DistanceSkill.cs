using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Skills/Distance", fileName = "Skill_Distance")]
    public class DistanceSkill : SkillDefinition
    {
        public override float Score(SkillContext ctx) => ctx.spray.sweetTime / ctx.OnSurface;

        public override string Detail(SkillContext ctx) => $"avg {ctx.spray.AverageDistance * 100f:0} cm";

        public override string Feedback(SkillContext ctx) =>
            ctx.spray.tooCloseTime >= ctx.spray.tooFarTime
                ? $"You were too close {SkillContext.Pct(ctx.spray.tooCloseTime / ctx.OnSurface)} of the time. Too close = heavy, wet paint that runs. Keep {ctx.Range} (GREEN ring)."
                : $"You were too far {SkillContext.Pct(ctx.spray.tooFarTime / ctx.OnSurface)} of the time. Too far = paint dries in the air and lands rough and speckled. Keep {ctx.Range} (GREEN ring).";
    }
}
