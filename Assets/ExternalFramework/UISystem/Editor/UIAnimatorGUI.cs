using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace UISystem
{
	[CustomEditor(typeof(UIAnimator)),CanEditMultipleObjects]
    public class UIAnimatorGUI : Editor
    {
		public bool showAnimatorEvents=false;
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			// showAnimatorEvents = EditorGUILayout.Toggle("ShowAnimatorEvents", showAnimatorEvents);
			// UIAnimator uiAnimator = target as UIAnimator;
			// if(showAnimatorEvents)
			// {
			// 	// EditorGUILayout.Equals()
			// }
			// else
			// {

			// }
		}
    }
}
