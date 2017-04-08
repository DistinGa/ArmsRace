using UnityEngine;

namespace EJaw.UnityTimers.Examples
{
    public sealed class TimerWithManualStep : MonoBehaviour
    {
        [SerializeField] private float delay = 5f;
        [SerializeField] private float step = 1f;
        private readonly Timer timer = new Timer();

        private void Awake()
        {
            Debug.Log(string.Format("TimerWithManualStep: start timer for {0} seconds with step {1}...", delay, step));
            timer.Start(delay, step);
            timer.Elapsed += OnTimerElapsed;
            timer.Finished += OnTimerFinished;
        }

        private void OnTimerElapsed(float timeLeft)
        {
            Debug.Log(string.Format("TimerWithManualStep: timer elapsed. Time left {0} seconds", timeLeft));
        }

        private void OnTimerFinished()
        {
            Debug.Log("TimerWithManualStep: timer finished.");
            timer.Elapsed -= OnTimerElapsed;
            timer.Finished -= OnTimerFinished;
        }
    }
}