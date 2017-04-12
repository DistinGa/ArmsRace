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
                    AddIcon(ge, resStep);
                }
            }

            if (Menu.gameObject.activeSelf)
                Menu.UpdateView();
        }

        void AddIcon(GlobalEffectObject ge, int side)
        {
            RectTransform newGO = Instantiate(ButtonPrefab) as RectTransform;
            newGO.GetComponent<Image>().sprite = ge.icon;
            newGO.GetComponent<Button>().onClick.AddListener(() => GameManagerScript.GM.ToggleTechMenu(Menu.gameObject));
            newGO.GetComponent<Button>().onClick.AddListener(() => SoundManager.SM.PlaySound("sound/buttons"));

            if (side < 0)    //usa
            {
                newGO.SetParent(GameObject.Find("UpMenu/LefttGEPanel").transform);
            }
            else if (side > 0)   //ussr
            {
                newGO.SetParent(GameObject.Find("UpMenu/RightGEPanel").transform);
            }

            SoundManager.SM.PlaySound("sound/ots4et");
        }

        //смена активных глобальных последствий
        void changeGE(int decade)
        {
            if (decade == curDecade)
                return;

            GlobalEffectsList = GetDecadeGEs(decade);
            curDecade = decade;

            //удаление иконок произошедших событий
            Transform trPanel;
            trPanel = GameObject.Find("UpMenu/LefttGEPanel").transform;
            foreach (Transform item in trPanel)
            {
                item.GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(item.gameObject);
            }

            trPanel = GameObject.Find("UpMenu/RightGEPanel").transform;
            foreach (Transform item in trPanel)
            {
                item.GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(item.gameObject);
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
                for (int i = 0; i < count; i++)
                {
                    res[i] = new GlobalEffectObject(SOGEPrefs.GlobalEffectsPreferences[decade * count + i]);
                }
            }

            return res;
        }

        public void ChangeCountersOnWar()
        {
            GameManagerScript GM = GameManagerScript.GM;
            int randV = 50, randLimit = 100; //задел для читинга ИИ
            int GEChance = GM.AI.GECHance(SettingsScript.Settings.AIPower);

            //для отмеченных ГП вероятность смещения counter в сторону ИИ зависит от сложности игры
            foreach (var item in GlobalEffectsList)
            {
                if (item.AIPrioritet)
                {
                    if (GM.AI.AIPlayer.Authority == Authority.Soviet)
                        randV = GEChance;
                    else
                        randV = randLimit - GEChance;
                }
                else
                    randV = 50;

                if (Random.Range(0, randLimit) >= randV)
                    item.counter--; //usa
                else
                    item.counter++; //ussr
            }
        }

        //ИИ тратит бонусные очки ГП
        public void AIBonusSpending(Authority aut)
        {
            bool done = false;

            foreach (var item in GlobalEffectsList)
            {
                if (item.AIPrioritet)
                {
                    if (aut == Authority.Soviet)
                        item.counter++;
                    else
                        item.counter--;

                    done = true;
                    break;
                }
            }

            //если не было приоритетных событий, тратит на любое ГП
            if (!done)
            {
                var item = GlobalEffectsList[Random.Range(0, GlobalEffectsList.Length)];

                if (aut == Authority.Soviet)
                    item.counter++;
                else
                    item.counter--;
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


            changeGE(geData.curDecade);
            for (int i = 0; i < GlobalEffectsList.Length; i++)
            {
                GlobalEffectsList[i].counter = geData.geObjArray[i, 0];
                GlobalEffectsList[i].sovGPP = geData.geObjArray[i, 1];
                GlobalEffectsList[i].usaGPP = geData.geObjArray[i, 2];

                //показ иконок произошедших событий
                if (GlobalEffectsList[i].GEDone)
                {
                    if (GlobalEffectsList[i].sovGPP > GlobalEffectsList[i].usaGPP)
                        AddIcon(GlobalEffectsList[i], 1);
                    else
                        AddIcon(GlobalEffectsList[i], -1);
                }
            }
        }
    }
}
