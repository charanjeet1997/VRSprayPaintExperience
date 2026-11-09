using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Paint loaded in the gun. Spot size and coat strength come from the spray distance and cone,
    /// see <see cref="SprayPainter"/>.
    /// </summary>
    [Serializable]
    public struct SprayBrushData
    {
        public Color color;

        [Tooltip("Coat added per stamp at the centre of the sweet spot with the default cone. Narrower cone = same paint on a smaller spot = heavier.")]
        [Range(0f, 1f)] public float amount;

        public static SprayBrushData Default => new SprayBrushData
        {
            color = new Color(0.85f, 0.1f, 0.1f),
            amount = 0.45f,
        };
    }
}
