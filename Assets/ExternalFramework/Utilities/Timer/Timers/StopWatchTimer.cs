namespace Games.Platformer3D.Utilities
{
    public class StopWatchTimer : Timer
    {
        public StopWatchTimer() : base(0) { }
		
        public override void Tick(float deltaTime)
        {
            if(IsRunning && Time < intialTime)
            {
                Time += deltaTime;
            }
        }
		
        public void Reset()
        {
            Time = 0;
        }
		
        public float GetTime()
        {
            return Time;
        }
    }
}