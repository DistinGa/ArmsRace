using UnityEngine;

namespace EJaw.UnityTimers.Examples
{
    public sealed class TimerWithDefaultStep : MonoBehaviour
    {
        [SerializeField] private float delay = 5f;
        private readonly Timer timer = new Timer();

        private void Awake()
        {
            Debug.Log(string.Format("TimerWithDefaultStep: start timer for {0} seconds...", delay));
            timer.Start(delay);
            timer.Elapsed += OnTimerElapsed;
            timer.Finished += OnTimerFinished;
        }

        private void OnTimerElapsed(float timeLeft)
        {
            Debug.Log(string.Format("TimerWithDefaultStep: timer elapsed. Time left {0} seconds", timeLeft));
        }

        private void OnTimerFinished()
        {
            Debug.Log("TimerWithDefaultStep: timer finished.");
            timer.Elapsed -= OnTimerElapsed;
            timer.Finished -= OnTimerFinished;
        }
    }
}
