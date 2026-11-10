using System;
using System.Text;
using DataBindingFramework;
using ServiceLocatorFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// World-space scorecard, two pages:
    /// Summary = overall score vs pass mark, coats, film thickness against the target, defects with deductions,
    /// what to fix. Technique = a bar per skill, a score per coat and the distance/angle timeline.
    /// Binds to the Assessment.* properties; buttons go out through the observer manager.
    /// </summary>
    public class AssessmentReportScreen : UISystem.Screen
    {
        [Header("Header")]
        [SerializeField] private TMP_Text headlineText;
        [SerializeField] private TMP_Text overallText;
        [SerializeField] private Image overallBadge;
        [SerializeField] private TMP_Text subtitleText;

        [Header("Summary page")]
        [SerializeField] private GameObject summaryPage;
        [SerializeField] private FilmBarView filmBar;
        [SerializeField] private DefectRowView[] defectRows = new DefectRowView[0];
        [SerializeField] private TMP_Text feedbackText;

        [Header("Technique page")]
        [SerializeField] private GameObject techniquePage;
        [SerializeField] private ScoreBarView[] bars = new ScoreBarView[0];
        [SerializeField] private TMP_Text coatsText;
        [SerializeField] private TechniqueGraphView graph;

        [Header("Buttons")]
        [SerializeField] private Button pageButton;
        [SerializeField] private TMP_Text pageLabel;
        [SerializeField] private Button thicknessButton;
        [SerializeField] private TMP_Text thicknessLabel;
        [SerializeField] private Button closeButton;

        [SerializeField] private Color goodColor = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color okColor = new Color(0.98f, 0.75f, 0.15f);
        [SerializeField] private Color badColor = new Color(0.95f, 0.35f, 0.25f);

        private IObserverManager observerManager;
        private bool showingTechnique;
        private AssessmentResult current;

        private void Start()
        {
            IPropertyManager propertyManager = ServiceLocator.Current.Get<IPropertyManager>();
            observerManager = ServiceLocator.Current.Get<IObserverManager>();

            Bind<AssessmentResult>(propertyManager, PropertyNameConstants.AssessmentResult, ShowResult);
            Bind<bool>(propertyManager, PropertyNameConstants.AssessmentThicknessView,
                on => thicknessLabel.text = on ? "Hide map" : "Thickness map");
            Bind<bool>(propertyManager, PropertyNameConstants.AssessmentVisible, visible =>
            {
                if (visible)
                {
                    SetPage(false);
                    Show();
                }
                else
                {
                    Hide();
                }
            });

            pageButton.onClick.AddListener(() => SetPage(!showingTechnique));
            thicknessButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.AssessmentThicknessToggled).Notify());
            closeButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.AssessmentClosed).Notify());
        }

        private void Bind<T>(IPropertyManager propertyManager, string key, Action<T> onChanged)
        {
            Property<T> property = propertyManager.GetOrCreateProperty<T>(key);
            property.Bind(this, onChanged);
            onChanged(property.Value);
        }

        private void SetPage(bool technique)
        {
            showingTechnique = technique && current != null && current.hasData;
            summaryPage.SetActive(!showingTechnique);
            techniquePage.SetActive(showingTechnique);
            pageLabel.text = showingTechnique ? "◄ Summary" : "Technique ►";
        }

        private void ShowResult(AssessmentResult result)
        {
            if (result == null)
                return;
            current = result;

            headlineText.text = result.headline;
            overallBadge.gameObject.SetActive(result.hasData);
            thicknessButton.gameObject.SetActive(result.hasData);
            pageButton.gameObject.SetActive(result.hasData);
            if (result.hasData)
            {
                overallText.text = Mathf.RoundToInt(result.overall * 100f).ToString();
                overallBadge.color = result.passed ? goodColor : badColor;
                int coats = result.coatScores.Count;
                subtitleText.text = $"Pass mark {Mathf.RoundToInt(result.passMark * 100f)}   ·   {coats} coat{(coats == 1 ? "" : "s")}   ·   technique {Mathf.RoundToInt(result.techniqueScore * 100f)}";
                filmBar.gameObject.SetActive(true);
                filmBar.Show(result.coat);
            }
            else
            {
                subtitleText.text = "";
                filmBar.gameObject.SetActive(false);
            }

            for (int i = 0; i < defectRows.Length; i++)
            {
                if (result.hasData && i < result.defects.Count)
                    defectRows[i].Show(result.defects[i]);
                else
                    defectRows[i].Hide();
            }

            var text = new StringBuilder();
            foreach (string line in result.feedback)
                text.Append("• ").Append(line).Append('\n');
            feedbackText.text = text.ToString().TrimEnd();

            for (int i = 0; i < bars.Length; i++)
            {
                if (result.hasData && i < result.skills.Count)
                    bars[i].Show(result.skills[i], ColorFor(result.skills[i].score));
                else
                    bars[i].Hide();
            }

            var coatsLine = new StringBuilder("Technique per coat:   ");
            for (int i = 0; i < result.coatScores.Count; i++)
                coatsLine.Append($"Coat {i + 1}  <b>{Mathf.RoundToInt(result.coatScores[i] * 100f)}</b>     ");
            coatsText.text = result.coatScores.Count > 0 ? coatsLine.ToString() : "";

            if (result.hasData)
                graph.Show(result);
            SetPage(showingTechnique);
        }

        private Color ColorFor(float score) =>
            score >= 0.8f ? goodColor : score >= 0.5f ? okColor : badColor;
    }
}
