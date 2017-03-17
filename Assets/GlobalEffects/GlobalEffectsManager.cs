using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace GlobalEffects
{
    public class GlobalEffectsManager : MonoBehaviour
    {
        public static GlobalEffectsManager GeM;

        public SOGlobalEffectsPrefs SOGEPrefs;
        public GlobalEffectsMenu Menu;
        public RectTransform ButtonPrefab;

        [SerializeField]
        GlobalEffectObject[] GlobalEffectsList = new GlobalEffectObject[6];
        int curDecade = -1; //(сохраняется)

        void Awake()
        {
            GeM = this;
        }

        // Use this for initialization
        void Start()
        {
            GameManagerScript.GM.SubscribeMonth(MonthTick);
            changeGE(0);    //при нулевом месяце не вызывается делегат
        }

        void MonthTick()
        {
            GameManagerScript GM = GameManagerScript.GM;

            //смена десятилетия
            if (GM.CurrentMonth % 120 == 0)
            {
                changeGE((int)(GM.CurrentMonth / 120));
            }

            //обсчёт глобальных последствий
            foreach (GlobalEffectObject ge in GlobalEffectsList)
            {
                int resStep = ge.DoStep();

                //отображение кнопки на верхнем меню
                if (resStep != 0)
                {
                    RectTransform newGO = Instantiate(ButtonPrefab);
                    newGO.GetComponent<Image>().sprite = ge.icon;

                    if (resStep < 1)    //usa
                    {
                        newGO.parent = GameObject.Find("UpMenu/LefttGEPanel").transform;
                    }
                    else if (resStep > 1)   //ussr
                    {
                        newGO.parent = GameObject.Find("UpMenu/RightGEPanel").transform;
                    }

                    SoundManager.SM.PlaySound("ots4et");
                }
            }

            if (Menu.gameObject.activeSelf)
                Menu.UpdateView();
        }

        //смена активных глобальных последствий
        void changeGE(int decade)
        {
            if (decade == curDecade)
                return;

            GlobalEffectObject[] GEs = GetDecadeGEs(decade);

            for (int i = 0; i < GEs.Length; i++)
            {
                GlobalEffectsList[i].eventName = GEs[i].eventName;
                GlobalEffectsList[i].sovDescription = GEs[i].sovDescription;
                GlobalEffectsList[i].usaDescription = GEs[i].usaDescription;
                GlobalEffectsList[i].picture = GEs[i].picture;
                GlobalEffectsList[i].sovGPPLimit = GEs[i].sovGPPLimit;
                GlobalEffectsList[i].usaGPPLimit = GEs[i].usaGPPLimit;

                GlobalEffectsList[i].counter = 0;
                GlobalEffectsList[i].sovGPP = 0;
                GlobalEffectsList[i].usaGPP = 0;
            }

            curDecade = decade;

            //удаление иконок произошедших событий
            Transform trPanel;
            trPanel = GameObject.Find("UpMenu/LefttGEPanel").transform;
            foreach (Transform item in trPanel)
            {
                Destroy(item);
            }

            trPanel = GameObject.Find("UpMenu/RightGEPanel").transform;
            foreach (Transform item in trPanel)
            {
                Destroy(item);
            }
        }


        //Если передано текущее десятилетие, возвращает GlobalEffectsList.
        //Если другое десятилетие - события из файла настроек (SOGEPrefs).
        public GlobalEffectObject[] GetDecadeGEs(int decade = -1)
        {
            int count = GlobalEffectsList.Length;
            GlobalEffectObject[] res = new GlobalEffectObject[count];

            if (decade == -1 || decade == curDecade)
            {
                res = GlobalEffectsList;
            }
            else
            {
                //curDecade = decade;

                for (int i = 0; i < count; i++)
                {
                    res[i] = new GlobalEffectObject(SOGEPrefs.GlobalEffectsPreferences[decade * count + i]);
                }
            }

            return res;
        }

        public void ChangeCountersOnWar()
        {
            int randV;

            foreach (var item in GlobalEffectsList)
            {
                randV = Random.Range(0, 100);

                if (randV < 50)
                    item.counter--;
                else
                    item.counter++;
            }
        }

        public GlobalEffectsData GetSavedData()
        {
            GlobalEffectsData res = new GlobalEffectsData();

            res.curDecade = curDecade;
            res.geObjArray = new int[6, 3];
            for (int i = 0; i < GlobalEffectsList.Length; i++)
            {
                res.geObjArray[i, 0] = GlobalEffectsList[i].counter;
                res.geObjArray[i, 1] = GlobalEffectsList[i].sovGPP;
                res.geObjArray[i, 2] = GlobalEffectsList[i].usaGPP;
            }

            return res;
        }

        public void SetSavedData(GlobalEffectsData geData)
        {
            curDecade = geData.curDecade;
            for (int i = 0; i < GlobalEffectsList.Length; i++)
            {
                GlobalEffectsList[i].counter = geData.geObjArray[i, 0];
                GlobalEffectsList[i].sovGPP = geData.geObjArray[i, 1];
                GlobalEffectsList[i].usaGPP = geData.geObjArray[i, 2];
            }
        }
    }
}
