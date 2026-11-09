using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace UISystem
{
    [System.Serializable]
    public class AnimationEvent
    {
        public float time;
        public UnityEvent Event;
        public bool isCalled=false;
    }
    public enum UIAnimationType
    {
        Show,
        Hide
    }
    public class UIAnimator : MonoBehaviour
    {
        [SerializeField] float animationTime;
        public List<AnimationEvent> animationEvents;
        IEnumerator showCoroutine;
        IEnumerator hideCoroutine;
        UIAnimationType animationType;
        List<UIAnimation> showAnim = new List<UIAnimation>();
        List<UIAnimation> hideAnim = new List<UIAnimation>();
        List<Animatable> showAnimatableUI = new List<Animatable>();
        List<Animatable> hideAnimatableUI = new List<Animatable>();
        List<AnimationEvent> tempAnimationEvents;
        int counter = 0;
        float maxHideWaitTime = 0;
        BaseUI baseUI;
        int maxCounter = 0;
        public void Awake()
        {
            foreach (UIAnimation animation in GetComponents<UIAnimation>())
            {
                if (animation.uIAnimationType == UIAnimationType.Show)
                {
                    showAnim.Add(animation);
                }
                else
                {
                    hideAnim.Add(animation);
                }
            }
            tempAnimationEvents = new List<AnimationEvent>(animationEvents);
            baseUI = GetComponent<BaseUI>();
        }
        public virtual void RegisterAnimatable(Animatable animatable, UIAnimationType animationType)
        {

            if (animationType == UIAnimationType.Show)
            {
                showAnimatableUI.Add(animatable);
            }
            else
            {
                hideAnimatableUI.Add(animatable);
                if (maxHideWaitTime < animatable.duration + animatable.delay)
                {
                    maxHideWaitTime = animatable.duration + animatable.delay;
                }
            }
        }
        public void StartShow()
        {
            StopAllCoroutines();
            baseUI.Enable();
            if (showAnim.Count > 0)
            {
                showCoroutine = ShowAnimation();
                StartCoroutine(showCoroutine);
            }
            else
            {
                ResetToDefault();
            }
            for (int i = 0; i < showAnimatableUI.Count; i++)
            {
                showAnimatableUI[i].StartAnimate();
            }
        }
        public void ResetToDefault()
        {
            baseUI.content.transform.localScale = Vector3.one;
            baseUI.content.transform.localRotation = Quaternion.identity;
            baseUI.canvasGroup.alpha = 1;
            baseUI.canvasGroup.interactable = true;
        }
        public void StartHide()
        {
            if (hideAnim.Count > 0)
            {
                hideCoroutine = HideAnimation();
                StartCoroutine(hideCoroutine);
            }
            else
            {
                CoroutineExtension.Execute(this, () => baseUI.Disable(), maxHideWaitTime);
            }
            for (int i = 0; i < hideAnimatableUI.Count; i++)
            {
                hideAnimatableUI[i].StartAnimate();
            }
        }
        public IEnumerator ShowAnimation()
        {
            float elapsed = 0;
            float perc;
            maxCounter = Mathf.Max(animationEvents.Count, showAnim.Count) - 1;

            for (counter = maxCounter; counter >= 0; counter--)
            {
                if (counter <= showAnim.Count - 1)
                {
                    showAnim[counter].OnAnimationStarted();
                }
                if (counter <= animationEvents.Count - 1 && animationEvents[counter].time <= elapsed)
                {
                    animationEvents[counter].Event.Invoke();
                }
            }
            while (elapsed <= animationTime)
            {
                perc = elapsed / animationTime;
                maxCounter = Mathf.Max(animationEvents.Count, showAnim.Count) - 1;

                for (counter = maxCounter; counter >= 0; counter--)
                {
                    if (counter <= showAnim.Count - 1)
                    {
                        showAnim[counter].OnAnimationRunning(showAnim[counter].animationCurve.Evaluate(perc));
                    }
                    if (counter <= animationEvents.Count - 1 && animationEvents[counter].time <= elapsed)
                    {
                        animationEvents[counter].Event.Invoke();
                    }
                }
                elapsed += Time.fixedDeltaTime;
                yield return null;
            }
            maxCounter = Mathf.Max(animationEvents.Count, showAnim.Count) - 1;
            for (counter = (animationEvents.Count > showAnim.Count) ? animationEvents.Count - 1 : showAnim.Count; counter >= 0; counter--)
            {
                if (counter <= showAnim.Count - 1)
                {
                    showAnim[counter].OnAnimationEnded();
                }
                if (counter <= animationEvents.Count - 1 && animationEvents[counter].time <= elapsed)
                {
                    animationEvents[counter].Event.Invoke();
                }
            }
            yield return null;
        }
        public IEnumerator HideAnimation()
        {
            float elapsed = 0;
            float perc;
            maxCounter = Mathf.Max(animationEvents.Count, hideAnim.Count) - 1;
            for (counter = maxCounter; counter >= 0; counter--)
            {
                if (counter <= hideAnim.Count - 1)
                {
                    hideAnim[counter].OnAnimationStarted();
                }
                if (counter <= animationEvents.Count - 1 && animationEvents[counter].time <= elapsed)
                {
                    animationEvents[counter].Event.Invoke();
                }
            }
            while (elapsed <= animationTime)
            {
                perc = elapsed / animationTime;
                maxCounter = Mathf.Max(animationEvents.Count, hideAnim.Count) - 1;

                for (counter = maxCounter; counter >= 0; counter--)
                {
                    if (counter <= hideAnim.Count - 1)
                    {
                        hideAnim[counter].OnAnimationRunning(hideAnim[counter].animationCurve.Evaluate(perc));
                    }
                    if (counter <= animationEvents.Count - 1 && animationEvents[counter].time <= elapsed)
                    {
                        animationEvents[counter].Event.Invoke();
                    }
                }
                elapsed += Time.fixedDeltaTime;
                yield return null;
            }
            maxCounter = Mathf.Max(animationEvents.Count, hideAnim.Count) - 1;
            for (counter = maxCounter; counter >= 0; counter--)
            {
                if (counter <= hideAnim.Count - 1)
                {
                    hideAnim[counter].OnAnimationEnded();
                }
                if (counter <= animationEvents.Count - 1 && animationEvents[counter].time <= elapsed)
                {
                    animationEvents[counter].Event.Invoke();
                }
            }
            baseUI.Disable();
            yield return null;
        }
        public void StopShow()
        {
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
                maxCounter = Mathf.Max(animationEvents.Count, showAnim.Count) - 1;
                for (counter = maxCounter; counter >= 0; counter--)
                {
                    if (counter <= showAnim.Count - 1)
                    {
                        showAnim[counter].OnAnimationEnded();
                    }
                    if (counter <= animationEvents.Count - 1 && animationEvents[counter].time <= 1)
                    {
                        animationEvents[counter].Event.Invoke();
                    }
                }
            }
            for (counter = 0; counter < showAnimatableUI.Count; counter++)
            {
                showAnimatableUI[counter].StopAnimate();
            }
        }

        public void StopHide()
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
                baseUI.Disable();
                maxCounter = Mathf.Max(animationEvents.Count, hideAnim.Count) - 1;
                for (counter = maxCounter; counter >= 0; counter--)
                {
                    if (counter <= hideAnim.Count - 1)
                    {
                        hideAnim[counter].OnAnimationEnded();
                    }
                    if (counter <= animationEvents.Count - 1 && animationEvents[counter].time <= 1)
                    {
                        animationEvents[counter].Event.Invoke();
                    }
                }
            }
            for (counter = 0; counter < hideAnimatableUI.Count; counter++)
            {
                hideAnimatableUI[counter].StopAnimate();
            }
        }
    }
}