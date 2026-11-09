using System;
using ServiceLocatorFramework;

namespace Games.GameStateFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class GameStateManager : MonoBehaviour
    {
        private bool isStarted = false;
        // game states
        List<IGameState> startGameStates = new List<IGameState>();
        List<IGameState> pauseGameStates = new List<IGameState>();
        List<IGameState> gameEndGameStates = new List<IGameState>();
        List<IGameState> quitGameStates = new List<IGameState>();
        List<IGameState> lateTickGameStates = new List<IGameState>();
        List<IGameState> fixedTickGameStates = new List<IGameState>();
        List<IGameState> tickGameStates = new List<IGameState>();

        private void Awake()
        {
            ServiceLocator.Current.Register(this);
        }

        private void Start()
        {
            StartGame();
        }

        private void Update()
        {
            if (isStarted)
            {
                foreach (var tickGameState in tickGameStates)
                {
                    ((IGameTick)tickGameState).Tick();
                }
            }
        }
        
        private void FixedUpdate()
        {
            if (isStarted)
            {
                foreach (var fixedTickGameState in fixedTickGameStates)
                {
                    ((IGameFixedTick)fixedTickGameState).FixedTick();
                }
            }
        }
        
        private void LateUpdate()
        {
            if (isStarted)
            {
                foreach (var lateTickGameState in lateTickGameStates)
                {
                    ((IGameLateTick)lateTickGameState).LateTick();
                }
            }
        }

        public void Register(IGameState gameState)
        {
            if (gameState is IGameStart)
            {
                startGameStates.Add(gameState);
            }
            if (gameState is IGamePause)
            {
                pauseGameStates.Add(gameState);
            }
            if (gameState is IQuitGame)
            {
                quitGameStates.Add(gameState);
            }
            if (gameState is IGameLateTick)
            {
                lateTickGameStates.Add(gameState);
            }
            if (gameState is IGameFixedTick)
            {
                fixedTickGameStates.Add(gameState);
            }
            if (gameState is IGameTick)
            {
                tickGameStates.Add(gameState);
            }

            if (gameState is IGameOver)
            {
                gameEndGameStates.Add(gameState);
            }
        }
        
        public void Unregister(IGameState gameState)
        {
            if (gameState is IGameStart)
            {
                startGameStates.Remove(gameState);
            }
            if (gameState is IGamePause)
            {
                pauseGameStates.Remove(gameState);
            }
            if (gameState is IQuitGame)
            {
                quitGameStates.Remove(gameState);
            }
            if (gameState is IGameLateTick)
            {
                lateTickGameStates.Remove(gameState);
            }
            if (gameState is IGameFixedTick)
            {
                fixedTickGameStates.Remove(gameState);
            }
            if (gameState is IGameTick)
            {
                tickGameStates.Remove(gameState);
            }
            
            if (gameState is IGameOver)
            {
                gameEndGameStates.Remove(gameState);
            }
        }

        public void StartGame()
        {
            isStarted = true;
            foreach (var startGameState in startGameStates)
            {
                ((IGameStart)startGameState).StartGame();
            }
        }
        
        public void PauseGame()
        {
            foreach (var pauseGameState in pauseGameStates)
            {
                ((IGamePause)pauseGameState).PauseGame();
            }
        }
        
        public void ResumeGame()
        {
            foreach (var pauseGameState in pauseGameStates)
            {
                ((IGamePause)pauseGameState).UnPauseGame();
            }
        }
        
        public void ExitGame()
        {
            isStarted = false;
            foreach (var quitGameState in quitGameStates)
            {
                ((IQuitGame)quitGameState).QuitGame();
            }
        }
        
        public void GameOver()
        {
            isStarted = false;
            foreach (var gameEndGameState in gameEndGameStates)
            {
                ((IGameOver)gameEndGameState).GameOver();
            }
        }

    }
}