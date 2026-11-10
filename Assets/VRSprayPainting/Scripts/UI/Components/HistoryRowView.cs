using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>One attempt in the history list: when, what, sprayer, score and pass/fail.</summary>
    public class HistoryRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text activityText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Color passColor = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color failColor = new Color(0.95f, 0.35f, 0.25f);

        public void Show(AttemptRecord record)
        {
            gameObject.SetActive(true);
            dateText.text = DateTime.TryParse(record.timestamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime when)
                ? when.ToString("dd MMM  HH:mm", CultureInfo.InvariantCulture)
                : record.timestamp;
            activityText.text = $"{record.activity}  <size=75%><color=#9aa0a8>{record.sprayer} · {record.coats} coat{(record.coats == 1 ? "" : "s")} · {record.averageFilm} µm</color></size>";
            scoreText.text = $"{record.score}  {(record.passed ? "PASS" : "FAIL")}";
            scoreText.color = record.passed ? passColor : failColor;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
