using System;
using System.Collections.Generic;
using UnityEngine;

namespace Games.WorldSystem
{
	public class WorldDeInitStateProvider : IWorldStateProvider,IWorldSingleTick
	{
		public List<IWorldDeinitState> entities;

		public WorldDeInitStateProvider(List<IWorldEntity> worldEntities)
		{
			entities = new List<IWorldDeinitState>();

			foreach (var entity in worldEntities)
			{
				if (entity is IWorldDeinitState)
				{
					entities.Add((IWorldDeinitState)entity);
				}
			}
		}

		public void Act(float transitionPercentage)
		{
			for (int indexOfInitState = 0; indexOfInitState < entities.Count; indexOfInitState++)
			{
				entities[indexOfInitState].Deinit();
			}
		}
	}
}