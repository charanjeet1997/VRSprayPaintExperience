using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Safety and prep state before spraying: respirator and eye protection worn (PpeItem puts them on when
    /// brought to the face; a full-face respirator covers the eyes, so no goggles are needed), booth extraction fan
    /// on (BoothFanSwitch). Read by the safety step, the report and the fan sound.
    /// </summary>
    public class SafetyGear : MonoBehaviour
    {
        [SerializeField] private PpeItem respirator;
        [Tooltip("Separate goggles. Leave empty when the respirator is full-face.")]
        [SerializeField] private PpeItem goggles;
        [SerializeField] private AudioSource fanAudio;

        public bool RespiratorWorn { get; private set; }
        public bool GogglesWorn { get; private set; }
        public bool FanOn { get; private set; }
        public bool AllSet => RespiratorWorn && GogglesWorn && FanOn;
        /// <summary>The respirator has a visor: one item protects lungs and eyes.</summary>
        public bool FullFace => respirator != null && respirator.ProtectsEyes;
        public Transform RespiratorTransform => respirator != null ? respirator.transform : null;
        public Transform GogglesTransform => goggles != null ? goggles.transform : null;
        /// <summary>The safety step is asking for the gear: the guide arrow and the blinking switch show.</summary>
        public bool GuideActive { get; set; }

        public event Action Changed;

        private void OnEnable()
        {
            if (respirator != null) respirator.Equipped += OnEquipped;
            if (goggles != null) goggles.Equipped += OnEquipped;
        }

        private void OnDisable()
        {
            if (respirator != null) respirator.Equipped -= OnEquipped;
            if (goggles != null) goggles.Equipped -= OnEquipped;
        }

        private void OnEquipped(PpeItem item)
        {
            if (item == respirator)
            {
                RespiratorWorn = true;
                if (respirator.ProtectsEyes)
                    GogglesWorn = true;
            }
            if (item == goggles) GogglesWorn = true;
            Changed?.Invoke();
        }

        public void SetFan(bool on)
        {
            if (FanOn == on)
                return;
            FanOn = on;
            if (fanAudio != null)
            {
                if (on) fanAudio.Play();
                else fanAudio.Stop();
            }
            Changed?.Invoke();
        }

        public void ToggleFan() => SetFan(!FanOn);

        /// <summary>Takes the gear off and puts it back on the bench (start of the safety lesson).</summary>
        public void ResetGear()
        {
            RespiratorWorn = false;
            GogglesWorn = false;
            respirator?.PutBack();
            goggles?.PutBack();
            SetFan(false);
            Changed?.Invoke();
        }
    }
}
