using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Aerosol cans must be shaken: the mixing ball rattles and pigment and propellant mix. This detects real
    /// shaking from the sprayer's movement (hard back-and-forth acceleration), plays a clack and a buzz per
    /// stroke, raises the mix, and lets it settle back over time. An unmixed can sputters (SprayPainter.Sputter).
    /// Only active for sprayer profiles that require shaking.
    /// </summary>
    [DefaultExecutionOrder(-40)] // after the demo moves the sprayer, before the painter sprays
    public class SprayCanShaker : MonoBehaviour
    {
        [SerializeField] private SprayGun sprayer;
        [SerializeField] private SprayPainter sprayPainter;
        [SerializeField] private AudioSource rattleSource;
        [Tooltip("Peak hand speed (m/s) a half-stroke must reach to count as shaking.")]
        [SerializeField] private float minShakeSpeed = 0.5f;
        [Tooltip("A half-stroke slower than this (seconds) is an ordinary move, not a shake.")]
        [SerializeField] private float maxHalfStrokeTime = 0.3f;
        [Tooltip("Speed (m/s) above which a direction reversal is detected.")]
        [SerializeField] private float reversalSpeed = 0.12f;
        [Tooltip("Velocity smoothing time constant (s), filters tracking jitter.")]
        [SerializeField] private float velocitySmoothing = 0.02f;
        [Tooltip("Strokes closer together than this are one stroke.")]
        [SerializeField] private float minStrokeInterval = 0.07f;
        [SerializeField, Range(0f, 1f)] private float hapticAmplitude = 0.4f;

        private SprayerProfile profile;
        private Vector3 lastPosition;
        private Vector3 lastVelocity;
        private Vector3 lastStrokeDirection;
        private float lastStrokeTime;
        private Vector3 halfDirection;
        private float halfPeakSpeed;
        private float halfStartTime;
        private bool previousHalfQuick;
        private bool hasHistory;

        /// <summary>0 = settled (sputters), 1 = fully mixed.</summary>
        public float Mix { get; private set; }
        public bool RequiresShaking => profile != null && profile.requiresShaking;
        public int Strokes { get; private set; }

        private void OnEnable() => sprayer.ProfileChanged += OnProfileChanged;
        private void OnDisable() => sprayer.ProfileChanged -= OnProfileChanged;

        private void OnProfileChanged(SprayerProfile newProfile)
        {
            profile = newProfile;
            ResetMix();
        }

        /// <summary>Back to a settled can (e.g. at the start of the shaking lesson).</summary>
        public void ResetMix()
        {
            Mix = 0f;
            Strokes = 0;
            hasHistory = false;
            halfDirection = Vector3.zero;
            previousHalfQuick = false;
            Apply();
        }

        private void Update()
        {
            if (!RequiresShaking)
            {
                hasHistory = false;
                if (sprayPainter.Sputter != 0f)
                    sprayPainter.Sputter = 0f;
                return;
            }

            float dt = Time.deltaTime;
            Vector3 position = sprayer.transform.position;
            bool moving = sprayer.IsHeld || sprayer.IsAutopilot;
            if (moving && hasHistory && dt > 1e-4f)
            {
                // Smoothed velocity: per-frame tracking jitter must not look like a shake.
                Vector3 raw = (position - lastPosition) / dt;
                lastVelocity = Vector3.Lerp(lastVelocity, raw, 1f - Mathf.Exp(-dt / velocitySmoothing));
                TrackHalfStroke(lastVelocity);
            }
            else
            {
                lastVelocity = Vector3.zero;
                halfDirection = Vector3.zero;
                previousHalfQuick = false;
            }

            lastPosition = position;
            hasHistory = moving;

            // Settles back slowly when left alone.
            Mix = Mathf.Max(0f, Mix - dt / Mathf.Max(profile.settleSeconds, 1f));
            Apply();
        }

        /// <summary>
        /// A shake is a fast back-and-forth: the hand moves quickly one way, then reverses within a short time.
        /// Each reversal is where the mixing ball hits the end of the can. Ordinary moves are slow or don't
        /// reverse quickly, so they don't count; a reversal only clacks if the half-stroke before it was also quick
        /// (an oscillation, not a single flick).
        /// </summary>
        private void TrackHalfStroke(Vector3 velocity)
        {
            float speed = velocity.magnitude;
            if (halfDirection == Vector3.zero)
            {
                if (speed > reversalSpeed)
                    BeginHalf(velocity, speed);
                return;
            }

            if (Vector3.Dot(velocity, halfDirection) < 0f && speed > reversalSpeed)
            {
                bool quick = halfPeakSpeed >= minShakeSpeed && Time.time - halfStartTime <= maxHalfStrokeTime;
                if (quick && previousHalfQuick && Time.time - lastStrokeTime > minStrokeInterval)
                    Stroke(halfDirection);
                previousHalfQuick = quick;
                BeginHalf(velocity, speed);
                return;
            }

            halfPeakSpeed = Mathf.Max(halfPeakSpeed, speed);
            // Too slow to be a shake: drop the rhythm.
            if (Time.time - halfStartTime > maxHalfStrokeTime)
                previousHalfQuick = false;
        }

        private void BeginHalf(Vector3 velocity, float speed)
        {
            halfDirection = velocity / speed;
            halfPeakSpeed = speed;
            halfStartTime = Time.time;
        }

        private void Stroke(Vector3 direction)
        {
            Strokes++;
            lastStrokeTime = Time.time;
            lastStrokeDirection = direction;
            Mix = Mathf.Min(1f, Mix + profile.mixPerShake);

            AudioClip[] clips = profile.rattleClips;
            if (rattleSource != null && clips != null && clips.Length > 0)
            {
                AudioClip clip = clips[Random.Range(0, clips.Length)];
                rattleSource.pitch = Random.Range(0.93f, 1.07f);
                // Harder shakes clack louder.
                rattleSource.PlayOneShot(clip, Mathf.Clamp01(0.75f + Random.Range(0f, 0.25f)));
            }
            sprayer.PulseHaptic(hapticAmplitude, 0.03f);
        }

        // Unmixed paint sputters; a well-shaken can sprays smoothly.
        private void Apply() => sprayPainter.Sputter = RequiresShaking ? Mathf.Clamp01(1f - Mix * 1.15f) : 0f;
    }
}
