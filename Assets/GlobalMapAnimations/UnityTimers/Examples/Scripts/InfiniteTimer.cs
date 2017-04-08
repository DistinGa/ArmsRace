using UnityEngine;

namespace EJaw.UnityTimers.Examples
{
    public sealed class InfiniteTimer : MonoBehaviour
    {
        private readonly Timer infiniteTimer = new Timer();
        private readonly Timer sentinelTimer = new Timer();

        private void Awake()
        {
            Debug.Log("InfiniteTimer: start infinite timer...");
            infiniteTimer.Start();
            sentinelTimer.Finished += OnSentinelTimerFinished;
            sentinelTimer.Start(Random.Range(1f, 3f));
        }

        private void OnSentinelTimerFinished()
        {
            Debug.Log(string.Format("InfiniteTimer: check timer value = {0} seconds.", infiniteTimer.TimeSpent));
            sentinelTimer.Start(Random.Range(1f, 3f));
        }
    }
}
