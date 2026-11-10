using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// Points at the next safety task while the safety step runs: a floating arrow and label over the respirator,
    /// then the goggles, then the booth fan switch. Hidden when everything is done or no safety step is running.
    /// </summary>
    public class SafetyGuide : MonoBehaviour
    {
        [SerializeField] private SafetyGear safety;
        [SerializeField] private Transform respirator;
        [SerializeField] private Transform goggles;
        [SerializeField] private Transform fanSwitch;
        [Tooltip("Moves over the current target; the bobbing arrow + label is its child.")]
        [SerializeField] private Transform marker;
        [SerializeField] private Text label;
        [Tooltip("Height of the arrow above an item on the table.")]
        [SerializeField] private float aboveItem = 0.16f;
        [Tooltip("Height of the arrow above the fan switch sign.")]
        [SerializeField] private float aboveSwitch = 0.12f;

        private Transform target;

        private void LateUpdate()
        {
            Transform next = null;
            string text = null;
            float above = aboveItem;
            if (safety.GuideActive)
            {
                if (!safety.RespiratorWorn) { next = respirator; text = "1  Respirator"; }
                else if (!safety.GogglesWorn && goggles != null) { next = goggles; text = "2  Goggles"; }
                else if (!safety.FanOn) { next = fanSwitch; text = safety.FullFace ? "2  Fan switch" : "3  Fan switch"; above = aboveSwitch; }
            }

            bool show = next != null;
            if (marker.gameObject.activeSelf != show)
                marker.gameObject.SetActive(show);
            if (!show)
                return;

            if (next != target)
            {
                target = next;
                label.text = text;
            }

            // Over the target, centred on its renderers.
            Vector3 top = next.position;
            Renderer[] renderers = next.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    b.Encapsulate(renderers[i].bounds);
                top = new Vector3(b.center.x, b.max.y, b.center.z);
            }
            marker.position = top + Vector3.up * above;
        }
    }
}
