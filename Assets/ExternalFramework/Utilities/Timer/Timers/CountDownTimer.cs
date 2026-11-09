using UnityEngine;

namespace Games.Platformer3D.Utilities
{
    public class CountDownTimer : Timer
    {
        public CountDownTimer(float time) : base(time) { }
		
        public override void Tick(float deltaTime)
        {
            if(IsRunning && Time > 0)
            {
                Debug.Log($"Timer Running {Time}");
                Time -= deltaTime;
            }
			
            if(IsRunning && Time <= 0)
            {
                Stop();
            }
        }
		
        public bool IsFinished => Time <= 0;
		
        public void Reset()
        {
            Time = intialTime;
        }
		
        public void Reset(float newTime)
        {
            intialTime = newTime;
            Reset();
        }
    }
}