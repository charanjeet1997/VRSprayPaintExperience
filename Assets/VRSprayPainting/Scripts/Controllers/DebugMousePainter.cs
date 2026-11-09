using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

namespace VRSprayPainting
{
    /// <summary>
    /// Temporary editor test input for the paint pipeline (before XR is connected).
    /// Hold left mouse to paint where the cursor hits (R resets, via PaintResetInput).
    /// Remove or disable once SprayGun drives painting.
    /// </summary>
    public class DebugMousePainter : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Color color = new Color(0.85f, 0.1f, 0.1f);
        [SerializeField, Range(0.005f, 0.3f)] private float radius = 0.04f;
        [SerializeField, Range(0f, 1f)] private float amount = 0.15f;
        [SerializeField] private float stampInterval = 1f / 30f;
        [SerializeField] private float maxDistance = 10f;

        private float nextStampTime;

        private void Start()
        {
            if (viewCamera == null)
                return;

            // Never render the desktop camera into the headset, even for the frames before XR reports active.
            if (viewCamera.TryGetComponent(out UniversalAdditionalCameraData urpData))
                urpData.allowXRRendering = false;
            viewCamera.gameObject.SetActive(!XRSettings.isDeviceActive);
        }

        private void Update()
        {
            // OpenXR over Link can start its session after Start, so keep checking.
            if (viewCamera != null && viewCamera.gameObject.activeSelf && XRSettings.isDeviceActive)
                viewCamera.gameObject.SetActive(false);

            Mouse mouse = Mouse.current;

            if (mouse == null || !mouse.leftButton.isPressed || Time.time < nextStampTime)
                return;

            nextStampTime = Time.time + stampInterval;

            Camera cam = viewCamera != null && viewCamera.isActiveAndEnabled ? viewCamera : Camera.main;
            if (cam == null)
                return;

            Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance) && hit.collider.TryGetComponent(out IPaintable paintable))
                paintable.ApplyPaint(PaintStamp.Circle(hit.textureCoord, color, radius, amount));
        }
    }
}
