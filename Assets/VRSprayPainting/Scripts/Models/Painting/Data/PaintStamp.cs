using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// One spray deposit in a surface's UV space. The spot is an ellipse: <see cref="axisU"/> and
    /// <see cref="axisV"/> are its two semi-axes in UV units, so tilted sprays and stretched UVs come out right.
    /// </summary>
    public struct PaintStamp
    {
        public Vector2 center;
        public Vector2 axisU;
        public Vector2 axisV;
        public Color color;
        [Tooltip("Opacity added at the spot centre (0-1).")]
        public float amount;
        [Tooltip("0 = hard wet edge (too close), 1 = soft feathered edge.")]
        public float softness;
        [Tooltip("0 = smooth coat, 1 = coarse speckle (dry spray from too far).")]
        public float grain;
        [Tooltip("Density of loose droplets outside the core.")]
        public float overspray;

        /// <summary>Round stamp with the default sweet-spot look.</summary>
        public static PaintStamp Circle(Vector2 uv, Color color, float radius, float amount) => new PaintStamp
        {
            center = uv,
            axisU = new Vector2(radius, 0f),
            axisV = new Vector2(0f, radius),
            color = color,
            amount = amount,
            softness = 0.6f,
            grain = 0.6f,
            overspray = 0.12f,
        };
    }
}
