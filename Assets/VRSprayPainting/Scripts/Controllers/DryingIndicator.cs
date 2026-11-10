using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// Small sign above the panel: "WET · 14 s until dry" while the coat dries, then "DRY · ready for coat 2".
    /// Real spray paint must dry between coats; spraying again on wet paint makes it run.
    /// </summary>
    public class DryingIndicator : MonoBehaviour
    {
        [SerializeField] private PaintableSurface surface;
        [SerializeField] private SprayTracker tracker;
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image background;
        [SerializeField] private Color wetColor = new Color(0.98f, 0.62f, 0.15f);
        [SerializeField] private Color dryColor = new Color(0.3f, 0.8f, 0.4f);

        private int shownSeconds = -1;
        private int shownState = -1;

        private void LateUpdate()
        {
            bool painted = tracker.CoatCount > 0 && !float.IsInfinity(surface.LastPaintTime);
            if (canvas.enabled != painted)
                canvas.enabled = painted;
            if (!painted)
            {
                shownState = -1;
                return;
            }

            float remaining = TrainingSettings.DryTime - (Time.time - surface.LastPaintTime);
            if (remaining > 0f)
            {
                int seconds = Mathf.CeilToInt(remaining);
                if (shownState == 0 && seconds == shownSeconds)
                    return;
                shownState = 0;
                shownSeconds = seconds;
                label.text = $"WET · {seconds} s until dry";
                Tint(wetColor);
            }
            else if (shownState != 1 || shownSeconds != tracker.CoatCount)
            {
                shownState = 1;
                shownSeconds = tracker.CoatCount;
                label.text = $"DRY · ready for coat {tracker.CoatCount + 1}";
                Tint(dryColor);
            }
        }

        private void Tint(Color color)
        {
            label.color = color;
            if (background != null)
                background.color = new Color(color.r * 0.12f, color.g * 0.12f, color.b * 0.12f, 0.9f);
        }
    }
}
