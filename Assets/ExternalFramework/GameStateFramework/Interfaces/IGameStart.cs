namespace Games.GameStateFramework
{
	using UnityEngine;
	using System;
	using System.Collections;

	public interface IGameStart : IGameState
	{
		void StartGame();
	}
}