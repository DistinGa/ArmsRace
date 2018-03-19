﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LeftFlagEvents
{
    public interface ILFEvent
    {
        GameObject GO { get;}
        LeftFlagEventManager LFManager { get; set; }
        bool TestSelfCondition();
        void OnClickEvent();
    }

    public class LeftFlagEventManager : MonoBehaviour
    {
        public GameObject SetAgressionFlagEventButton;
        public GameObject SetCHGovFlagEventButton;
        public GameObject CanChangeGE;

        List<ILFEvent> listLFE = new List<ILFEvent>();
        GameManagerScript GM;

        void Start()
        {
            GM = GameManagerScript.GM;
            GM.SubscribeMonth(checkLFStatus);
        }

        void checkLFStatus()
        {
            //проверка на актуальность значка
            ILFEvent item;
            for (int i = listLFE.Count - 1; i >= 0; i--)
            {
                item = listLFE[i];
                if (!item.TestSelfCondition())
                {
                    Destroy(item.GO);
                    listLFE.Remove(item);
                }
            }

            //поиск новых
            foreach (CountryScript c in CountryScript.Countries())
            {
                //страны, где можно начать войну
                if (c.CanAgression(GM.Player.Authority))
                {
                    if (listLFE.Where(x=>(x is AgressionEventFlagScript) && (x as AgressionEventFlagScript).Country == c).Count() > 0)    //в списке уже есть такой флаг
                        continue;

                    GameObject tmpGO = Instantiate(SetAgressionFlagEventButton);
                    tmpGO.transform.SetParent(transform.FindChild("Panel/Flags"));
                    tmpGO.transform.SetAsFirstSibling();
                    AgressionEventFlagScript tmpAE = tmpGO.GetComponent<AgressionEventFlagScript>();
                    tmpAE.Country = c;
                    listLFE.Add(tmpAE);
                }

                //страны, где можно сменить правительство
                if (c.CanChangeGov(GM.Player.Authority))
                {
                    if (listLFE.Where(x => (x is ChGovEventFlagScript) && (x as ChGovEventFlagScript).Country == c).Count() > 0)    //в списке уже есть такой флаг
                        continue;

                    GameObject tmpGO = Instantiate(SetCHGovFlagEventButton);
                    tmpGO.transform.SetParent(transform.FindChild("Panel/Flags"));
                    tmpGO.transform.SetAsFirstSibling();
                    ChGovEventFlagScript tmpAE = tmpGO.GetComponent<ChGovEventFlagScript>();
                    tmpAE.Country = c;
                    listLFE.Add(tmpAE);
                }
            }

            //Возможность влиять на глобальные последствия
            if (GM.Player.TYGEDiscounter > 0)
            {
                if (listLFE.Where(x => (x is CanChangeGE)).Count() == 0)    //в списке ещё нет такого флага
                {
                    GameObject tmpGO = Instantiate(CanChangeGE);
                    tmpGO.transform.SetParent(transform.FindChild("Panel/Flags"));
                    tmpGO.transform.SetAsFirstSibling();
                    CanChangeGE tmpAE = tmpGO.GetComponent<CanChangeGE>();
                    listLFE.Add(tmpAE);
                }
            }

            //DLC
            if (SettingsScript.Settings.UNAvailable)
                GM.DLC_UN.ShowFlag();
        }

        //public void AddFlag(GameObject tmpGO, CountryScript c = null)
        //{
        //    if (tmpGO.GetComponent<UNCondemnAggressorFlagScript>() != null && HasCondemnFlag())    //такой флаг уже есть в списке
        //    {
        //        Destroy(tmpGO);
        //        return;
        //    }

        //    tmpGO.transform.SetParent(transform.FindChild("Panel/Flags"));
        //    tmpGO.transform.SetAsFirstSibling();
        //    ILFEvent tmpAE = tmpGO.GetComponent<ILFEvent>();
        //    tmpAE.LFManager = this;
        //    listLFE.Add(tmpAE);
        //}

        public void AddFlag(GameObject prefab)
        {
            GameObject tmpGO = Instantiate(prefab);
            tmpGO.transform.SetParent(transform.FindChild("Panel/Flags"));
            tmpGO.transform.SetAsFirstSibling();
            ILFEvent LFEvent = tmpGO.GetComponent<ILFEvent>();
            LFEvent.LFManager = this;

            listLFE.Add(LFEvent);
        }

        public void DelFlag(ILFEvent item)
        {
            listLFE.Remove(item);
            Destroy(item.GO);
        }

        bool HasUNFlag()
        {
            return (listLFE.Where(x => (x is UNFlagScript)).Count() > 0);
        }
    }
}