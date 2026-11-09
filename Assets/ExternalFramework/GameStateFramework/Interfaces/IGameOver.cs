namespace Games.GameStateFramework
{
	using UnityEngine;
	using System;
	using System.Collections;

	public interface IGameOver : IGameState
	{
		void GameOver();
	}
}