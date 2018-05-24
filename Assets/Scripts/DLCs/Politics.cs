using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DLC_Politics
{
    public class Politics : MonoBehaviour
    {
        public PoliticsMenu PolitMenu;
        public List<PolMinister> USMinisters = new List<PolMinister>();
        public List<PolMinister> SovMinisters = new List<PolMinister>();
        public GameObject btnSov, btnUS;    //Кнопки над флагами верхнего меню для открытия окна лидера/министров.

        int openedSlots = 0;
        //Министры на текущую декаду.
        PolMinister[] curUSMinisters = new PolMinister[3];
        PolMinister[] curSovMinisters = new PolMinister[3];

        GameManagerScript GM;

#region Properties
        public int OpenedSlots
        {
            get { return openedSlots; }
        }
#endregion

        void Start()
        {
            GM = GameManagerScript.GM;
            bool PolCheck = SettingsScript.Settings.PoliticsAvailable;
            btnSov.SetActive(PolCheck);
            btnUS.SetActive(PolCheck);
        }

        public PolMinister[] curMinisters(Authority auth)
        {
            if (auth == Authority.Amer)
                return curUSMinisters;
            else if (auth == Authority.Soviet)
                return curSovMinisters;
            else
            {
                Debug.LogError("Authority is incorrect");
            }

            return null;
        }

        public void ApplyMonthBonuses(PlayerScript pl)
        {
            foreach (PolMinister item in curMinisters(pl.Authority))
            {
                switch (item.BonusType)
                {
                    case BonusType.MonSupAlli:
                        //0.1 monthly support grow in alliance ( каждый месяц +0.1 influence во все страны альянса )
                        foreach (CountryScript c in CountryScript.Countries(pl.Authority))
                        {
                            c.Support += 0.1f;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void ApplyImmediateBonuses(PlayerScript pl)
        {
            pl.TYGEDiscounter = pl.PlayerLeader.GetTenYearsGlobalEffectsChange();

            foreach (PolMinister item in curMinisters(pl.Authority))
            {
                if (item.BonusType == BonusType.GEPoint && pl == GM.Player) //Для игрока этот бонус применяем сразу после выборов, т.к. годовое применение бонусов произойдёт раньше, чем игрок успеет проголосовать
                {
                    //+1 global consequences point per 5 years (добавляется 1 в начале сразу после выборов и еще +1 через 5 лет)
                    pl.TYGEDiscounter++;
                    //print(GM.GetCurrentDate() + ": " + pl.MyCountry.Name + " add GC point");
                }

                if (item.BonusType == BonusType.Industry)
                {
                    //+ 2 industry to USSR/USA ( одноразовая постройка, когда перевыборы и политик уходит то построенные +2 остаются )
                    pl.MyCountry.Score += 2;
                }

                if (item.BonusType == BonusType.YearInfToFoe && pl == GM.Player)   //Для игрока этот бонус применяем сразу после выборов, т.к. годовое применение бонусов произойдёт раньше, чем игрок успеет проголосовать
                {
                    //- 1 influence to foe once per two years ( каждые 2 года -1 influence во все страны альянса противника )
                    //Это первый год (год выборов)
                    Authority oppAuth = GM.GetOpponentTo(pl).Authority;
                    foreach (CountryScript c in CountryScript.Countries(oppAuth))
                    {
                        c.AddInfluence(oppAuth, -1, true);
                    }
                }
            }

            //Пересчёт стоимости текущих технологий
            pl.Outlays[OutlayField.air].SetNewCost(GM.MDInstance.GetTechCost(OutlayField.air, pl.GetCurMilTech(OutlayField.air), pl));
            pl.Outlays[OutlayField.ground].SetNewCost(GM.MDInstance.GetTechCost(OutlayField.ground, pl.GetCurMilTech(OutlayField.ground), pl));
            pl.Outlays[OutlayField.sea].SetNewCost(GM.MDInstance.GetTechCost(OutlayField.sea, pl.GetCurMilTech(OutlayField.sea), pl));
            pl.Outlays[OutlayField.rocket].SetNewCost(GM.MDInstance.GetTechCost(OutlayField.rocket, pl.GetCurMilTech(OutlayField.rocket), pl));

            if (pl.Outlays[OutlayField.spaceGround].Cost != 0)
                pl.Outlays[OutlayField.spaceGround].SetNewCost(GM.SRInstance.GetTechCost(pl.CurGndTechIndex, pl));
            if (pl.Outlays[OutlayField.spaceLaunches].Cost != 0)
                pl.Outlays[OutlayField.spaceLaunches].SetNewCost(GM.SRInstance.GetTechCost(pl.CurLnchTechIndex, pl));

            pl.Outlays[OutlayField.spy].SetNewCost(GM.GetSpyCost(pl));
            pl.Outlays[OutlayField.military].SetNewCost(GM.GetMilitaryCost(pl));
            pl.Outlays[OutlayField.diplomat].SetNewCost(GM.GetDiplomatCost(pl));

        }


        public void ApplyAnnualBonuses(PlayerScript pl)
        {
            foreach (PolMinister item in curMinisters(pl.Authority))
            {
                switch (item.BonusType)
                {
                    case BonusType.GEPoint:
                        //+1 global consequences point per 5 years (добавляется 1 в начале сразу после выборов и еще +1 через 5 лет)
                        if (GM.CurrentMonth % 60 == 0)  //1 раз через 5 лет
                            pl.TYGEDiscounter ++;
                        break;
                    case BonusType.YearInfToFoe:
                        if (GM.CurrentMonth % 24 == 0)
                        {
                            //- 1 influence to foe once per two years ( каждые 2 года -1 influence во все страны альянса противника )
                            Authority oppAuth = GM.GetOpponentTo(pl).Authority;
                            foreach (CountryScript c in CountryScript.Countries(oppAuth))
                            {
                                c.AddInfluence(oppAuth, -1, true);
                            }
                        }
                        break;
                    case BonusType.YearMil:
                        //+ 2 military yearly (игрок получает каждый год 2 милитари в свой пул)
                        pl.MilitaryPool += 2;
                        break;
                    case BonusType.YearBudgetGain:
                        //+ 50 to budget yearly
                        pl.Budget += 50;
                        break;
                    default:
                        break;
                }
            }
        }

        public int GetFPBonus(PlayerScript pl)
        {
            //+ 10 firepower (так же как и с лидером, +10 пока этот политик выбран ( тоесть на 10 лет ))
            int res = 0;

            foreach (PolMinister item in curMinisters(pl.Authority))
            {
                if (item != null && item.BonusType == BonusType.FPGain)
                {
                    res += 10;
                }
            }

            return res;
        }

        public float GetMilTechDiscount(PlayerScript pl)
        {
            //military tech 5% cheaper (так же как и с лидером, скидка пока этот политик выбран ( тоесть на 10 лет ))
            float res = 0;

            foreach (PolMinister item in curMinisters(pl.Authority))
            {
                if (item != null && item.BonusType == BonusType.MilTechDiscount)
                {
                    res += 0.05f;
                }
            }

            return res;
        }

        public bool FreeParadRiot(PlayerScript pl)
        {
            //no cost for parades and riots
            bool res = false;

            foreach (PolMinister item in curMinisters(pl.Authority))
            {
                if (item != null && item.BonusType == BonusType.FreeParadeRiot)
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        public void ShowPolMenu(PlayerScript pl)
        {
            PolitMenu.ShowMenu(pl.Authority, false);
        }

        public void StartVotings(PlayerScript pl)
        {
            PolitMenu.ShowMenu(pl.Authority, true);
        }

        public void AIVotings(PlayerScript pl)
        {
            //Выбор министров, соответствующих лидеру
            List<PolMinister> tmpMinisters, tmpMinisters2;
            PolMinister[] arrMinisters = new PolMinister[3];

            int dec = 1950 + GM.CurrentMonth / 120 * 10;
            if (pl.Authority == Authority.Amer)
                tmpMinisters = USMinisters.Where(m => m.InDecade(dec)).ToList();
            else
                tmpMinisters = SovMinisters.Where(m => m.InDecade(dec)).ToList();

            int indx;
            switch (pl.PlayerLeader.ActualLeaderType)
            {
                case LeaderType.Economic:
                    //все либералы
                    tmpMinisters2 = tmpMinisters.Where(m => m.Liberal && m.LeaderType == LeaderType.Diplomatic).ToList();
                    indx = Random.Range(0, tmpMinisters2.Count);
                    arrMinisters[0] = tmpMinisters2[indx];

                    tmpMinisters2 = tmpMinisters.Where(m => m.Liberal && m.LeaderType == LeaderType.Economic).ToList();
                    indx = Random.Range(0, tmpMinisters2.Count);
                    arrMinisters[1] = tmpMinisters2[indx];

                    tmpMinisters2 = tmpMinisters.Where(m => m.Liberal && m.LeaderType == LeaderType.Militaristic).ToList();
                    indx = Random.Range(0, tmpMinisters2.Count);
                    arrMinisters[2] = tmpMinisters2[indx];

                    break;
                case LeaderType.Militaristic:
                    //все консерваторы
                    tmpMinisters2 = tmpMinisters.Where(m => !m.Liberal && m.LeaderType == LeaderType.Diplomatic).ToList();
                    indx = Random.Range(0, tmpMinisters2.Count);
                    arrMinisters[0] = tmpMinisters2[indx];

                    tmpMinisters2 = tmpMinisters.Where(m => !m.Liberal && m.LeaderType == LeaderType.Economic).ToList();
                    indx = Random.Range(0, tmpMinisters2.Count);
                    arrMinisters[1] = tmpMinisters2[indx];

                    tmpMinisters2 = tmpMinisters.Where(m => !m.Liberal && m.LeaderType == LeaderType.Militaristic).ToList();
                    indx = Random.Range(0, tmpMinisters2.Count);
                    arrMinisters[2] = tmpMinisters2[indx];

                    break;
                case LeaderType.Diplomatic:
                    //смешанная группа
                    tmpMinisters2 = tmpMinisters.Where(m => m.LeaderType == LeaderType.Diplomatic).ToList();  //в tmpConservators и консерваторы, и либералы (чтобы отдельную переменную не заводить)
                    indx = Random.Range(0, tmpMinisters2.Count);
                    arrMinisters[0] = tmpMinisters2[indx];

                    tmpMinisters2 = tmpMinisters.Where(m => m.LeaderType == LeaderType.Economic).ToList();
                    indx = Random.Range(0, tmpMinisters2.Count);
                    arrMinisters[1] = tmpMinisters2[indx];

                    if (arrMinisters[0].Liberal != arrMinisters[1].Liberal)
                    {
                        tmpMinisters2 = tmpMinisters.Where(m => m.LeaderType == LeaderType.Militaristic).ToList();
                        indx = Random.Range(0, tmpMinisters2.Count);
                        arrMinisters[2] = tmpMinisters2[indx];
                    }
                    else if (arrMinisters[0].Liberal && arrMinisters[1].Liberal)
                    {
                        tmpMinisters2 = tmpMinisters.Where(m => !m.Liberal && m.LeaderType == LeaderType.Militaristic).ToList();
                        indx = Random.Range(0, tmpMinisters2.Count);
                        arrMinisters[2] = tmpMinisters2[indx];
                    }
                    else
                    {
                        tmpMinisters2 = tmpMinisters.Where(m => m.Liberal && m.LeaderType == LeaderType.Militaristic).ToList();
                        indx = Random.Range(0, tmpMinisters2.Count);
                        arrMinisters[2] = tmpMinisters2[indx];
                    }
                    break;
                default:
                    break;
            }

            ApplyVotingsResult(pl, pl.PlayerLeader.LeaderID, pl.PlayerLeader.LeaderType, arrMinisters);
        }

        //Применение результатов выборов
        public void ApplyVotingsResult(PlayerScript pl, int LeaderIndx, LeaderType LeaderType, PolMinister[] Ministers)
        {
            if (GM.CurrentMonth != 0 && pl.PlayerLeader.LeaderID == LeaderIndx) //Нулевой месяц не проверяем, т.к. в первые выборы лидер совпадает с выбранным в лобби.
            {
                //Стагнация
                GlobalEffects.GlobalEffectsManager.GeM.Stagnation(pl.Authority, 2, 1);
            }

            pl.PlayerLeader.LeaderID = LeaderIndx;
            pl.PlayerLeader.LeaderType = LeaderType;
            if (pl.Authority == Authority.Amer)
                //curUSMinisters = Ministers;
                Ministers.CopyTo(curUSMinisters, 0);
            else
                //curSovMinisters = Ministers;
                Ministers.CopyTo(curSovMinisters, 0);

            ApplyImmediateBonuses(pl);
        }

        public void OpenSlot(Authority auth)
        {
            if (openedSlots == 3)
                return;

            openedSlots++;
            //Влияние на глобальные последствия
            GlobalEffects.GlobalEffectsManager.GeM.Stagnation(auth, 1, 1);
#if !DEBUG
            if (openedSlots == 3)
                SteamManager.UnLockAchievment("Politics");
#endif
        }

        public PolitData GetSavedData()
        {
            PolitData res = new PolitData();
            res.USMinisterIndxs = new int[3];
            res.SovMinisterIndxs = new int[3];

            res.SlotsOpened = OpenedSlots;
            for (int i = 0; i < 3; i++)
            {
                res.USMinisterIndxs[i] = USMinisters.FindIndex(m => m.Desc == curUSMinisters[i].Desc);
                res.SovMinisterIndxs[i] = SovMinisters.FindIndex(m => m.Desc == curSovMinisters[i].Desc);
            }

            return res;
        }

        public void SetSavedData(PolitData PolitData)
        {
            openedSlots = PolitData.SlotsOpened;
            for (int i = 0; i < 3; i++)
            {
                curUSMinisters[i] = USMinisters[PolitData.USMinisterIndxs[i]];
                curSovMinisters[i] = SovMinisters[PolitData.SovMinisterIndxs[i]];
            }
        }
    }

    [System.Serializable]
    public class PolMinister
    {
        public string Desc;
        public Sprite Photo;
        public bool Liberal;
        public BonusType BonusType;
        public LeaderType LeaderType;
        public List<int> Decades = new List<int>();

        public bool InDecade(int dec)
        {
            bool res = false;

            res = Decades.Exists(d => d == dec);

            return res;
        }

        public string GetBonusDescr()
        {
            string res = "";

            switch (BonusType)
            {
                case BonusType.GEPoint:
                    res = "+1 global consequences point per 5 years";
                    break;
                case BonusType.MonSupAlli:
                    res = "0.1 monthly support grow in alliance";
                    break;
                case BonusType.YearInfToFoe:
                    res = "-1 influence to foe once per two years";
                    break;
                case BonusType.YearMil:
                    res = "+2 military yearly";
                    break;
                case BonusType.FPGain:
                    res = "+10 firepower";
                    break;
                case BonusType.MilTechDiscount:
                    res = "military technology cost -5%";
                    break;
                case BonusType.FreeParadeRiot:
                    res = "no cost for parades and riots";
                    break;
                case BonusType.YearBudgetGain:
                    res = "+50 to budget yearly";
                    break;
                case BonusType.Industry:
                    res = "+2 scores to homeland";
                    break;
                default:
                    break;
            }

            return res;
        }
    }

    public enum BonusType
    {
        GEPoint,
        MonSupAlli,
        YearInfToFoe,

        YearMil,
        FPGain,
        MilTechDiscount,

        FreeParadeRiot,
        YearBudgetGain,
        Industry
    }
}