namespace Games.GameStateFramework
{
	using UnityEngine;
	using System;
	using System.Collections;

	public interface IGameFixedTick : IGameState
	{
		void FixedTick();
	}
}