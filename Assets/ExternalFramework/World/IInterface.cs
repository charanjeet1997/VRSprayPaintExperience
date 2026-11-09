using System;
using System.Collections.Generic;
using UnityEngine;

namespace Games.WorldSystem
{
    public interface IWorldTick
    {

    }

    public interface IWorldContinousTick : IWorldTick
    {
    }

    public interface IWorldSingleTick : IWorldTick
    {

    }

    public interface IWorldState
    {

    }

    public interface IWorldRunningState : IWorldState
    {
        void Running();
    }
    public interface IWorldInitState : IWorldState
    {
        void Initialize();
    }
    public interface IWorldDeinitState : IWorldState
    {
        void Deinit();
    }
    public interface IWorldPauseState : IWorldState
    {
        void Pause();
    }

    public interface IGameOverState : IWorldState
    {
        void GameOver();
    }

    public interface IWorldStateProvider
    {
        void Act(float transitionPercentage);
    }

    public interface IWorldEntity
    {

    }
    public interface IWorldManager
    {
        World GetCurrentWorld();
        IWorldState GetCurrentWorldStatus();
        void CreateWorld(WorldDatabase.WorldData worldData);
        void CreateWorld(WorldName worldName);
        void DestroyWorld(WorldName worldName);
        void ChangeWorldStateTo(float transitionTime, IWorldStateProvider worldStateProvider);
        List<IWorldEntity> GetCurrentWorldEntity();
    }
}