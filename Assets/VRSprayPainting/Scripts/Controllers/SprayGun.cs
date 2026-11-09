using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRSprayPainting
{
    /// <summary>
    /// VR interaction for the spray gun: grab with XRGrabInteractable, trigger (activate) sprays,
    /// thumbstick up/down on the holding hand tightens/widens the spray cone (precision, like a gun's fan knob).
    /// Drives the VFX cone and a light haptic buzz while spraying.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class SprayGun : MonoBehaviour
    {
        public const float DefaultConeAngle = 20f;

        [SerializeField] private Transform nozzle;
        [Tooltip("VFX Graph spray (droplets + mist). Each exposes SprayColor and SprayBlend.")]
        [SerializeField] private VisualEffect[] sprayEffects = new VisualEffect[0];
        [Tooltip("Tinted with the current paint colour so the user can see what's loaded.")]
        [SerializeField] private Renderer paintCupRenderer;

        [Header("Precision (full cone angle, degrees)")]
        [SerializeField] private float minConeAngle = 8f;
        [SerializeField] private float maxConeAngle = 35f;
        [SerializeField] private float coneAngle = DefaultConeAngle;
        [Tooltip("Degrees per second at full stick.")]
        [SerializeField] private float coneAdjustSpeed = 25f;
        [SerializeField] private InputActionProperty leftThumbstick;
        [SerializeField] private InputActionProperty rightThumbstick;
        [Tooltip("Mist spreads a little wider than the droplets that make the paint.")]
        [SerializeField] private float mistConeScale = 1.25f;

        [Header("Haptics")]
        [SerializeField, Range(0f, 1f)] private float sprayHapticAmplitude = 0.12f;
        [SerializeField, Range(0f, 1f)] private float triggerHapticAmplitude = 0.35f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SprayColorId = Shader.PropertyToID("SprayColor");
        private static readonly int SprayBlendId = Shader.PropertyToID("SprayBlend");

        private XRGrabInteractable grabInteractable;
        private MaterialPropertyBlock cupBlock;
        private XRBaseInputInteractor holder;
        private float appliedCone = -1f;
        private float nextHapticTime;

        public bool IsSpraying { get; private set; }
        public bool IsHeld => holder != null;
        /// <summary>Driven by the intro demo instead of a hand.</summary>
        public bool IsAutopilot { get; private set; }
        public Transform Nozzle => nozzle;
        public float ConeAngle => coneAngle;
        public float MinConeAngle => minConeAngle;
        public float MaxConeAngle => maxConeAngle;

        public event Action SprayStarted;
        public event Action SprayStopped;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();

            // Graphs must stay silent until the trigger is pulled.
            foreach (VisualEffect effect in sprayEffects)
                if (effect != null)
                    effect.Stop();

            ApplyConeToEffects();
        }

        private void OnEnable()
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.activated.AddListener(OnActivated);
            grabInteractable.deactivated.AddListener(OnDeactivated);
            grabInteractable.selectExited.AddListener(OnReleased);
            leftThumbstick.action?.Enable();
            rightThumbstick.action?.Enable();
        }

        private void OnDisable()
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.activated.RemoveListener(OnActivated);
            grabInteractable.deactivated.RemoveListener(OnDeactivated);
            grabInteractable.selectExited.RemoveListener(OnReleased);
            SetSpraying(false);
        }

        private void Update()
        {
            AdjustCone();

            if (IsSpraying && holder != null && Time.time >= nextHapticTime)
            {
                // Overlapping short pulses = continuous buzz that stops cleanly when the trigger is released.
                holder.SendHapticImpulse(sprayHapticAmplitude, 0.12f);
                nextHapticTime = Time.time + 0.1f;
            }
        }

        private void AdjustCone()
        {
            if (holder != null)
            {
                InputAction stick = holder.handedness == InteractorHandedness.Left ? leftThumbstick.action : rightThumbstick.action;
                float y = stick != null ? stick.ReadValue<Vector2>().y : 0f;
                if (Mathf.Abs(y) > 0.2f)
                    coneAngle -= y * coneAdjustSpeed * Time.deltaTime; // push up = tighter, more precise
            }

            coneAngle = Mathf.Clamp(coneAngle, minConeAngle, maxConeAngle);
            if (!Mathf.Approximately(coneAngle, appliedCone))
                ApplyConeToEffects();
        }

        // VelocityDirection blends the nozzle axis with the sphere normal: half-angle ≈ atan((1 - b) / b).
        private void ApplyConeToEffects()
        {
            appliedCone = coneAngle;
            for (int i = 0; i < sprayEffects.Length; i++)
            {
                VisualEffect effect = sprayEffects[i];
                if (effect == null || !effect.HasFloat(SprayBlendId))
                    continue;

                bool mist = effect.name.IndexOf("Mist", StringComparison.OrdinalIgnoreCase) >= 0;
                float half = coneAngle * 0.5f * (mist ? mistConeScale : 1f);
                effect.SetFloat(SprayBlendId, 1f / (1f + Mathf.Tan(half * Mathf.Deg2Rad)));
            }
        }

        /// <summary>Demo control: the gun is moved by script, not a hand.</summary>
        public void SetAutopilot(bool on)
        {
            IsAutopilot = on;
            if (!on)
                SetSpraying(false);
        }

        /// <summary>Demo control: presses / releases the trigger. Ignored unless on autopilot.</summary>
        public void SetAutoSpray(bool spraying)
        {
            if (IsAutopilot)
                SetSpraying(spraying);
        }

        /// <summary>Visual feedback only: spray particles and paint cup show the loaded colour.</summary>
        public void SetSprayColor(Color color)
        {
            // Droplets and mist share the loaded colour; each graph applies its own alpha over life.
            foreach (VisualEffect effect in sprayEffects)
                if (effect != null && effect.HasVector4(SprayColorId))
                    effect.SetVector4(SprayColorId, new Vector4(color.r, color.g, color.b, 1f));

            if (paintCupRenderer != null)
            {
                cupBlock ??= new MaterialPropertyBlock();
                paintCupRenderer.GetPropertyBlock(cupBlock);
                cupBlock.SetColor(BaseColorId, color);
                paintCupRenderer.SetPropertyBlock(cupBlock);
            }
        }

        private void OnGrabbed(SelectEnterEventArgs args) => holder = args.interactorObject as XRBaseInputInteractor;

        private void OnActivated(ActivateEventArgs args)
        {
            holder ??= args.interactorObject as XRBaseInputInteractor;
            holder?.SendHapticImpulse(triggerHapticAmplitude, 0.04f);
            SetSpraying(true);
        }

        private void OnDeactivated(DeactivateEventArgs args) => SetSpraying(false);

        // Dropping the gun with the trigger held must not leave it spraying.
        private void OnReleased(SelectExitEventArgs args)
        {
            SetSpraying(false);
            holder = null;
        }

        private void SetSpraying(bool spraying)
        {
            if (IsSpraying == spraying)
                return;

            IsSpraying = spraying;

            // Play/Stop fire OnPlay/OnStop on the spawner; live particles finish their life naturally.
            foreach (VisualEffect effect in sprayEffects)
            {
                if (effect == null)
                    continue;
                if (spraying)
                    effect.Play();
                else
                    effect.Stop();
            }

            if (spraying)
                SprayStarted?.Invoke();
            else
                SprayStopped?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxConeAngle = Mathf.Max(minConeAngle, maxConeAngle);
            coneAngle = Mathf.Clamp(coneAngle, minConeAngle, maxConeAngle);
        }
#endif
    }
}
