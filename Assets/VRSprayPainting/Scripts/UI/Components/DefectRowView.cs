using TMPro;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>One defect on the report: name, what was measured and the points it cost.</summary>
    public class DefectRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private Color okColor = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color badColor = new Color(0.95f, 0.35f, 0.25f);

        public void Show(DefectScore defect)
        {
            gameObject.SetActive(true);
            labelText.text = defect.label;
            detailText.text = defect.severity < 0.02f ? "none" : defect.detail;
            bool clean = defect.points < 0.5f;
            pointsText.text = clean ? "OK" : $"-{Mathf.RoundToInt(defect.points)}";
            pointsText.color = clean ? okColor : badColor;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
