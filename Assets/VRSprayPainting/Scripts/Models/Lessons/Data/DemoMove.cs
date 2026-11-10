using System;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>One step of an automatic spray demo: the gun glides from one point of the demo area to another.</summary>
    [Serializable]
    public struct DemoMove
    {
        [TextArea(1, 3)] public string caption;
        [Tooltip("Start / end on the panel, -0.5..0.5 of the demo area.")]
        public Vector2 from;
        public Vector2 to;
        [Tooltip("Nozzle distance from the surface, metres.")]
        public float distance;
        [Tooltip("Gun tilt away from straight-on, degrees (turned sideways).")]
        public float tilt;
        public float duration;
        public bool spray;
        [Tooltip("Load the demo's second colour before this move.")]
        public bool switchColor;
        [Tooltip("Shake the sprayer up and down at 'from' for the duration (mixing an aerosol can).")]
        public bool shake;
    }
}
