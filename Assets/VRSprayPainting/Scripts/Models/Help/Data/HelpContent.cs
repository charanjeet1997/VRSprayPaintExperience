using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Pages of the "How to use" guide. Edit the text in the Inspector; no code per page.</summary>
    [CreateAssetMenu(menuName = "VR Spray Painting/Help Content", fileName = "HelpContent")]
    public class HelpContent : ScriptableObject
    {
        [Serializable]
        public struct Page
        {
            public string title;
            [TextArea(4, 12)] public string body;
        }

        public List<Page> pages = new List<Page>();
    }
}
