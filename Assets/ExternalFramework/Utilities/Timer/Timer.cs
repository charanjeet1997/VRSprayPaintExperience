using System;
using UnityEngine;

namespace Games.Platformer3D.Utilities
{
	public abstract class Timer
	{
		protected float intialTime;
		protected float Time { get; set; }
		
		public bool IsRunning { get; protected set; }
		
		public float Progress => Time / intialTime;
		
		public Action OnTimerStart = delegate {  };
		public Action OnTimerEnd = delegate {  };
		
		protected Timer(float time)
		{
			// Debug.Log($"Initial Time {time}");
			intialTime = time;
			IsRunning = false;
		}
		
		public void Start()
		{
			Time = intialTime;
			if(!IsRunning)
			{
				IsRunning = true;
				OnTimerStart();
			}
		}
		
		public void Stop()
		{
			if(IsRunning)
			{
				IsRunning = false;
				OnTimerEnd();
			}
		}
		
		public void Resume() => IsRunning = true;
		
		public void Pause() => IsRunning = false;
		
		public abstract void Tick(float deltaTime);
	}
	
	// countdown and cooldown timers

	// stopwatch timer
}