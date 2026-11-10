using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// One kind of sprayer (spray gun, spray can...): its real-world working range, spray pattern, paint flow and
    /// sounds. The held sprayer, painter and audio take everything from the active profile; lessons and feedback
    /// read the range through placeholders. New sprayer = new profile asset + a SprayerModel on the sprayer.
    /// </summary>
    [CreateAssetMenu(menuName = "VR Spray Painting/Sprayer Profile", fileName = "Sprayer")]
    public class SprayerProfile : ScriptableObject
    {
        [Tooltip("Stable key. Don't change after release.")]
        public string sprayerId = "gun";
        public string displayName = "Spray gun";
        [Tooltip("Prefix for saved lesson progress; each sprayer keeps its own results. Empty = unprefixed keys.")]
        public string progressScope = "";

        [Header("Working range (metres from the nozzle)")]
        public float sweetMin = 0.15f;
        public float sweetMax = 0.25f;
        [Tooltip("Paint fades to nothing here (dry spray).")]
        public float maxDistance = 0.40f;

        [Header("Spray pattern")]
        [Tooltip("Full cone angle at start, degrees.")]
        public float coneAngle = 20f;
        [Tooltip("Thumbstick widens / narrows the spray (gun fan control). Off = fixed nozzle (can).")]
        public bool coneAdjustable = true;
        public float minConeAngle = 8f;
        public float maxConeAngle = 35f;

        [Header("Film thickness")]
        [Tooltip("Converts simulated paint to real dry film thickness, and the target range for a finished job.")]
        public FilmSpec film = FilmSpec.Default;

        [Header("Paint")]
        [Tooltip("Coat added per stamp at the sweet spot with the default cone (see SprayBrushData).")]
        [Range(0f, 1f)] public float flowAmount = 0.45f;

        [Header("Shaking (aerosol can)")]
        [Tooltip("Must be shaken before use; unshaken it sputters.")]
        public bool requiresShaking;
        [Tooltip("Mix gained per shake stroke (0..1).")]
        [Range(0.01f, 1f)] public float mixPerShake = 0.08f;
        [Tooltip("Seconds for a fully mixed can to settle back (unmixed) when not shaken.")]
        public float settleSeconds = 90f;
        [Tooltip("Mixing ball clacks, one per shake stroke (picked at random so shaking doesn't sound repetitive).")]
        public AudioClip[] rattleClips = new AudioClip[0];

        [Header("Sound")]
        public AudioClip hissLoop;
        public AudioClip sprayStart;
        public AudioClip sprayStop;
        public AudioClip triggerClick;
        [Range(0f, 1f)] public float hissVolume = 0.55f;
        [Tooltip("Hiss pitch at the widest and tightest cone.")]
        public Vector2 pitchRange = new Vector2(0.9f, 1.18f);
    }
}
