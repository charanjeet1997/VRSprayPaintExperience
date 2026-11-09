using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Floating world hint: bobs up and down and turns (yaw only) to face the headset.
    /// </summary>
    public class HintBob : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.02f;
        [SerializeField] private float speed = 2.5f;

        private Vector3 startPosition;

        private void Awake() => startPosition = transform.localPosition;

        private void LateUpdate()
        {
            transform.localPosition = startPosition + Vector3.up * (Mathf.Sin(Time.time * speed) * amplitude);

            Camera cam = Camera.main;
            if (cam == null)
                return;

            Vector3 away = transform.position - cam.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(away);
        }
    }
}
