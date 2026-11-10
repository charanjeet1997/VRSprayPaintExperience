using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>Wall switch for the booth's extraction fan: point and press; shows ON / OFF.</summary>
    public class BoothFanSwitch : MonoBehaviour
    {
        [SerializeField] private SafetyGear safety;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private Image lamp;
        [SerializeField] private Color onColor = new Color(0.3f, 0.85f, 0.4f);
        [SerializeField] private Color offColor = new Color(0.9f, 0.25f, 0.2f);

        private void OnEnable()
        {
            safety.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => safety.Changed -= Refresh;

        private void Start() => button.onClick.AddListener(safety.ToggleFan);

        // Blinks while the safety step is waiting for the fan.
        private void Update()
        {
            if (lamp == null || safety.FanOn)
                return;
            float pulse = safety.GuideActive ? 0.55f + 0.45f * Mathf.Sin(Time.time * 6f) : 1f;
            lamp.color = Color.Lerp(offColor * 0.4f, offColor, pulse);
        }

        private void Refresh()
        {
            stateText.text = safety.FanOn ? "BOOTH FAN  ON" : "BOOTH FAN  OFF";
            if (lamp != null)
                lamp.color = safety.FanOn ? onColor : offColor;
        }
    }
}
