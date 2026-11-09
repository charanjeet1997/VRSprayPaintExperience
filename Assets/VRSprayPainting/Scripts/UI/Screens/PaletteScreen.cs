using DataBindingFramework;
using ServiceLocatorFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// World-space palette: colour swatches, clear paint, reopen the guide.
    /// Always visible; reflects the current colour from the Paint.Color property.
    /// </summary>
    public class PaletteScreen : UISystem.Screen
    {
        [SerializeField] private ColorSwatchButton[] swatches;
        [SerializeField] private Image currentColorPreview;
        [SerializeField] private TMP_Text currentColorName;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button guideButton;

        private IObserverManager observerManager;

        private void Start()
        {
            observerManager = ServiceLocator.Current.Get<IObserverManager>();
            Property<Color> paintColor = ServiceLocator.Current.Get<IPropertyManager>().GetOrCreateProperty<Color>(PropertyNameConstants.PaintColor);

            foreach (ColorSwatchButton swatch in swatches)
                swatch.Clicked += OnSwatchClicked;

            clearButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.PaintResetRequested).Notify());
            guideButton.onClick.AddListener(() => observerManager.GetOrCreateObserver(ObserverNameConstants.OnboardingRestartPressed).Notify());

            paintColor.Bind(this, OnPaintColorChanged);
            OnPaintColorChanged(paintColor.Value);

            Show();
        }

        private void OnSwatchClicked(Color color) =>
            observerManager.GetOrCreateObserver<Color>(ObserverNameConstants.PaintColorSelected).Notify(color);

        private void OnPaintColorChanged(Color color)
        {
            currentColorPreview.color = color;
            string name = "Custom";
            foreach (ColorSwatchButton swatch in swatches)
            {
                bool selected = swatch.Color == color;
                swatch.SetSelected(selected);
                if (selected)
                    name = swatch.ColorName;
            }

            if (currentColorName != null)
                currentColorName.text = name;
        }
    }
}
