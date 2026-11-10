using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Converts the simulator's paint amount into real dry film thickness and holds the target range.
    /// Thickness in µm = volume × micronsPerVolume. Below the target = thin / see-through, above = heavy (runs).
    /// </summary>
    [Serializable]
    public struct FilmSpec
    {
        [Tooltip("Dry film thickness (µm) per unit of simulated paint volume.")]
        public float micronsPerVolume;
        [Tooltip("Target dry film thickness for the finished job, µm.")]
        public Vector2 target;

        public static FilmSpec Default => new FilmSpec { micronsPerVolume = 25f, target = new Vector2(40f, 75f) };

        public float ToMicrons(float volume) => volume * micronsPerVolume;
        public float TargetMinVolume => target.x / Mathf.Max(micronsPerVolume, 1e-3f);
        public float TargetMaxVolume => target.y / Mathf.Max(micronsPerVolume, 1e-3f);
    }
}
