using UnityEngine;
using System.Collections;
using System;

namespace LeftFlagEvents
{
    public class BaseEventFlagScript : MonoBehaviour, ILFEvent
    {
        private CameraScriptXZ scCamera;
        private LeftFlagEventManager lfMan;

        public GameObject GO
        {
            get
            {
                return gameObject;
            }
        }

        public LeftFlagEventManager LFManager
        {
            get
            {
                return lfMan;
            }

            set
            {
                lfMan = value;
            }
        }

        void Start()
        {
            scCamera = FindObjectOfType<CameraScriptXZ>();
        }

        public virtual void OnClickEvent()
        {
        }

        //Проверка условия для отображения флага
        public virtual bool TestSelfCondition()
        {
            return true;
        }

        public void OnMouseEnter()
        {
            scCamera.setOverMenu = true;
        }

        public void OnMouseExit()
        {
            scCamera.setOverMenu = false;
        }
    }
}