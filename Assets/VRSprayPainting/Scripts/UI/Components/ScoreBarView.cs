using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>One skill row on the report: name, filled bar, percentage and the measured value.</summary>
    public class ScoreBarView : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Image fill;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text detailText;

        public void Show(SkillScore skill, Color color)
        {
            gameObject.SetActive(true);
            labelText.text = skill.label;
            // Bar length = score (anchor-based, works without a fill sprite).
            RectTransform bar = fill.rectTransform;
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = new Vector2(Mathf.Clamp01(skill.score), 1f);
            bar.offsetMin = bar.offsetMax = Vector2.zero;
            fill.color = color;
            valueText.text = $"{Mathf.RoundToInt(skill.score * 100f)}%";
            valueText.color = color;
            if (detailText != null)
                detailText.text = skill.detail;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
