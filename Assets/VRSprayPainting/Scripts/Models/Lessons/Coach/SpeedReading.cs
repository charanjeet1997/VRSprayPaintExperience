using UnityEngine;

namespace VRSprayPainting
{
    [CreateAssetMenu(menuName = "VR Spray Painting/Coach/Speed Reading", fileName = "Reading_Speed")]
    public class SpeedReading : CoachReading
    {
        public override bool TryRead(CoachContext ctx, out CoachValue reading)
        {
            reading = default;
            if (!Aimed(ctx.painter))
                return false;

            float speed = ctx.tracker.CurrentSpeed;
            Vector2 good = ctx.tracker.GoodSpeed;
            string value = $"{speed * 100f:0} cm/s";
            if (speed < good.x)
                reading = new CoachValue { value = value, hint = "Too slow: keep moving", state = ReadingState.Low };
            else if (speed > good.y)
                reading = new CoachValue { value = value, hint = "Too fast: slow down", state = ReadingState.High };
            else
                reading = new CoachValue { value = value, hint = "Good pace", state = ReadingState.Good };
            return true;
        }
    }
}
