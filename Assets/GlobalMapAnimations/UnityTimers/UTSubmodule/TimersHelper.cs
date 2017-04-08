using System;
using UnityEngine;

namespace EJaw.UnityTimers
{
    public static class TimersHelper
    {
        /// <summary>
        /// TransformExtensions. Set default local transform values
        /// </summary>
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// TransformExtensions. Set default global transform values
        /// </summary>
        public static void ResetGlobal(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.Euler(Vector3.zero);
            transform.localScale = Vector3.one;
        }

        public static void SendEvent(Action action)
        {
            var handler = action;
            if (handler != null)
                handler();
        }

        public static void SendEvent<T1>(Action<T1> action, T1 p1)
        {
            var handler = action;
            if (handler != null)
                handler(p1);
        }
    }
}
