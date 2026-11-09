using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Games.WorldSystem
{
	public class World : MonoBehaviour
	{
		#region PUBLIC_VARS
		[SerializeField]
		private WorldName worldName;
		
		[SerializeField]
		private List<IWorldEntity> worldEntities;
		
		[SerializeField] 
		private IWorldStateProvider worldStateProvider;
		#endregion

		#region PRIVATE_VARS
		#endregion

		#region UNITY_CALLBACKS
		private void Awake()
		{
			worldEntities = new List<IWorldEntity>(transform.GetComponentsInChildren<IWorldEntity>());
		}
		#endregion

		#region PUBLIC_METHODS
		public void TransitionTo(float transitionTime, IWorldStateProvider worldStateProvider)
		{
			StopAllCoroutines();
			StartCoroutine(Transit(transitionTime, worldStateProvider));
			if (worldStateProvider is IWorldContinousTick)
			{
				StartCoroutine(Tick(transitionTime, worldStateProvider));
			}
		}
		public void Add(IWorldEntity worldEntity)
		{
			if(!worldEntities.Contains(worldEntity))
				worldEntities.Add(worldEntity);
		}

		public void Remove(IWorldEntity worldEntity)
		{
			if(worldEntities.Contains(worldEntity))
				worldEntities.Remove(worldEntity);
		}

		public List<IWorldEntity> GetWorldEntities()
		{
			return worldEntities;
		}
		#endregion

		#region PRIVATE_METHODS

		private IEnumerator Transit(float transitionTime,IWorldStateProvider worldStateProvider)
		{
			float currentTime = 0f;
			while (currentTime<=transitionTime)
			{
				currentTime += Time.deltaTime;
				worldStateProvider.Act(Mathf.Clamp01(currentTime/transitionTime));
				yield return null;
			}
		}
		private IEnumerator Tick(float transitionTime,IWorldStateProvider worldStateProvider)
		{
			yield return new WaitForSeconds(transitionTime);
			while (gameObject.activeInHierarchy)
			{
				worldStateProvider.Act(1f);
				yield return null;
			}
		}
		#endregion
	}
}