using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UISystem;
using UnityEngine.UI;
namespace UISystem
{
    public class UIFade : UIAnimation
    {
        public float initialAlpha;
        public float targetAlpha;
        CanvasGroup canvasGroup;
        public override void Awake()
        {
            base.Awake();
            canvasGroup = content.GetComponent<CanvasGroup>();
        }
        public override void OnAnimationStarted()
        {
            base.OnAnimationStarted();
        }
        public override void OnAnimationRunning(float animPerc)
        {
            canvasGroup.alpha = Mathf.LerpUnclamped(initialAlpha, targetAlpha, animPerc);
            base.OnAnimationRunning(animPerc);
        }
        public override void OnAnimationEnded()
        {
            canvasGroup.alpha=targetAlpha;
            base.OnAnimationEnded();
        }
    }
}
