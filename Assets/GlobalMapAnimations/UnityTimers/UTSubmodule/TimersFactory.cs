using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EJaw.UnityTimers
{
    /// <summary>
    /// Logic of creation and reuse MonoBehaviourTimers
    /// </summary>
    public sealed class TimersFactory
    {
        #region Fields

        private Transform timersParent = null;
        private readonly List<MonoBehaviourTimer> monoTimers = new List<MonoBehaviourTimer>();
        private static TimersFactory instance = null;
        private const string PARENT_NAME = "_Timers";

        #endregion

        #region Properties

        public static TimersFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new TimersFactory();
                return instance;
            }
        }

        public MonoBehaviourTimer MonoTimer
        {
            get
            {
                var timer = monoTimers.FirstOrDefault(t => t != null && !t.gameObject.activeInHierarchy);
                if (timer == null)
                    timer = CreateMonoTimer();
                return timer;
            }
        }

        #endregion

        #region Functions

        private TimersFactory()
        {
            Init();
        }

        private MonoBehaviourTimer CreateMonoTimer()
        {
            if (timersParent == null)
                Init();

            if (timersParent != null)
            {
                var timerName = string.Format("Timer_{0}", timersParent.childCount);
                var mbt = (new GameObject(timerName)).AddComponent<MonoBehaviourTimer>();
                mbt.transform.parent = timersParent;
                mbt.transform.ResetLocal();
                monoTimers.Add(mbt);
                mbt.gameObject.SetActive(false);
                return mbt;
            }

            return null;
        }

        private void Init()
        {
            if (timersParent == null)
            {
                var go = GameObject.Find(PARENT_NAME);
                if (go == null)
                    go = new GameObject(PARENT_NAME);

                timersParent = go.transform;
                timersParent.ResetGlobal();
            }

            monoTimers.Clear();
            monoTimers.AddRange(timersParent.GetComponentsInChildren<MonoBehaviourTimer>());
        }

        #endregion
    }
}