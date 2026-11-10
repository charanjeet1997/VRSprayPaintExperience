using UnityEngine;

namespace VRSprayPainting
{
    public enum ReadingState
    {
        Good,
        Low,    // e.g. too close
        High,   // e.g. too far
        Bad,
    }

    /// <summary>What a live reading shows this frame.</summary>
    public struct CoachValue
    {
        public string value;
        public string hint;
        public ReadingState state;
    }

    /// <summary>Live state the readings can use.</summary>
    public struct CoachContext
    {
        public SprayPainter painter;
        public SprayTracker tracker;
        public SprayCanShaker shaker;
    }

    /// <summary>
    /// A live reading shown on the sprayer while practising (angle, distance, ...). Each reading is an asset of a
    /// subclass; lessons pick one per practice step. New reading = new subclass + asset.
    /// </summary>
    public abstract class CoachReading : ScriptableObject
    {
        /// <summary>False when there is nothing to show this frame.</summary>
        public abstract bool TryRead(CoachContext ctx, out CoachValue reading);

        /// <summary>Pointing at a surface within reach and not touching it.</summary>
        protected static bool Aimed(SprayPainter painter) =>
            painter.Zone != SprayZone.NoSurface && painter.Zone != SprayZone.Clipping;
    }
}
