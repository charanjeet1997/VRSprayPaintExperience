using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    [RequireComponent(typeof(Image))]
    public class ImageFiller : Animatable
    {
        private Image image;
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
            image.fillAmount=animPerc;
        }
        public override void OnAnimationEnded()
        {
            base.OnAnimationEnded();
        }
    }
}

