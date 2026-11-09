using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRSprayPainting
{
    /// <summary>
    /// Puts a dropped grabbable back where it started (the stool) so the user never has to
    /// pick it up off the floor.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class ReturnHome : MonoBehaviour
    {
        [SerializeField] private float returnDelay = 2.5f;
        [Tooltip("Return only if it ended up this far from home.")]
        [SerializeField] private float minDistanceFromHome = 0.3f;
        [Tooltip("Below this height it is treated as dropped on the floor.")]
        [SerializeField] private float floorHeight = 0.35f;

        private XRGrabInteractable grabInteractable;
        private Rigidbody body;
        private Vector3 homePosition;
        private Quaternion homeRotation;
        private float releasedTime = -1f;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
            homePosition = transform.position;
            homeRotation = transform.rotation;
        }

        private void OnEnable()
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }

        private void OnDisable()
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }

        private void OnGrabbed(SelectEnterEventArgs args) => releasedTime = -1f;
        private void OnReleased(SelectExitEventArgs args) => releasedTime = Time.time;

        private void Update()
        {
            if (grabInteractable.isSelected || releasedTime < 0f)
                return;

            bool lost = Vector3.Distance(transform.position, homePosition) > minDistanceFromHome
                        || transform.position.y < floorHeight;

            if (!lost)
            {
                // Came to rest near home: it was put down on purpose. Still moving: keep watching.
                if (body.IsSleeping() || body.linearVelocity.sqrMagnitude < 0.0025f)
                    releasedTime = -1f;
                return;
            }

            if (Time.time - releasedTime >= returnDelay)
                Return();
        }

        private void Return()
        {
            releasedTime = -1f;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = homePosition;
            body.rotation = homeRotation;
            transform.SetPositionAndRotation(homePosition, homeRotation);
        }
    }
}
