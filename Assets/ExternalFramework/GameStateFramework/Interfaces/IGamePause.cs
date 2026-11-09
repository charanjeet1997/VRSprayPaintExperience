namespace Games.GameStateFramework
{
	using UnityEngine;
	using System;
	using System.Collections;

	public interface IGamePause : IGameState
	{
		void PauseGame();
		void UnPauseGame();
	}
}