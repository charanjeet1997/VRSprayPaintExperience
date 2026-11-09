using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UISystem
{
    [RequireComponent(typeof(UIAnimator))]
    public class UIAnimation : MonoBehaviour
    {
        public UIAnimationType uIAnimationType;
        protected GameObject content;
        protected GameObject overlay;
        protected Canvas canvas;
        protected BaseUI baseUI;
        protected bool isAnimationRunning;
        [SerializeField] public AnimationCurve animationCurve;
        public virtual void Awake()
        {
            if (GetComponent<Popup>() != null)
            {
                content = transform.GetChild(1).gameObject;
                overlay = transform.GetChild(0).gameObject;
            }
            else if (GetComponent<Screen>() != null)
            {
                overlay = null;
                content = transform.GetChild(0).gameObject;
            }
            else if (GetComponent<NavBar>() != null)
            {
                overlay = null;
                content = transform.GetChild(0).gameObject;
            }
            else
            {
                content = transform.GetChild(0).gameObject;
                Debug.Log("No Base UI on : " + gameObject.name);
            }
            canvas = GetComponent<Canvas>();
            baseUI = GetComponent<BaseUI>();
        }

        public virtual void OnAnimationStarted()
        {
            isAnimationRunning = false;
        }
        public virtual void OnAnimationEnded()
        {
            isAnimationRunning = false;
        }
        public virtual void OnAnimationRunning(float percentage)
        {
            isAnimationRunning = true;
        }
    }
}