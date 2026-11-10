using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VRSprayPainting
{
    /// <summary>
    /// Shows a hand instead of the controller model and curls its fingers from the real controller input:
    /// grip closes middle, ring and little fingers, trigger curls the index finger, the thumb follows the grip.
    /// Works on any hand rig with OpenXR-style finger bones (fingers along +Z, curl around +X).
    /// </summary>
    public class ControllerHandPoser : MonoBehaviour
    {
        [Serializable]
        private class Finger
        {
            [Tooltip("Proximal, intermediate, distal.")]
            public Transform[] bones = new Transform[0];
            [Tooltip("Curl per bone at full press, degrees around the bone's X axis.")]
            public Vector3 curl = new Vector3(65f, 85f, 55f);
            [NonSerialized] public Quaternion[] open;
        }

        [SerializeField] private InputActionProperty gripValue;
        [SerializeField] private InputActionProperty triggerValue;

        [SerializeField] private Finger thumb = new Finger { curl = new Vector3(20f, 30f, 35f) };
        [SerializeField] private Finger index = new Finger();
        [SerializeField] private Finger middle = new Finger();
        [SerializeField] private Finger ring = new Finger();
        [SerializeField] private Finger little = new Finger();

        [Tooltip("Fingers rest slightly bent rather than flat.")]
        [SerializeField, Range(0f, 1f)] private float restCurl = 0.12f;
        [Tooltip("How fast fingers follow the input (per second).")]
        [SerializeField] private float followSpeed = 18f;

        private float grip;
        private float trigger;

        private void Awake()
        {
            foreach (Finger finger in Fingers())
            {
                finger.open = new Quaternion[finger.bones.Length];
                for (int i = 0; i < finger.bones.Length; i++)
                    if (finger.bones[i] != null)
                        finger.open[i] = finger.bones[i].localRotation;
            }
        }

        private void OnEnable()
        {
            gripValue.action?.Enable();
            triggerValue.action?.Enable();
        }

        private void LateUpdate()
        {
            float targetGrip = Read(gripValue);
            float targetTrigger = Read(triggerValue);
            float k = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            grip = Mathf.Lerp(grip, targetGrip, k);
            trigger = Mathf.Lerp(trigger, targetTrigger, k);

            Pose(index, trigger);
            Pose(middle, grip);
            Pose(ring, grip);
            Pose(little, grip);
            Pose(thumb, Mathf.Max(grip, trigger * 0.6f));
        }

        private void Pose(Finger finger, float amount)
        {
            if (finger.open == null)
                return;
            float t = Mathf.Lerp(restCurl, 1f, Mathf.Clamp01(amount));
            for (int i = 0; i < finger.bones.Length && i < 3; i++)
            {
                if (finger.bones[i] == null)
                    continue;
                finger.bones[i].localRotation = finger.open[i] * Quaternion.Euler(finger.curl[i] * t, 0f, 0f);
            }
        }

        private static float Read(InputActionProperty property)
        {
            InputAction action = property.action;
            return action != null && action.enabled ? action.ReadValue<float>() : 0f;
        }

        private Finger[] Fingers() => new[] { thumb, index, middle, ring, little };
    }
}
