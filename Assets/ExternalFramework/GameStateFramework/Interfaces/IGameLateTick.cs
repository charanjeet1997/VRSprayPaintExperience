namespace Games.GameStateFramework
{
	using UnityEngine;
	using System;
	using System.Collections;

	public interface IGameLateTick : IGameState
	{
		void LateTick();
	}
}