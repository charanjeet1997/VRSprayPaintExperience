namespace VRSprayPainting
{
    /// <summary>
    /// Paint thickness measured over a surface by CoatAnalyzer, against the film target. Fractions are of the
    /// whole surface: missing + thin + inTarget + thick = 1.
    /// </summary>
    public class CoatStats
    {
        public FilmSpec film = FilmSpec.Default;

        public float missingFraction = 1f;  // no paint at all (bare spots)
        public float thinFraction;          // painted but below the target thickness (see-through)
        public float inTargetFraction;      // within the target thickness
        public float thickFraction;         // above the target: heavy, would run / sag
        public float coveredFraction;       // at or above the target minimum (in target + thick)
        public float paintedFraction;       // any paint at all

        /// <summary>Spread of thickness over the body of the coat (std-dev / mean). 0 = perfectly even.</summary>
        public float variation;
        /// <summary>Average over the body of the coat, simulated volume.</summary>
        public float averageVolume;
        /// <summary>Average dry film thickness over the body of the coat, µm.</summary>
        public float averageFilm;
    }
}
