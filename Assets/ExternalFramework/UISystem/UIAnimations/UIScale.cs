using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UISystem;
using UnityEngine.UI;

namespace UISystem
{
    public class UIScale : UIAnimation
    {
        public Vector3 initScale;
        public Vector3 finalScale;
        RectTransform rectTransform;
        public override void Awake()
        {
            base.Awake();
            rectTransform = content.GetComponent<RectTransform>();
        }
        public override void OnAnimationStarted()
        {
            base.OnAnimationStarted();
            
        }
        public override void OnAnimationRunning(float animPerc)
        {
            rectTransform.localScale = Vector2.LerpUnclamped(initScale, finalScale, animPerc);
            base.OnAnimationRunning(animPerc);
        }
        public override void OnAnimationEnded()
        {
            rectTransform.localScale = finalScale;
            base.OnAnimationEnded();
        }
    }
}
