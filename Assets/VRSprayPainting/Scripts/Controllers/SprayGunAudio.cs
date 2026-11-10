using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Spray gun sounds: valve burst on press, a looping air hiss while spraying, a choke-off on release.
    /// The hiss gets higher and thinner as the cone tightens and a little louder/brighter near a surface
    /// (the spray bouncing back), like a real gun.
    /// </summary>
    public class SprayGunAudio : MonoBehaviour
    {
        [SerializeField] private SprayGun sprayGun;
        [SerializeField] private SprayPainter sprayPainter;
        [SerializeField] private AudioSource loopSource;
        [SerializeField] private AudioSource oneShotSource;

        [SerializeField] private AudioClip hissLoop;
        [SerializeField] private AudioClip sprayStart;
        [SerializeField] private AudioClip sprayStop;
        [SerializeField] private AudioClip triggerClick;

        [SerializeField, Range(0f, 1f)] private float hissVolume = 0.55f;
        [SerializeField] private float fadeIn = 0.04f;
        [SerializeField] private float fadeOut = 0.12f;
        [Tooltip("Pitch at the widest cone and at the tightest cone.")]
        [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.18f);
        [Tooltip("Extra volume when the nozzle is at the minimum distance from a surface.")]
        [SerializeField, Range(0f, 1f)] private float nearSurfaceBoost = 0.3f;

        private float level;

        private void OnEnable()
        {
            sprayGun.SprayStarted += OnSprayStarted;
            sprayGun.SprayStopped += OnSprayStopped;
            sprayGun.ProfileChanged += ApplyProfile;
        }

        private void OnDisable()
        {
            sprayGun.SprayStarted -= OnSprayStarted;
            sprayGun.SprayStopped -= OnSprayStopped;
            sprayGun.ProfileChanged -= ApplyProfile;
            loopSource.Stop();
            level = 0f;
        }

        private void Start()
        {
            loopSource.clip = hissLoop;
            loopSource.loop = true;
            loopSource.playOnAwake = false;
            loopSource.volume = 0f;
            oneShotSource.playOnAwake = false;
        }

        // Each sprayer sounds different (gun air hiss vs aerosol can).
        private void ApplyProfile(SprayerProfile profile)
        {
            if (profile.hissLoop != null) hissLoop = profile.hissLoop;
            if (profile.sprayStart != null) sprayStart = profile.sprayStart;
            if (profile.sprayStop != null) sprayStop = profile.sprayStop;
            if (profile.triggerClick != null) triggerClick = profile.triggerClick;
            hissVolume = profile.hissVolume;
            pitchRange = profile.pitchRange;

            bool wasPlaying = loopSource.isPlaying;
            loopSource.clip = hissLoop;
            if (wasPlaying)
                loopSource.Play();
        }

        private void Update()
        {
            float target = sprayGun.IsSpraying ? 1f : 0f;
            float rate = 1f / Mathf.Max(target > level ? fadeIn : fadeOut, 0.001f);
            level = Mathf.MoveTowards(level, target, rate * Time.deltaTime);

            if (level <= 0f)
            {
                if (loopSource.isPlaying)
                    loopSource.Stop();
                return;
            }

            float precision = Mathf.InverseLerp(sprayGun.MaxConeAngle, sprayGun.MinConeAngle, sprayGun.ConeAngle);
            float near = 0f;
            if (sprayPainter != null && sprayPainter.Zone != SprayZone.NoSurface)
                near = 1f - Mathf.InverseLerp(sprayPainter.ClipDistance, sprayPainter.SweetMax, sprayPainter.AimDistance);

            loopSource.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, precision) + near * 0.04f;
            loopSource.volume = hissVolume * level * (1f + near * nearSurfaceBoost) * Mathf.Lerp(1f, 0.85f, precision);

            if (!loopSource.isPlaying)
            {
                // Random start point so repeated presses don't sound identical.
                loopSource.time = Random.Range(0f, hissLoop.length * 0.9f);
                loopSource.Play();
            }
        }

        private void OnSprayStarted()
        {
            oneShotSource.pitch = Random.Range(0.96f, 1.04f);
            if (triggerClick != null)
                oneShotSource.PlayOneShot(triggerClick, 0.6f);
            if (sprayStart != null)
                oneShotSource.PlayOneShot(sprayStart, hissVolume);
        }

        private void OnSprayStopped()
        {
            oneShotSource.pitch = Random.Range(0.96f, 1.04f);
            if (sprayStop != null)
                oneShotSource.PlayOneShot(sprayStop, hissVolume * 0.8f);
            if (triggerClick != null)
                oneShotSource.PlayOneShot(triggerClick, 0.35f);
        }
    }
}
