using System.Collections.Generic;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Everything skills and defects need to score an attempt: what was recorded, what was measured, the targets.</summary>
    public class SkillContext
    {
        public SprayStats spray;
        public CoatStats coat;
        /// <summary>Gun counts as straight within this angle (degrees).</summary>
        public float maxGoodTilt;
        /// <summary>Ideal nozzle distance range, metres.</summary>
        public float sweetMin;
        public float sweetMax;
        /// <summary>The sprayer must be shaken (aerosol can).</summary>
        public bool requiresShaking;
        /// <summary>Paint runs that formed on the surface.</summary>
        public int runCount;
        /// <summary>Technique recorded per coat (a new coat starts after the previous one dried).</summary>
        public List<SprayStats> coats = new List<SprayStats>();

        // Safety and prep.
        public bool respiratorWorn = true;
        public bool gogglesWorn = true;
        public bool fanOn = true;

        public string Range => $"{sweetMin * 100f:0}-{sweetMax * 100f:0} cm";
        public float OnSurface => Mathf.Max(spray.onSurfaceTime, 1e-4f);

        public static string Pct(float fraction) => $"{Mathf.Clamp01(fraction) * 100f:0}%";
    }
}
