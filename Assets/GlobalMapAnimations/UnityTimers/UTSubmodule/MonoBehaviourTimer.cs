using System;
using System.Collections;
using UnityEngine;

namespace EJaw.UnityTimers
{
    /// <summary>
    /// Class that check time with Coroutines
    /// </summary>
    public sealed class MonoBehaviourTimer : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Send when Coroutine is finished
        /// </summary>
        public event Action Finished = delegate { };

        /// <summary>
        /// Send when the timer step has elapsed
        /// </summary>
        public event Action<float> Elapsed = delegate { };

        /// <summary>
        /// How much time left timer to finish
        /// </summary>
        private float timeLeft = 0f;

        /// <summary>
        /// How much time spent timer from start
        /// </summary>
        private float timeSpent = 0f;

        #endregion

        #region Properties

        /// <summary>
        /// How much time left timer to finish
        /// </summary>
        public float TimeLeft
        {
            get { return timeLeft; }
        }

        /// <summary>
        /// How much time spent timer from start
        /// </summary>
        public float TimeSpent
        {
            get { return timeSpent; }
        }

        #endregion

        #region Functions

        public void StartTimer()
        {
            gameObject.SetActive(true);
            StartCoroutine(Step());
        }

        public void StartTimer(float delay, float step)
        {
            gameObject.SetActive(true);
            StartCoroutine(Step(delay, step));
        }

        public void StopTimer()
        {
            StopAllCoroutines();
            timeLeft = 0;
            timeSpent = 0;
            gameObject.SetActive(false);
        }

        private IEnumerator Step()
        {
            timeSpent = 0;
            while (true)
            {
                yield return new WaitForSeconds(Time.deltaTime);
                timeSpent += Time.deltaTime;
                TimersHelper.SendEvent(Elapsed, timeSpent);
            }
        }

        private IEnumerator Step(float delay, float step)
        {
            timeLeft = delay;
            TimersHelper.SendEvent(Elapsed, timeLeft);
            do
            {
                yield return new WaitForSeconds(step);
                timeLeft -= step;
                if (timeLeft > 0)
                    TimersHelper.SendEvent(Elapsed, timeLeft);
                else
                    break;
            } while (true);
            timeLeft = 0;

            TimersHelper.SendEvent(Elapsed, timeLeft);
            TimersHelper.SendEvent(Finished);
            gameObject.SetActive(false);
        }

        #endregion
    }
}
