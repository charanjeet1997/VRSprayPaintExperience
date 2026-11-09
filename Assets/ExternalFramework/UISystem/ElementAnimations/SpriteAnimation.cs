using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UISystem
{
    [RequireComponent(typeof(Image))]
    public class SpriteAnimation : Animatable
    {
        public Sprite[] sprites;
        Image image;
        int i=0;
        private int spriteIndex;
        public override void Awake()
        {
            base.Awake();
            image=GetComponent<Image>();
        }
        public override void OnAnimationStarted()
        {
            base.OnAnimationStarted();
        }
        public override void OnAnimationEnded()
        {
            base.OnAnimationEnded();
        }
        public override void OnAnimationRunning(float percentage)
        {
            base.OnAnimationRunning(percentage);
            image.sprite=sprites[(i + 1) % sprites.Length];
            i= (int)((sprites.Length - 1)*percentage);
            
        }
    }
}
