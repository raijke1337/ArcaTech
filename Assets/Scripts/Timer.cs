using System;

namespace Arcatech
{
    [Serializable]
    public abstract class Timer
    {
        protected float initialTime;
        protected float Time { get; set; }
        public bool IsRunning { get; protected set; }
        public float Progress => Time / initialTime;

        public Action OnTimerStarted = delegate { };
        public Action OnTimerStopped = delegate { };

        protected Timer(float value)
        {
            initialTime = value;
            IsRunning = false;

        }

        public Timer Start()
        {
            Time = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                OnTimerStarted.Invoke();
            }
            return this;
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimerStopped.Invoke();
            }
        }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public abstract void Tick(float delta);

    }

    public class CountDownTimer : Timer
    {
        public CountDownTimer(float time) : base(time)
        {

        }

        public override void Tick(float delta)
        {
            if (IsRunning && Time > 0)
            {
                Time -= delta;
                
            }
            if (IsRunning && Time <= 0)
            {
                Stop();
            }
        }
        /// <summary>
        /// has expired
        /// </summary>
        public bool IsReady => Time <= 0;
        public void Reset()
        {
            Time = initialTime;
        }
        public void Reset(float newtime)
        {
            Time = newtime;
        }
    }
    public class StopwatchTimer : Timer
    {
        public StopwatchTimer() : base(0) { }

        public override void Tick(float delta)
        {
            if (IsRunning)
            {
                Time += delta;
            }
        }
        public void Reset() => Time = 0;
        public float GetTime => Time;
    }


}