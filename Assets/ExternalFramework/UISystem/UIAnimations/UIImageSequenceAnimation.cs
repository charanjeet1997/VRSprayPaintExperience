using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UISystem;
using UnityEngine.UI;

namespace UISystem
{
    public class UIImageSequenceAnimation : Animatable
    {
        public Sprite[] sprites;
        private Image targetImage;

        public override void Awake()
        {
            base.Awake();
            targetImage = GetComponent<Image>();
        }

        public override void OnAnimationStarted()
        {
            base.OnAnimationStarted();
        }

        public override void OnAnimationRunning(float animPerc)
        {
            if (targetImage != null && sprites != null && sprites.Length > 0)
            {
                int index = Mathf.Clamp(Mathf.FloorToInt(animPerc * sprites.Length), 0, sprites.Length - 1);
                targetImage.sprite = sprites[index];
            }
            base.OnAnimationRunning(animPerc);
        }

        public override void OnAnimationEnded()
        {
            base.OnAnimationEnded();
        }
    }
}
