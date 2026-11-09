using System;
using Games.WorldSystem;
using UnityEngine;

namespace nameSpaceName
{
	public class T1 : MonoBehaviour,IWorldEntity,IWorldInitState
	{

		#region PUBLIC_VARS

		#endregion

		#region PRIVATE_VARS

		#endregion

		#region UNITY_CALLBACKS

		#endregion

		#region PUBLIC_METHODS

		public void Initialize()
		{
			Debug.Log("T1 Init");
		}

		#endregion

		#region PRIVATE_METHODS

		#endregion

	}
}