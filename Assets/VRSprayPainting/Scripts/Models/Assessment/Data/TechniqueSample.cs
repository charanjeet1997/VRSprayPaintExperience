namespace VRSprayPainting
{
    /// <summary>One point of the technique timeline (recorded a few times a second while paint lands).</summary>
    public struct TechniqueSample
    {
        /// <summary>Seconds of spraying on the surface so far.</summary>
        public float time;
        /// <summary>Nozzle distance, metres.</summary>
        public float distance;
        /// <summary>Tilt from straight on, degrees.</summary>
        public float tilt;
        /// <summary>Coat number this sample belongs to (1-based).</summary>
        public int coat;
    }
}
