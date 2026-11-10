using System;
using DataBindingFramework;
using ServiceLocatorFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// World-space lesson panel. Binds to the Lesson.* properties and sends button presses through the observer
    /// manager; holds no lesson logic itself.
    /// </summary>
    public class LessonScreen : UISystem.Screen
    {
        [SerializeField] private TMP_Text stepLabel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private GameObject progressRoot;
        [SerializeField] private RectTransform progressFill;
        [SerializeField] private Button primaryButton;
        [SerializeField] private TMP_Text primaryLabel;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TMP_Text secondaryLabel;
        [Tooltip("Accent while working, success colour on a passed result.")]
        [SerializeField] private Graphic[] accentGraphics = new Graphic[0];
        [SerializeField] private Color accentColor = new Color(0.98f, 0.55f, 0.1f);
        [SerializeField] private Color successColor = new Color(0.3f, 0.8f, 0.4f);

        private void Start()
        {
            IPropertyManager propertyManager = ServiceLocator.Current.Get<IPropertyManager>();
            IObserverManager observerManager = ServiceLocator.Current.Get<IObserverManager>();

            Bind<string>(propertyManager, PropertyNameConstants.LessonStepLabel, value => stepLabel.text = value);
            Bind<string>(propertyManager, PropertyNameConstants.LessonTitle, value => titleText.text = value);
            Bind<string>(propertyManager, PropertyNameConstants.LessonBody, value => bodyText.text = value);
            Bind<string>(propertyManager, PropertyNameConstants.LessonPrimaryLabel, value => SetButton(primaryButton, primaryLabel, value));
            Bind<string>(propertyManager, PropertyNameConstants.LessonSecondaryLabel, value => SetButton(secondaryButton, secondaryLabel, value));
            Bind<float>(propertyManager, PropertyNameConstants.LessonProgress, OnProgressChanged);
            Bind<bool>(propertyManager, PropertyNameConstants.LessonSucceeded, OnSucceededChanged);
            Bind<bool>(propertyManager, PropertyNameConstants.LessonVisible, visible =>
            {
                if (visible) Show();
                else Hide();
            });

            primaryButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.LessonPrimaryPressed).Notify());
            secondaryButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.LessonSecondaryPressed).Notify());
        }

        private void Bind<T>(IPropertyManager propertyManager, string key, Action<T> onChanged)
        {
            Property<T> property = propertyManager.GetOrCreateProperty<T>(key);
            property.Bind(this, onChanged);
            onChanged(property.Value);
        }

        private static void SetButton(Button button, TMP_Text label, string text)
        {
            bool show = !string.IsNullOrEmpty(text);
            button.gameObject.SetActive(show);
            if (show)
                label.text = text;
        }

        private void OnProgressChanged(float progress)
        {
            // Negative = no bar for this stage.
            progressRoot.SetActive(progress >= 0f);
            progressFill.anchorMin = Vector2.zero;
            progressFill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
            progressFill.offsetMin = progressFill.offsetMax = Vector2.zero;
        }

        private void OnSucceededChanged(bool succeeded)
        {
            Color color = succeeded ? successColor : accentColor;
            foreach (Graphic graphic in accentGraphics)
                if (graphic != null)
                    graphic.color = color;
        }
    }
}
