using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Skills/Gun Angle", fileName = "Skill_Angle")]
    public class AngleSkill : SkillDefinition
    {
        public override float Score(SkillContext ctx) => ctx.spray.angleOkTime / ctx.OnSurface;

        public override string Detail(SkillContext ctx) => $"avg {ctx.spray.AverageTilt:0}°";

        public override string Feedback(SkillContext ctx) =>
            $"You held the sprayer tilted more than {ctx.maxGoodTilt:0}° for {SkillContext.Pct(1f - ctx.spray.angleOkTime / ctx.OnSurface)} of the time. " +
            "Tilted = more paint on the near side, less on the far side. Point it straight at the surface.";
    }
}
