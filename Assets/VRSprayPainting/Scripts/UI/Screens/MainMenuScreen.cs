using System.Collections.Generic;
using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// Main menu: one card per lesson in the catalog (spawned from a prefab into a scrolling list, locked until its
    /// prerequisites are passed), Free Paint and the getting-started tour. Binds Menu.* properties; choices go
    /// out through the observer manager.
    /// </summary>
    public class MainMenuScreen : UISystem.Screen
    {
        [SerializeField] private LessonCardView cardPrefab;
        [Tooltip("Parent of the cards (layout group inside a scroll view).")]
        [SerializeField] private RectTransform cardContainer;
        [Header("Sprayer choice")]
        [SerializeField] private SprayerOptionView optionPrefab;
        [SerializeField] private RectTransform optionContainer;

        [SerializeField] private Button freePaintButton;
        [SerializeField] private Button tourButton;
        [SerializeField] private Button howToButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private Button settingsButton;

        private IObserverManager observerManager;
        private readonly List<LessonCardView> cards = new List<LessonCardView>();
        private readonly List<SprayerOptionView> options = new List<SprayerOptionView>();

        private void Start()
        {
            IPropertyManager propertyManager = ServiceLocator.Current.Get<IPropertyManager>();
            observerManager = ServiceLocator.Current.Get<IObserverManager>();

            freePaintButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.MenuFreePaintSelected).Notify());
            tourButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.MenuTourSelected).Notify());
            howToButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.MenuHowToSelected).Notify());
            historyButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.MenuHistorySelected).Notify());
            settingsButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.MenuSettingsSelected).Notify());

            Property<List<LessonCardInfo>> cardsProperty = propertyManager.GetOrCreateProperty<List<LessonCardInfo>>(PropertyNameConstants.MenuCards);
            cardsProperty.Bind(this, ShowCards);
            ShowCards(cardsProperty.Value);

            Property<List<SprayerOptionInfo>> sprayers = propertyManager.GetOrCreateProperty<List<SprayerOptionInfo>>(PropertyNameConstants.MenuSprayers);
            sprayers.Bind(this, ShowSprayers);
            ShowSprayers(sprayers.Value);

            Property<bool> visible = propertyManager.GetOrCreateProperty<bool>(PropertyNameConstants.MenuVisible);
            visible.Bind(this, OnVisibleChanged);
            OnVisibleChanged(visible.Value);
        }

        private void ShowCards(List<LessonCardInfo> infos)
        {
            int needed = infos?.Count ?? 0;
            while (cards.Count < needed)
            {
                LessonCardView card = Instantiate(cardPrefab, cardContainer);
                card.Clicked += index => observerManager.GetOrCreateObserver<int>(ObserverNameConstants.MenuLessonSelected).Notify(index);
                cards.Add(card);
            }

            for (int i = 0; i < cards.Count; i++)
            {
                if (infos != null && i < infos.Count)
                    cards[i].Show(infos[i]);
                else
                    cards[i].Hide();
            }
        }

        private void ShowSprayers(List<SprayerOptionInfo> infos)
        {
            int needed = infos?.Count ?? 0;
            while (options.Count < needed)
            {
                SprayerOptionView option = Instantiate(optionPrefab, optionContainer);
                option.Clicked += index => observerManager.GetOrCreateObserver<int>(ObserverNameConstants.MenuSprayerSelected).Notify(index);
                options.Add(option);
            }

            for (int i = 0; i < options.Count; i++)
            {
                if (infos != null && i < infos.Count)
                    options[i].Show(infos[i]);
                else
                    options[i].Hide();
            }
        }

        private void OnVisibleChanged(bool visible)
        {
            if (visible) Show();
            else Hide();
        }
    }
}
