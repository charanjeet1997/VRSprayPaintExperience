using System.Collections.Generic;
using DataBindingFramework;
using ServiceLocatorFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// Attempt history: totals (attempts, passed, average, trend of the last five against the five before), the
    /// most recent attempts and a CSV export for instructors.
    /// </summary>
    public class HistoryScreen : UISystem.Screen
    {
        [SerializeField] private TMP_Text summaryText;
        [Tooltip("Rows, newest first. Extra rows are hidden.")]
        [SerializeField] private HistoryRowView[] rows = new HistoryRowView[0];
        [SerializeField] private TMP_Text emptyText;
        [SerializeField] private Button exportButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button backButton;

        private void Start()
        {
            IObserverManager observers = ServiceLocator.Current.Get<IObserverManager>();
            Property<bool> visible = ServiceLocator.Current.Get<IPropertyManager>().GetOrCreateProperty<bool>(PropertyNameConstants.HistoryVisible);
            visible.Bind(this, OnVisible);
            OnVisible(visible.Value);

            exportButton.onClick.AddListener(Export);
            backButton.onClick.AddListener(() => observers.GetOrCreateObserver(ObserverNameConstants.MenuRequested).Notify());
        }

        private void OnVisible(bool visible)
        {
            if (visible)
            {
                statusText.text = "";
                Refresh();
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void Refresh()
        {
            IReadOnlyList<AttemptRecord> all = AttemptHistory.Attempts;
            int passed = 0;
            float sum = 0f;
            foreach (AttemptRecord a in all)
            {
                if (a.passed) passed++;
                sum += a.score;
            }

            emptyText.gameObject.SetActive(all.Count == 0);
            summaryText.text = all.Count == 0
                ? ""
                : $"{all.Count} attempts   ·   {passed} passed   ·   average {Mathf.RoundToInt(sum / all.Count)}   ·   {Trend(all)}";

            for (int i = 0; i < rows.Length; i++)
            {
                int index = all.Count - 1 - i; // newest first
                if (index >= 0)
                    rows[i].Show(all[index]);
                else
                    rows[i].Hide();
            }
        }

        // Average of the last 5 attempts against the 5 before them.
        private static string Trend(IReadOnlyList<AttemptRecord> all)
        {
            if (all.Count < 6)
                return "keep practising to see your trend";
            float recent = 0f, before = 0f;
            int n = Mathf.Min(5, all.Count - 5);
            for (int i = 0; i < 5; i++) recent += all[all.Count - 1 - i].score;
            for (int i = 0; i < n; i++) before += all[all.Count - 6 - i].score;
            float change = recent / 5f - before / n;
            return change >= 0.5f ? $"<color=#4fd16a>▲ {change:0} recently</color>"
                : change <= -0.5f ? $"<color=#ff5a45>▼ {-change:0} recently</color>"
                : "steady";
        }

        private void Export()
        {
            string path = AttemptHistory.ExportCsv();
            statusText.text = path != null ? $"Saved: {path}" : "Export failed.";
        }
    }
}
