namespace VRSprayPainting
{
    /// <summary>
    /// How the user handled the gun while the trigger was held, accumulated by SprayTracker.
    /// All times in seconds of trigger-held time.
    /// </summary>
    public class SprayStats
    {
        public float sprayTime;        // trigger held
        public float onSurfaceTime;    // ...and paint was landing (in range, on a paintable)
        public float missedTime;       // ...and the spray hit nothing paintable or was out of range (wasted paint)

        // Of onSurfaceTime:
        public float angleOkTime;      // tilt within the allowed angle
        public float tiltSum;          // tilt × dt, for the average tilt
        public float sweetTime;        // in the sweet distance
        public float tooCloseTime;
        public float tooFarTime;
        public float distanceSum;      // distance × dt, for the average distance

        // Travel speed of the spray spot over the surface.
        public float speedSampleTime;
        public float speedOkTime;
        public float tooSlowTime;
        public float tooFastTime;
        public float speedSum;         // speed × dt

        /// <summary>Longest time the spot stayed (almost) in one place with the trigger held: makes runs in real life.</summary>
        public float longestDwell;

        /// <summary>Completed sweeps across the surface (a pass ends on a direction change, release or leaving it).</summary>
        public int passes;

        // Trigger control: where spraying started / stopped. Good technique starts and stops off the panel.
        public int sprayStarts;
        public int startsOnSurface;
        public int sprayStops;
        public int stopsOnSurface;

        /// <summary>Paint landing while the can was not mixed (sputtering).</summary>
        public float unmixedTime;

        public float AverageTilt => onSurfaceTime > 0f ? tiltSum / onSurfaceTime : 0f;
        public float AverageDistance => onSurfaceTime > 0f ? distanceSum / onSurfaceTime : 0f;
        public float AverageSpeed => speedSampleTime > 0f ? speedSum / speedSampleTime : 0f;

        public SprayStats Clone() => (SprayStats)MemberwiseClone();
    }
}
