using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Coach/Angle Reading", fileName = "Reading_Angle")]
    public class AngleReading : CoachReading
    {
        public override bool TryRead(CoachContext ctx, out CoachValue reading)
        {
            reading = default;
            if (!Aimed(ctx.painter))
                return false;
            float tilt = ctx.painter.AimTilt;
            bool ok = tilt <= ctx.tracker.MaxGoodTilt;
            reading = new CoachValue
            {
                value = $"{tilt:0}°",
                hint = ok ? "Straight" : "Tilted: straighten",
                state = ok ? ReadingState.Good : ReadingState.Bad,
            };
            return true;
        }
    }
}
