using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// Film thickness on the report: one bar split into bare / thin / on target / heavy (share of the panel),
    /// with the average dry film thickness against the target in µm.
    /// </summary>
    public class FilmBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform bare;
        [SerializeField] private RectTransform thin;
        [SerializeField] private RectTransform onTarget;
        [SerializeField] private RectTransform heavy;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text legendText;

        public void Show(CoatStats coat)
        {
            float x = 0f;
            x = Segment(bare, x, coat.missingFraction);
            x = Segment(thin, x, coat.thinFraction);
            x = Segment(onTarget, x, coat.inTargetFraction);
            Segment(heavy, x, coat.thickFraction);

            FilmSpec film = coat.film;
            summaryText.text = $"Average {coat.averageFilm:0} µm   ·   target {film.target.x:0}-{film.target.y:0} µm   ·   {coat.inTargetFraction * 100f:0}% on target";
            legendText.text =
                $"<color=#8a8f98>■</color> bare {coat.missingFraction * 100f:0}%   " +
                $"<color=#4f8cff>■</color> thin {coat.thinFraction * 100f:0}%   " +
                $"<color=#4fd16a>■</color> on target {coat.inTargetFraction * 100f:0}%   " +
                $"<color=#ff5a45>■</color> heavy {coat.thickFraction * 100f:0}%";
        }

        // Places one segment from x (0..1) with the given width; returns the new x.
        private static float Segment(RectTransform segment, float x, float width)
        {
            width = Mathf.Clamp01(width);
            segment.anchorMin = new Vector2(x, 0f);
            segment.anchorMax = new Vector2(Mathf.Min(1f, x + width), 1f);
            segment.offsetMin = segment.offsetMax = Vector2.zero;
            segment.gameObject.SetActive(width > 0.001f);
            return x + width;
        }
    }
}
