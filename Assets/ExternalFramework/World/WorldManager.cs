using System;
using System.Collections.Generic;
using ServiceLocatorFramework;
using UnityEngine;

namespace Games.WorldSystem
{
	public class WorldManager : MonoBehaviour,IWorldManager
	{
		#region STATIC_FIELDS
		public static WorldManager Instance;
		#endregion
		#region PUBLIC_VARS
		[SerializeField]
		private WorldDatabase _worldDatabase;
		
		[SerializeField]
		private Dictionary<WorldName, World> worlds;
		#endregion

		#region PRIVATE_VARS
		
		private World currentWorld;
		#endregion

		#region UNITY_CALLBACKS
		private void Awake()
		{
			Instance = this;
		}

		private void OnEnable()
		{
			ServiceLocator.Current.Register<IWorldManager>(this);
		}

		private void OnDisable()
		{
			ServiceLocator.Current.Unregister<IWorldManager>();			
		}

		private void Start()
		{
			worlds = new Dictionary<WorldName, World>();
		}
		#endregion

		#region PUBLIC_METHODS
		public World GetCurrentWorld()
		{
			return currentWorld;
		}
		public IWorldState GetCurrentWorldStatus()
		{
			return (IWorldState) currentWorld;
		}
		public void CreateWorld(WorldDatabase.WorldData worldData)
		{
			if (!worlds.ContainsKey(worldData.WorldName))
			{
				currentWorld = Instantiate(worldData.World);
				worlds.Add(worldData.WorldName,currentWorld);
			}
		}

		public void CreateWorld(WorldName worldName)
		{
			Debug.Log($"World Name {worldName.ToString()}");
			if (!worlds.ContainsKey(worldName))
			{
				WorldDatabase.WorldData worldData = _worldDatabase.GetWorld(worldName);
				if (worldData != null)
				{
					
					currentWorld = Instantiate(worldData.World);
					worlds.Add(worldData.WorldName,currentWorld);
				}
				else
				{
					Debug.LogError("Requested World is not in the database");
				}
			}
			else
			{
				Debug.LogWarning("Requested World is already exist");				
			}
		}
		public void DestroyWorld(WorldName worldName)
		{
			Debug.Log("Destroy world Called : "+worldName.ToString());
			if (worlds.ContainsKey(worldName))
			{
				World world = worlds[worldName];
				worlds.Remove(worldName);
				Destroy(world.gameObject);
			}
		}

		public void ChangeWorldStateTo(float transitionTime,IWorldStateProvider worldStateProvider)
		{
			if(currentWorld == null) Debug.Log("Current World is null");
			currentWorld.TransitionTo(transitionTime, worldStateProvider);
		}

		public List<IWorldEntity> GetCurrentWorldEntity()
		{
			if(currentWorld == null) Debug.Log("Current World is null");
			return currentWorld.GetWorldEntities();
		}
		#endregion

		#region PRIVATE_METHODS

		#endregion

	}
}