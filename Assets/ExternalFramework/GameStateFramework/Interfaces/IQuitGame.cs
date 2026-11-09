namespace Games.GameStateFramework
{
	using UnityEngine;
	using System;
	using System.Collections;

	public interface IQuitGame : IGameState
	{
		void QuitGame();
	}
}