using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRSprayPainting
{
    /// <summary>
    /// Protective gear (respirator, goggles): pick it up and bring it to your face to put it on. It then
    /// disappears from the hand (worn), with a click and a buzz. PutBack returns it to the bench.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class PpeItem : MonoBehaviour
    {
        [SerializeField] private string itemName = "Respirator";
        [Tooltip("Distance to the headset (metres) that counts as putting it on.")]
        [SerializeField] private float wearDistance = 0.22f;
        [Tooltip("Also protects the eyes (full-face respirator with a visor): no separate goggles needed.")]
        [SerializeField] private bool protectsEyes;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip equipClip;

        private XRGrabInteractable grab;
        private Rigidbody body;
        private Vector3 homePosition;
        private Quaternion homeRotation;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor holder;
        private bool startKinematic;

        public string ItemName => itemName;
        public bool IsWorn { get; private set; }
        public bool ProtectsEyes => protectsEyes;

        public event Action<PpeItem> Equipped;

        private void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
            homePosition = transform.position;
            homeRotation = transform.rotation;
            startKinematic = body != null && body.isKinematic;
        }

        private void OnEnable()
        {
            grab.selectEntered.AddListener(OnGrabbed);
            grab.selectExited.AddListener(OnReleased);
        }

        private void OnDisable()
        {
            grab.selectEntered.RemoveListener(OnGrabbed);
            grab.selectExited.RemoveListener(OnReleased);
        }

        private void OnGrabbed(SelectEnterEventArgs args) => holder = args.interactorObject;
        private void OnReleased(SelectExitEventArgs args) => holder = null;

        private void Update()
        {
            if (IsWorn || holder == null)
                return;

            Camera cam = Camera.main;
            if (cam != null && Vector3.Distance(transform.position, cam.transform.position) < wearDistance)
                Wear();
        }

        private void Wear()
        {
            IsWorn = true;
            if (holder != null && grab.interactionManager != null)
                grab.interactionManager.SelectExit(holder, grab);
            if (holder is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor input)
                input.SendHapticImpulse(0.5f, 0.08f);
            holder = null;

            if (audioSource != null && equipClip != null)
                AudioSource.PlayClipAtPoint(equipClip, transform.position, 0.8f);

            SetVisible(false);
            Equipped?.Invoke(this);
        }

        /// <summary>Back on the bench, not worn.</summary>
        public void PutBack()
        {
            IsWorn = false;
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.position = homePosition;
                body.rotation = homeRotation;
            }
            transform.SetPositionAndRotation(homePosition, homeRotation);
            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
                r.enabled = visible;
            foreach (Collider c in GetComponentsInChildren<Collider>(true))
                c.enabled = visible;
            if (body != null)
                body.isKinematic = !visible || startKinematic;
        }
    }
}
