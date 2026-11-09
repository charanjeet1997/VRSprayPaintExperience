using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UISystem;
using UnityEngine.UI;

namespace UISystem
{
    public class UISlide : UIAnimation
    {
        public Vector3 initPos;
        public Vector3 finalPos;
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
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(initPos, finalPos, animPerc);
            base.OnAnimationRunning(animPerc);
        }

        public override void OnAnimationEnded()
        {
            rectTransform.anchoredPosition = finalPos;
            base.OnAnimationEnded();
        }
    }
}
