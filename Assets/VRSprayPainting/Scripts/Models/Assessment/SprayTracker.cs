using System.Collections.Generic;
using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Records how the user sprays: every frame the trigger is held it logs gun angle, distance, spot speed over the
    /// surface and whether paint is landing. Ignores the intro demo (autopilot). Starts over when the paint is cleared.
    /// </summary>
    public class SprayTracker : MonoBehaviour
    {
        [SerializeField] private SprayGun sprayGun;
        [SerializeField] private SprayPainter sprayPainter;

        [Header("Real-world targets")]
        [Tooltip("Gun counts as straight within this many degrees of the surface normal.")]
        [SerializeField] private float maxGoodTilt = 15f;
        [Tooltip("Good travel speed of the spray spot over the surface, m/s (real guns: about 0.3 m/s).")]
        [SerializeField] private Vector2 goodSpeed = new Vector2(0.12f, 0.6f);
        [Tooltip("Spot moving less than this (metres) counts as holding still.")]
        [SerializeField] private float dwellRadius = 0.02f;
        [Tooltip("Spot jumps larger than this in one frame (another surface, an edge) are not counted as speed.")]
        [SerializeField] private float maxFrameJump = 0.1f;
        [Tooltip("A sweep must travel at least this far (metres) to count as a pass.")]
        [SerializeField] private float minPassLength = 0.2f;
        [Tooltip("Spraying counts as unmixed (sputtering) above this.")]
        [SerializeField, Range(0f, 1f)] private float unmixedSputter = 0.3f;

        private SprayStats stats = new SprayStats();
        private bool hadSpot;
        private Vector3 lastSpot;
        private Vector3 dwellAnchor;
        private float dwellTime;
        private float sweepTravel;
        private float sweepSign;

        // Coats: a new coat starts when spraying resumes after the paint dried (TrainingSettings.DryTime).
        private readonly List<SprayStats> coatStarts = new List<SprayStats>();
        private float lastLandingTime = float.NegativeInfinity;
        // Technique timeline for the report graph.
        private readonly List<TechniqueSample> samples = new List<TechniqueSample>();
        private float sampleTimer;
        private const float SampleInterval = 0.1f;
        private const int MaxSamples = 3000;

        /// <summary>Coats sprayed since the last reset (0 before any paint landed).</summary>
        public int CoatCount => coatStarts.Count;
        /// <summary>Time.time paint last landed (for drying).</summary>
        public float LastLandingTime => lastLandingTime;

        /// <summary>Technique recorded per coat, in order.</summary>
        public List<SprayStats> CoatSnapshots()
        {
            var coats = new List<SprayStats>();
            for (int i = 0; i < coatStarts.Count; i++)
            {
                SprayStats end = i + 1 < coatStarts.Count ? coatStarts[i + 1] : stats;
                coats.Add(Difference(end, coatStarts[i]));
            }
            return coats;
        }

        public List<TechniqueSample> SamplesSnapshot() => new List<TechniqueSample>(samples);

        private static SprayStats Difference(SprayStats end, SprayStats start) => new SprayStats
        {
            sprayTime = end.sprayTime - start.sprayTime,
            onSurfaceTime = end.onSurfaceTime - start.onSurfaceTime,
            missedTime = end.missedTime - start.missedTime,
            angleOkTime = end.angleOkTime - start.angleOkTime,
            tiltSum = end.tiltSum - start.tiltSum,
            sweetTime = end.sweetTime - start.sweetTime,
            tooCloseTime = end.tooCloseTime - start.tooCloseTime,
            tooFarTime = end.tooFarTime - start.tooFarTime,
            distanceSum = end.distanceSum - start.distanceSum,
            speedSampleTime = end.speedSampleTime - start.speedSampleTime,
            speedOkTime = end.speedOkTime - start.speedOkTime,
            tooSlowTime = end.tooSlowTime - start.tooSlowTime,
            tooFastTime = end.tooFastTime - start.tooFastTime,
            speedSum = end.speedSum - start.speedSum,
        };

        public float MaxGoodTilt => maxGoodTilt;
        public Vector2 GoodSpeed => goodSpeed;
        /// <summary>Seconds of paint landing on a surface since the last reset (no allocation, per-frame safe).</summary>
        public float OnSurfaceTime => stats.onSurfaceTime;
        /// <summary>Smoothed travel speed of the spray spot (m/s), 0 when not spraying on a surface.</summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>Passes completed since the last reset, including the one in progress if long enough.</summary>
        public int PassCount => stats.passes + (Mathf.Abs(sweepTravel) >= minPassLength ? 1 : 0);

        /// <summary>Copy of everything recorded since the last reset.</summary>
        public SprayStats Snapshot() => stats.Clone();

        public void ResetStats()
        {
            stats = new SprayStats();
            hadSpot = false;
            dwellTime = 0f;
            sweepTravel = 0f;
            sweepSign = 0f;
            coatStarts.Clear();
            samples.Clear();
            sampleTimer = 0f;
            lastLandingTime = float.NegativeInfinity;
        }

        private void OnEnable()
        {
            sprayGun.SprayStarted += OnSprayStarted;
            sprayGun.SprayStopped += OnSprayStopped;
        }

        private void OnDisable()
        {
            sprayGun.SprayStarted -= OnSprayStarted;
            sprayGun.SprayStopped -= OnSprayStopped;
        }

        private bool Landing
        {
            get
            {
                SprayZone zone = sprayPainter.Zone;
                return zone == SprayZone.Wet || zone == SprayZone.Sweet || zone == SprayZone.Dry;
            }
        }

        private void OnSprayStarted()
        {
            if (sprayGun.IsAutopilot)
                return;
            stats.sprayStarts++;
            if (Landing)
                stats.startsOnSurface++;
        }

        private void OnSprayStopped()
        {
            if (sprayGun.IsAutopilot)
                return;
            stats.sprayStops++;
            if (Landing)
                stats.stopsOnSurface++;
            EndSweep();
        }

        // A sweep counts as a pass when it travelled far enough.
        private void EndSweep()
        {
            if (Mathf.Abs(sweepTravel) >= minPassLength)
                stats.passes++;
            sweepTravel = 0f;
            sweepSign = 0f;
        }

        private void Start()
        {
            ServiceLocator.Current.Get<IObserverManager>()
                .GetOrCreateObserver(ObserverNameConstants.PaintResetPerformed)
                .Bind(this, ResetStats);
        }

        // After SprayPainter.Update, so the aim state is this frame's.
        private void LateUpdate()
        {
            if (!sprayGun.IsSpraying || sprayGun.IsAutopilot)
            {
                CurrentSpeed = 0f;
                hadSpot = false;
                dwellTime = 0f;
                return;
            }

            float dt = Time.deltaTime;
            stats.sprayTime += dt;

            SprayZone zone = sprayPainter.Zone;
            bool landing = zone == SprayZone.Wet || zone == SprayZone.Sweet || zone == SprayZone.Dry;
            if (!landing)
            {
                // Clipping isn't wasted paint, just no paint; everything else misses the target.
                if (zone != SprayZone.Clipping)
                    stats.missedTime += dt;
                hadSpot = false;
                dwellTime = 0f;
                CurrentSpeed = 0f;
                EndSweep();
                return;
            }

            if (sprayPainter.Sputter > unmixedSputter)
                stats.unmixedTime += dt;

            if (coatStarts.Count == 0 || Time.time - lastLandingTime >= TrainingSettings.DryTime)
                coatStarts.Add(stats.Clone());
            lastLandingTime = Time.time;
            stats.onSurfaceTime += dt;

            float tilt = sprayPainter.AimTilt;
            stats.tiltSum += tilt * dt;
            if (tilt <= maxGoodTilt)
                stats.angleOkTime += dt;

            float distance = sprayPainter.AimDistance;
            stats.distanceSum += distance * dt;

            sampleTimer += dt;
            if (sampleTimer >= SampleInterval && samples.Count < MaxSamples)
            {
                sampleTimer = 0f;
                samples.Add(new TechniqueSample { time = stats.onSurfaceTime, distance = distance, tilt = tilt, coat = coatStarts.Count });
            }
            if (zone == SprayZone.Sweet) stats.sweetTime += dt;
            else if (zone == SprayZone.Wet) stats.tooCloseTime += dt;
            else stats.tooFarTime += dt;

            Vector3 spot = sprayPainter.AimPoint;
            if (hadSpot && dt > 0f)
            {
                float step = Vector3.Distance(spot, lastSpot);
                if (step <= maxFrameJump)
                {
                    float speed = step / dt;
                    CurrentSpeed = Mathf.Lerp(CurrentSpeed, speed, 1f - Mathf.Exp(-8f * dt));
                    stats.speedSampleTime += dt;
                    stats.speedSum += speed * dt;
                    if (speed < goodSpeed.x) stats.tooSlowTime += dt;
                    else if (speed > goodSpeed.y) stats.tooFastTime += dt;
                    else stats.speedOkTime += dt;

                    // Passes: travel along the surface's horizontal; a direction change ends the sweep.
                    Vector3 across = Vector3.Cross(Vector3.up, sprayPainter.AimNormal);
                    if (across.sqrMagnitude < 1e-4f)
                        across = Vector3.right;
                    float along = Vector3.Dot(spot - lastSpot, across.normalized);
                    if (Mathf.Abs(along) > 1e-4f)
                    {
                        float sign = Mathf.Sign(along);
                        if (sweepSign != 0f && sign != sweepSign && Mathf.Abs(sweepTravel) >= minPassLength * 0.5f)
                            EndSweep();
                        sweepSign = sign;
                        sweepTravel += along;
                    }
                }
            }

            // Holding the spray on one spot.
            if (!hadSpot || Vector3.Distance(spot, dwellAnchor) > dwellRadius)
            {
                dwellAnchor = spot;
                dwellTime = 0f;
            }
            else
            {
                dwellTime += dt;
                stats.longestDwell = Mathf.Max(stats.longestDwell, dwellTime);
            }

            lastSpot = spot;
            hadSpot = true;
        }
    }
}
