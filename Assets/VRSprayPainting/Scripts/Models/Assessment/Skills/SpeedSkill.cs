using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Skills/Speed", fileName = "Skill_Speed")]
    public class SpeedSkill : SkillDefinition
    {
        public override float Score(SkillContext ctx) =>
            ctx.spray.speedSampleTime > 0f ? ctx.spray.speedOkTime / ctx.spray.speedSampleTime : 0f;

        public override string Detail(SkillContext ctx) => $"avg {ctx.spray.AverageSpeed * 100f:0} cm/s";

        public override string Feedback(SkillContext ctx) =>
            ctx.spray.tooSlowTime >= ctx.spray.tooFastTime
                ? "You moved too slowly. Slow passes put on too much paint, which runs. Sweep at a steady walking-arm pace."
                : "You moved too fast. Fast passes leave a thin, see-through coat. Slow down a little and keep the pace steady.";
    }
}
