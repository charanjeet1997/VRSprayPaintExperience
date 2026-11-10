using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Coach/Distance Reading", fileName = "Reading_Distance")]
    public class DistanceReading : CoachReading
    {
        public override bool TryRead(CoachContext ctx, out CoachValue reading)
        {
            reading = default;
            if (!Aimed(ctx.painter))
                return false;
            string cm = $"{ctx.painter.AimDistance * 100f:0} cm";
            reading = ctx.painter.Zone switch
            {
                SprayZone.Sweet => new CoachValue { value = cm, hint = "Good distance", state = ReadingState.Good },
                SprayZone.Wet => new CoachValue { value = cm, hint = "Too close: back off", state = ReadingState.Low },
                SprayZone.Dry => new CoachValue { value = cm, hint = "Too far: move in", state = ReadingState.High },
                _ => new CoachValue { value = cm, hint = "Out of range", state = ReadingState.Bad },
            };
            return true;
        }
    }
}
