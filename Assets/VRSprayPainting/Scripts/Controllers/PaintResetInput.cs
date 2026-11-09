using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VRSprayPainting
{
    /// <summary>
    /// Resets every paintable surface: B / Y on the Quest controllers, R on the keyboard,
    /// or the palette's "Clear paint" button (PaintResetRequested observer).
    /// Publishes PaintResetPerformed afterwards.
    /// </summary>
    public class PaintResetInput : MonoBehaviour
    {
        private InputAction resetAction;
        private IObserverManager observerManager;

        private void OnEnable()
        {
            resetAction = new InputAction("ResetPaint", InputActionType.Button);
            resetAction.AddBinding("<XRController>{RightHand}/secondaryButton");
            resetAction.AddBinding("<XRController>{LeftHand}/secondaryButton");
            resetAction.AddBinding("<Keyboard>/r");
            resetAction.performed += OnResetAction;
            resetAction.Enable();
        }

        private void Start()
        {
            observerManager = ServiceLocator.Current.Get<IObserverManager>();
            observerManager.GetOrCreateObserver(ObserverNameConstants.PaintResetRequested).Bind(this, ResetAll);
        }

        private void OnDisable()
        {
            resetAction.performed -= OnResetAction;
            resetAction.Dispose();
            resetAction = null;
        }

        public void ResetAll()
        {
            foreach (PaintableSurface surface in FindObjectsByType<PaintableSurface>(FindObjectsSortMode.None))
                surface.ResetPaint();

            observerManager?.GetOrCreateObserver(ObserverNameConstants.PaintResetPerformed).Notify();
        }

        private void OnResetAction(InputAction.CallbackContext context) => ResetAll();
    }
}
