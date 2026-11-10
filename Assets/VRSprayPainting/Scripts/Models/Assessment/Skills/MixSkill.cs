using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Spraying an aerosol can before it is shaken: it sputters, thin and blotchy. Can only.</summary>
    [CreateAssetMenu(menuName = "VR Spray Painting/Skills/Can Mixed", fileName = "Skill_Mix")]
    public class MixSkill : SkillDefinition
    {
        public override bool Applies(SkillContext ctx) => ctx.requiresShaking;

        public override float Score(SkillContext ctx) => 1f - ctx.spray.unmixedTime / ctx.OnSurface;

        public override string Detail(SkillContext ctx) => $"{ctx.spray.unmixedTime:0.0} s unmixed";

        public override string Feedback(SkillContext ctx) =>
            $"You sprayed for {ctx.spray.unmixedTime:0.0} s before the can was mixed. An unshaken can sputters and leaves a thin, blotchy coat. " +
            "Shake it hard for about a minute (until the ball rattles freely) before spraying, and again if you pause.";
    }
}
