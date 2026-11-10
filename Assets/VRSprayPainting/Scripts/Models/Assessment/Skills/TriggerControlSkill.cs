using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Starting or stopping the spray while aimed at the panel dumps a burst of paint there (heavy edges, spits).
    /// Good technique: press the trigger before the edge, release after it.
    /// </summary>
    [CreateAssetMenu(menuName = "VR Spray Painting/Skills/Trigger Control", fileName = "Skill_TriggerControl")]
    public class TriggerControlSkill : SkillDefinition
    {
        public override bool Applies(SkillContext ctx) => ctx.spray.sprayStarts + ctx.spray.sprayStops > 0;

        public override float Score(SkillContext ctx)
        {
            int total = ctx.spray.sprayStarts + ctx.spray.sprayStops;
            int onPanel = ctx.spray.startsOnSurface + ctx.spray.stopsOnSurface;
            return total > 0 ? 1f - (float)onPanel / total : 1f;
        }

        public override string Detail(SkillContext ctx) =>
            $"{ctx.spray.startsOnSurface + ctx.spray.stopsOnSurface} of {ctx.spray.sprayStarts + ctx.spray.sprayStops} on panel";

        public override string Feedback(SkillContext ctx) =>
            $"You started or stopped spraying on the panel {ctx.spray.startsOnSurface + ctx.spray.stopsOnSurface} times. " +
            "Each start and stop dumps extra paint there, leaving heavy spots at the edges. Press the trigger just before the panel and release just after it.";
    }
}
