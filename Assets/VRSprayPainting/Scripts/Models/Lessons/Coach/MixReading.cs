using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>How well the spray can is mixed; shown whether or not it is aimed (you shake it in the air).</summary>
    [CreateAssetMenu(menuName = "VR Spray Painting/Coach/Mix Reading", fileName = "Reading_Mix")]
    public class MixReading : CoachReading
    {
        [Tooltip("Mix that sprays smoothly.")]
        [SerializeField, Range(0f, 1f)] private float readyMix = 0.85f;

        public override bool TryRead(CoachContext ctx, out CoachValue reading)
        {
            reading = default;
            if (ctx.shaker == null || !ctx.shaker.RequiresShaking)
                return false;

            float mix = ctx.shaker.Mix;
            string value = $"{mix * 100f:0}%";
            reading = mix >= readyMix
                ? new CoachValue { value = value, hint = "Mixed: ready to spray", state = ReadingState.Good }
                : mix >= 0.4f
                    ? new CoachValue { value = value, hint = "Keep shaking", state = ReadingState.High }
                    : new CoachValue { value = value, hint = "Shake the can!", state = ReadingState.Bad };
            return true;
        }
    }
}
