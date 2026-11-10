using DataBindingFramework;
using ServiceLocatorFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// "How to use" guide, one page at a time (controls, pointing at buttons, live cues, colours, safety).
    /// Shown on first launch and from the main menu. Back / Done return to the menu.
    /// </summary>
    public class HowToUseScreen : UISystem.Screen
    {
        [SerializeField] private HelpContent helpContent;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text pageText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text nextLabel;
        [SerializeField] private Button closeButton;

        private IObserverManager observerManager;
        private int page;

        private void Start()
        {
            observerManager = ServiceLocator.Current.Get<IObserverManager>();
            Property<bool> visible = ServiceLocator.Current.Get<IPropertyManager>().GetOrCreateProperty<bool>(PropertyNameConstants.HowToVisible);
            visible.Bind(this, OnVisible);
            OnVisible(visible.Value);

            previousButton.onClick.AddListener(() => ShowPage(page - 1));
            nextButton.onClick.AddListener(() =>
            {
                if (page + 1 < helpContent.pages.Count) ShowPage(page + 1);
                else Close();
            });
            closeButton.onClick.AddListener(Close);
        }

        private void OnVisible(bool visible)
        {
            if (visible)
            {
                ShowPage(0);
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void ShowPage(int index)
        {
            if (helpContent == null || helpContent.pages.Count == 0)
                return;
            page = Mathf.Clamp(index, 0, helpContent.pages.Count - 1);
            HelpContent.Page p = helpContent.pages[page];
            titleText.text = p.title;
            bodyText.text = p.body;
            pageText.text = $"{page + 1} / {helpContent.pages.Count}";
            previousButton.gameObject.SetActive(page > 0);
            nextLabel.text = page + 1 < helpContent.pages.Count ? "Next" : "Done";
        }

        private void Close() => observerManager.GetOrCreateObserver(ObserverNameConstants.HowToClosed).Notify();
    }
}
