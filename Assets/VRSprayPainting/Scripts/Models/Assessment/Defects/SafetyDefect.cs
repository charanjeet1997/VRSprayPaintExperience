using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Spraying without a respirator and goggles, or without the booth fan running.</summary>
    [CreateAssetMenu(menuName = "VR Spray Painting/Defects/Safety", fileName = "Defect_Safety")]
    public class SafetyDefect : DefectDefinition
    {
        public override float Severity(SkillContext ctx) =>
            (ctx.respiratorWorn ? 0f : 0.4f) + (ctx.gogglesWorn ? 0f : 0.3f) + (ctx.fanOn ? 0f : 0.3f);

        public override string Detail(SkillContext ctx)
        {
            var missing = new System.Collections.Generic.List<string>();
            if (!ctx.respiratorWorn) missing.Add("respirator");
            if (!ctx.gogglesWorn) missing.Add("eye protection");
            if (!ctx.fanOn) missing.Add("fan");
            return missing.Count == 0 ? "all set" : "no " + string.Join(", ", missing);
        }

        public override string Feedback(SkillContext ctx) =>
            "Safety: spray paint fumes and mist are toxic. Always wear a respirator rated for paint and safety goggles, " +
            "and switch the booth extraction fan on before spraying.";
    }
}
