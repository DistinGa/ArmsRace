using System;
using UnityEngine;

namespace EJaw.UnityTimers
{
    public sealed class Timer
    {
        #region Fields

        /// <summary>
        /// Send when timer is finished
        /// </summary>
        public event Action Finished = delegate { };

        /// <summary>
        /// Send when the timer step has elapsed
        /// </summary>
        public event Action<float> Elapsed = delegate { };

        /// <summary>
        /// Called, when timer is finished
        /// </summary>
        private Action onFinished = null;

        private MonoBehaviourTimer monoBehaviourTimer = null;

        private const float DEFAULT_STEP = 0.1f;

        #endregion

        #region Properties

        /// <summary>
        /// Return true if timer is running
        /// </summary>
        public bool IsRunning
        {
            get { return monoBehaviourTimer != null; }
        }

        /// <summary>
        /// How much time left timer to finish
        /// </summary>
        public float TimeLeft
        {
            get
            {
                if (monoBehaviourTimer != null)
                    return monoBehaviourTimer.TimeLeft;
                return 0f;
            }
        }

        /// <summary>
        /// How much time spent timer
        /// </summary>
        public float TimeSpent
        {
            get
            {
                if (monoBehaviourTimer != null)
                    return monoBehaviourTimer.TimeSpent;
                return 0f;
            }
        }

        #endregion

        #region Constructors

        public Timer()
        {
        }

        #endregion

        #region Functions

        /// <summary>
        /// Start timer from 0 second till you stop it
        /// </summary>
        public void Start()
        {
            Stop();

            monoBehaviourTimer = TimersFactory.Instance.MonoTimer;
            if (monoBehaviourTimer != null)
            {
                monoBehaviourTimer.StartTimer();
            }
            else
                Debug.LogError("Timer: can't start due to null mono timer!");
        }

        /// <summary>
        /// Start timer with delay than count from delay value till 0
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="onFinished"></param>
        public void Start(float delay, Action onFinished = null)
        {
            Start(delay, DEFAULT_STEP, onFinished);
        }

        /// <summary>
        /// Start timer with delay than count from delay value till 0
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="step"></param>
        /// <param name="onFinished"></param>
        public void Start(float delay, float step, Action onFinished = null)
        {
            Stop();

            monoBehaviourTimer = TimersFactory.Instance.MonoTimer;
            if (monoBehaviourTimer != null)
            {
                monoBehaviourTimer.Finished += OnMonoTimerFinished;
                this.onFinished = onFinished;
                if (delay > 0)
                {
                    monoBehaviourTimer.Elapsed += OnMonoTimerElapsed;
                    monoBehaviourTimer.StartTimer(delay, Math.Max(step, DEFAULT_STEP));
                }
                else
                    OnMonoTimerFinished();
            }
            else
                Debug.LogError("Timer: can't start due to null mono timer!");
        }

        public void Stop()
        {
            if (monoBehaviourTimer != null)
                monoBehaviourTimer.StopTimer();
            ClearMonoBehaviourTimer();
            onFinished = null;
        }

        private void ClearMonoBehaviourTimer()
        {
            if (monoBehaviourTimer != null)
            {
                monoBehaviourTimer.Finished -= OnMonoTimerFinished;
                monoBehaviourTimer.Elapsed -= OnMonoTimerElapsed;
            }
            monoBehaviourTimer = null;
        }

        private void OnMonoTimerFinished()
        {
            ClearMonoBehaviourTimer();
            TimersHelper.SendEvent(Finished);
            TimersHelper.SendEvent(onFinished);
            onFinished = null;
        }

        private void OnMonoTimerElapsed(float left)
        {
            TimersHelper.SendEvent(Elapsed, left);
        }

        #endregion
    }
}