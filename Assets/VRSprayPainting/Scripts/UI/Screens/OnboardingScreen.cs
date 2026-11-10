using System;
using DataBindingFramework;
using ServiceLocatorFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// World-space guide panel. Binds to the Onboarding.* properties and sends button presses
    /// through the observer manager; holds no onboarding logic itself.
    /// </summary>
    public class OnboardingScreen : UISystem.Screen
    {
        [SerializeField] private TMP_Text stepLabel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [Tooltip("One Image per step, left to right. Extra dots are hidden.")]
        [SerializeField] private Image[] stepDots = new Image[0];
        [SerializeField] private GameObject inputHintRoot;
        [SerializeField] private TMP_Text inputHintText;
        [SerializeField] private GameObject progressRoot;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button primaryButton;
        [SerializeField] private TMP_Text primaryLabel;
        [SerializeField] private Button skipButton;
        [Tooltip("Tinted accent while working on a step, success colour when it is done.")]
        [SerializeField] private Graphic[] accentGraphics = new Graphic[0];
        [SerializeField] private Color accentColor = new Color(0.98f, 0.55f, 0.1f);
        [SerializeField] private Color successColor = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color dotDoneColor = new Color(0.98f, 0.55f, 0.1f, 0.45f);
        [SerializeField] private Color dotTodoColor = new Color(1f, 1f, 1f, 0.15f);

        private IObserverManager observerManager;
        private int stepIndex;
        private int stepCount;
        private bool succeeded;

        private void Start()
        {
            IPropertyManager propertyManager = ServiceLocator.Current.Get<IPropertyManager>();
            observerManager = ServiceLocator.Current.Get<IObserverManager>();

            Bind<string>(propertyManager, PropertyNameConstants.OnboardingStepLabel, value => stepLabel.text = value);
            Bind<string>(propertyManager, PropertyNameConstants.OnboardingTitle, value => titleText.text = value);
            Bind<string>(propertyManager, PropertyNameConstants.OnboardingBody, value => bodyText.text = value);
            Bind<string>(propertyManager, PropertyNameConstants.OnboardingPrimaryLabel, OnPrimaryLabelChanged);
            Bind<string>(propertyManager, PropertyNameConstants.OnboardingInputHint, OnInputHintChanged);
            Bind<float>(propertyManager, PropertyNameConstants.OnboardingProgress, OnProgressChanged);
            Bind<int>(propertyManager, PropertyNameConstants.OnboardingStepIndex, value => { stepIndex = value; RefreshDots(); });
            Bind<int>(propertyManager, PropertyNameConstants.OnboardingStepCount, value => { stepCount = value; RefreshDots(); });
            Bind<bool>(propertyManager, PropertyNameConstants.OnboardingSucceeded, OnSucceededChanged);
            Bind<bool>(propertyManager, PropertyNameConstants.OnboardingVisible, OnVisibleChanged);

            primaryButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.OnboardingPrimaryPressed).Notify());
            skipButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.OnboardingSkipPressed).Notify());
        }

        // Applies the current value immediately so it doesn't matter who started first.
        private void Bind<T>(IPropertyManager propertyManager, string key, Action<T> onChanged)
        {
            Property<T> property = propertyManager.GetOrCreateProperty<T>(key);
            property.Bind(this, onChanged);
            onChanged(property.Value);
        }

        private void OnPrimaryLabelChanged(string label)
        {
            bool hasLabel = !string.IsNullOrEmpty(label);
            primaryButton.gameObject.SetActive(hasLabel);
            if (hasLabel)
                primaryLabel.text = label;
        }

        private void OnInputHintChanged(string hint)
        {
            bool hasHint = !string.IsNullOrEmpty(hint);
            inputHintRoot.SetActive(hasHint);
            if (hasHint)
                inputHintText.text = hint;
        }

        private void OnProgressChanged(float progress)
        {
            // Negative = this step has no progress bar.
            progressRoot.SetActive(progress >= 0f);
            // Bar length by anchors: Image.Filled needs a sprite and draws full without one.
            RectTransform fill = progressFill.rectTransform;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
        }

        private void OnSucceededChanged(bool value)
        {
            succeeded = value;
            Color color = value ? successColor : accentColor;
            foreach (Graphic graphic in accentGraphics)
                if (graphic != null)
                    graphic.color = color;
            RefreshDots();
        }

        private void RefreshDots()
        {
            for (int i = 0; i < stepDots.Length; i++)
            {
                Image dot = stepDots[i];
                if (dot == null)
                    continue;

                dot.gameObject.SetActive(i < stepCount);
                bool current = i == stepIndex;
                dot.color = current ? (succeeded ? successColor : accentColor) : i < stepIndex ? dotDoneColor : dotTodoColor;
                dot.rectTransform.localScale = current ? new Vector3(1.6f, 1f, 1f) : Vector3.one;
            }
        }

        private void OnVisibleChanged(bool visible)
        {
            if (visible)
                Show();
            else
                Hide();
        }
    }
}
