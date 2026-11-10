using DataBindingFramework;
using ServiceLocatorFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// Live reading that floats just in front of and above the gun (toward the eyes) during practice: the CoachReading asset the current practice step uses (angle, distance, ...),
    /// green when right, coloured when wrong, with a word saying what to change. Faces the headset.
    /// Which reading shows comes from the Lesson.CoachReading property (null = hidden).
    /// </summary>
    public class SprayCoachReadout : MonoBehaviour
    {
        [SerializeField] private SprayGun sprayGun;
        [SerializeField] private SprayPainter sprayPainter;
        [SerializeField] private SprayTracker tracker;
        [SerializeField] private SprayCanShaker shaker;
        [SerializeField] private Canvas readoutCanvas;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text hintText;
        [Tooltip("Pulled from the gun toward the eyes (metres), so the gun and hand never hide it.")]
        [SerializeField] private float towardEyes = 0.12f;
        [Tooltip("Raised above the gun (metres), out of the line of sight to the spray spot.")]
        [SerializeField] private float above = 0.09f;

        [SerializeField] private Color goodColor = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color closeColor = new Color(1f, 0.55f, 0.1f);
        [SerializeField] private Color farColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color badColor = new Color(0.95f, 0.35f, 0.25f);

        private CoachReading reading;

        private void Start()
        {
            Property<CoachReading> property = ServiceLocator.Current.Get<IPropertyManager>()
                .GetOrCreateProperty<CoachReading>(PropertyNameConstants.CoachReading);
            property.Bind(this, value => reading = value);
            reading = property.Value;
            readoutCanvas.enabled = false;
        }

        private void LateUpdate()
        {
            // Readings decide for themselves when they have something to show (e.g. mix shows while not aiming).
            bool visible = reading != null && sprayGun.IsHeld && TrainingSettings.CuesEnabled
                           && reading.TryRead(new CoachContext { painter = sprayPainter, tracker = tracker, shaker = shaker }, out CoachValue value)
                           && Apply(value);
            if (readoutCanvas.enabled != visible)
                readoutCanvas.enabled = visible;
            if (visible)
                Place();
        }

        // Between the gun and the eyes, a little above: always in front of the gun and the hand holding it.
        private void Place()
        {
            Transform t = readoutCanvas.transform;
            Vector3 gun = sprayGun.transform.position;
            Camera cam = Camera.main;
            if (cam == null)
            {
                t.position = gun + Vector3.up * above;
                return;
            }

            Transform eye = cam.transform;
            Vector3 toEye = eye.position - gun;
            float reach = toEye.magnitude;
            Vector3 dir = reach > 1e-4f ? toEye / reach : -eye.forward;
            // Never closer than 15 cm to the eyes (hard to focus).
            float pull = Mathf.Min(towardEyes, Mathf.Max(reach - 0.15f, 0f));
            t.position = gun + dir * pull + eye.up * above;
            t.rotation = Quaternion.LookRotation(t.position - eye.position, eye.up);
        }

        private bool Apply(CoachValue value)
        {
            Color color = value.state switch
            {
                ReadingState.Good => goodColor,
                ReadingState.Low => closeColor,
                ReadingState.High => farColor,
                _ => badColor,
            };
            Apply(value.value, value.hint, color);
            return true;
        }

        private void Apply(string value, string hint, Color color)
        {
            valueText.text = value;
            valueText.color = color;
            hintText.text = hint;
            background.color = new Color(color.r * 0.15f, color.g * 0.15f, color.b * 0.15f, 0.92f);
        }
    }
}
