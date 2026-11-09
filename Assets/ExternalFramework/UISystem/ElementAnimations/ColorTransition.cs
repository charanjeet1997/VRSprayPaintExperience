using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    [RequireComponent(typeof(Image))]

    public class ColorTransition : Animatable
    {
        private Image image;
        public Gradient transitionColor;
        public override void Awake()
        {
            base.Awake();
            image = GetComponent<Image>();
        }

        public override void OnAnimationStarted()
        {
            base.OnAnimationStarted();
        }
        public override void OnAnimationRunning(float animPerc)
        {
            base.OnAnimationRunning(animPerc);
            Debug.Log(animPerc);
            image.color = transitionColor.Evaluate(animPerc);
        }
        public override void OnAnimationEnded()
        {
            base.OnAnimationEnded();
        }
    }
}

