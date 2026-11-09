using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Applies the colour picked on the palette to the spray painter and the gun visuals,
    /// and publishes the current colour for the UI.
    /// </summary>
    public class PaintSettingsController : MonoBehaviour
    {
        [SerializeField] private SprayPainter sprayPainter;
        [SerializeField] private SprayGun sprayGun;

        private IPropertyManager propertyManager;

        private void Start()
        {
            propertyManager = ServiceLocator.Current.Get<IPropertyManager>();
            ServiceLocator.Current.Get<IObserverManager>()
                .GetOrCreateObserver<Color>(ObserverNameConstants.PaintColorSelected)
                .Bind(this, ApplyColor);

            ApplyColor(sprayPainter.Color);
        }

        private void ApplyColor(Color color)
        {
            sprayPainter.SetColor(color);
            sprayGun.SetSprayColor(color);
            propertyManager.GetOrCreateProperty<Color>(PropertyNameConstants.PaintColor).Value = color;
        }
    }
}
