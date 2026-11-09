using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UISystem
{
    public class Scale : Animatable
    {
		public Vector3 fromScale;
        public Vector3 toScale;
		public Vector3 finalScale;
		
		public UnityEvent onScaleChanged;
		public override void OnAnimationStarted()
		{
			base.OnAnimationStarted();
			rectTransform.localScale = fromScale;
		}
		public override void OnAnimationRunning(float animPerc)
		{
			base.OnAnimationRunning(animPerc);
			rectTransform.localScale = Vector3.LerpUnclamped(fromScale, toScale, animPerc);
//			Debug.Log("Scale CAlled");
		}
		public override void OnAnimationEnded()
		{
			base.OnAnimationEnded();
			rectTransform.localScale = finalScale;
			onScaleChanged?.Invoke();
		}
    }
}

