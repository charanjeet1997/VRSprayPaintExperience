using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Ring on the surface where the gun points, sized to the real spray spot.
    /// Green = sweet spot, orange = close (wet), yellow = far (dry), red = too far. Hidden when the nozzle clips into the surface.
    /// Shown only while the gun is held (or flown by the demo) and a paintable surface is within reach.
    /// </summary>
    public class SprayAimMarker : MonoBehaviour
    {
        [SerializeField] private SprayGun sprayGun;
        [SerializeField] private SprayPainter sprayPainter;
        [SerializeField] private Renderer markerRenderer;
        [Tooltip("Lift off the surface to avoid z-fighting.")]
        [SerializeField] private float surfaceOffset = 0.002f;
        [SerializeField] private float minMarkerDiameter = 0.015f;
        [SerializeField, Range(0f, 1f)] private float opacity = 0.75f;
        [Tooltip("Dimmed while spraying so it doesn't hide the paint.")]
        [SerializeField, Range(0f, 1f)] private float sprayingOpacity = 0.3f;

        [SerializeField] private Color sweetColor = new Color(0.25f, 1f, 0.35f);
        [SerializeField] private Color wetColor = new Color(1f, 0.55f, 0.1f);
        [SerializeField] private Color dryColor = new Color(1f, 0.9f, 0.2f);
        [SerializeField] private Color outOfRangeColor = new Color(1f, 0.2f, 0.2f);

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock block;
        private DataBindingFramework.Property<bool> cuesAllowed;

        private void LateUpdate()
        {
            cuesAllowed ??= ServiceLocatorFramework.ServiceLocator.Current.Get<DataBindingFramework.IPropertyManager>()
                .GetOrCreateProperty<bool>(PropertyNameConstants.CuesAllowed);
            // The demo always shows it; the trainee sees it when cues are on (instructor setting, not in tests).
            bool cues = sprayGun.IsAutopilot || (TrainingSettings.CuesEnabled && cuesAllowed.Value);
            bool visible = cues && (sprayGun.IsHeld || sprayGun.IsAutopilot) && sprayPainter.Zone != SprayZone.NoSurface && sprayPainter.Zone != SprayZone.Clipping;
            if (markerRenderer.enabled != visible)
                markerRenderer.enabled = visible;
            if (!visible)
                return;

            Transform t = markerRenderer.transform;
            t.SetPositionAndRotation(
                sprayPainter.AimPoint + sprayPainter.AimNormal * surfaceOffset,
                Quaternion.LookRotation(-sprayPainter.AimNormal, Mathf.Abs(sprayPainter.AimNormal.y) < 0.9f ? Vector3.up : Vector3.forward));
            float diameter = Mathf.Max(sprayPainter.SpotRadiusAt(sprayPainter.AimDistance) * 2f, minMarkerDiameter);
            t.localScale = new Vector3(diameter, diameter, diameter);

            Color color = sprayPainter.Zone switch
            {
                SprayZone.Sweet => sweetColor,
                SprayZone.Wet => wetColor,
                SprayZone.Dry => dryColor,
                _ => outOfRangeColor,
            };
            color.a *= sprayGun.IsSpraying ? sprayingOpacity : opacity;

            block ??= new MaterialPropertyBlock();
            markerRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            markerRenderer.SetPropertyBlock(block);
        }
    }
}
