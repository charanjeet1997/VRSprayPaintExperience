using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// A real-world paint defect (runs, dry spray, orange peel, missed spots...). Each is an asset of a subclass
    /// that measures how bad it was (0..1). The score loses up to maxDeduction points, scaled by the instructor's
    /// defect strictness. New defect = new subclass + asset.
    /// </summary>
    public abstract class DefectDefinition : ScriptableObject
    {
        [SerializeField] private string label = "Defect";
        [Tooltip("Points (out of 100) lost at full severity, before the strictness setting.")]
        [SerializeField, Range(0f, 50f)] private float maxDeduction = 10f;

        public string Label => label;
        public float MaxDeduction => maxDeduction;

        /// <summary>False when this defect can't happen in the attempt; it is then left out.</summary>
        public virtual bool Applies(SkillContext ctx) => true;

        /// <summary>0 = none, 1 = as bad as it gets.</summary>
        public abstract float Severity(SkillContext ctx);

        /// <summary>Short measured value, e.g. "3 runs".</summary>
        public abstract string Detail(SkillContext ctx);

        /// <summary>Cause, real-world effect and fix, in plain language.</summary>
        public abstract string Feedback(SkillContext ctx);
    }
}
