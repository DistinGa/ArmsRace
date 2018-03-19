using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UN : MonoBehaviour {
    public GameObject UNButton;
    public UNMenu UNMenu;

    int[,] arPrestGains = new int[5, 3];

    [SerializeField]
    UnityEngine.UI.Text UNPrestigeUSSR, UNPrestigeUSA;
    [SerializeField]
    int MonPrestigeIncValue = 1, AlliPrestigeIncValue = 40, MilPrestigeIncValue = 1, SpyPrestigeIncValue = 1;
    [SerializeField]
    GameObject UNFlag;
    [SerializeField]
    int CondemnAggressorCost = 50, PeacekeepingCost = 50, InterventionCost = 50, SpeechCost = 50;

    int amPrestige, amMonPrestige, sovPrestige, sovMonPrestige;
    GameManagerScript GM;

    #region Properties
    public int GetCondemnAggressorCost
    {
        get { return CondemnAggressorCost; }
    }

    public int GetPeacekeepingCost
    {
        get { return PeacekeepingCost; }
    }

    public int GetInterventionCost
    {
        get { return InterventionCost; }
    }

    public int GetSpeechCost
    {
        get { return SpeechCost; }
    }

    public int PrestGainsCount
    {
        get { return arPrestGains.GetLength(0); }
    }

    public int AlliPrestGain
    {
        get { return AlliPrestigeIncValue; }
    }

    public int MilPrestGain
    {
        get { return MilPrestigeIncValue; }
    }

    public int SpyPrestGain
    {
        get { return SpyPrestigeIncValue; }
    }
    #endregion

    void Start () {
        GM = GameManagerScript.GM;
        bool UNCheck = SettingsScript.Settings.UNAvailable;
        UNButton.SetActive(UNCheck);
        UNPrestigeUSSR.gameObject.SetActive(UNCheck);
        UNPrestigeUSA.gameObject.SetActive(UNCheck);

        if (UNCheck)
        {
            GM.ExtractUpMenu(35.55f);
            GM.SubscribeMonth(UNViewTick);
            UNPrestigeUSA.text = amPrestige.ToString();
            UNPrestigeUSSR.text = sovPrestige.ToString();
        }
    }

    public void UNViewTick()
    {
        if (UNMenu.isActiveAndEnabled)
            UNMenu.UpdateView();
    }

    public void UNActionTick()
    {
        int suprScore, suprInf, suprArma;
        CheckMonSupremacy(out suprScore, out suprInf, out suprArma);

        if (suprScore > 0)
            PrestigeAddMonthly(GM.Player.Authority);
        else if (suprScore < 0)
            PrestigeAddMonthly(GM.AI.AIPlayer.Authority);

        if (suprInf > 0)
            PrestigeAddMonthly(GM.Player.Authority);
        else if (suprInf < 0)
            PrestigeAddMonthly(GM.AI.AIPlayer.Authority);

        if (suprArma > 0)
            PrestigeAddMonthly(GM.Player.Authority);
        else if (suprArma < 0)
            PrestigeAddMonthly(GM.AI.AIPlayer.Authority);

        AIAction();
    }

    //Положительное значение возвращаемого параметра - превосходство игрока, отрицательное - превосходство ИИ, ноль - нет превосходства
    public void CheckMonSupremacy(out int suprScore, out int suprInf, out int suprArma)
    {
        suprScore = 0;
        suprInf = 0;
        suprArma = 0;

        int plScore = GM.GetPlayerByAuthority(GM.Player.Authority).Score;
        int oppScore = GM.GetPlayerByAuthority(GM.AI.AIPlayer.Authority).Score;

        int plInf = 0, oppInf = 0;
        foreach (CountryScript c in CountryScript.Countries())
        {
            if (GM.Player.Authority == Authority.Amer)
            {
                plInf += c.AmInf;
                oppInf += c.SovInf;
            }
            else
            {
                plInf += c.SovInf;
                oppInf += c.AmInf;
            }
        }

        if (plInf > oppInf)
            suprInf = 1;
        else if (plInf < oppInf)
            suprInf = -1;

        if (plScore > oppScore)
            suprScore = 1;
        else if (plScore < oppScore)
            suprScore = -1;

        if (SettingsScript.Settings.ArmageddonAvailable)
        {
            int plNP = GM.Player.RelativeNuclearPower();
            int oppNP = GM.AI.AIPlayer.RelativeNuclearPower();

            if (plNP > 50)
                suprArma = 1;
            else if (oppNP > 50)
                suprArma = -1;
        }
    }

    #region Resolutions
    //При этой опции глобальный инфлуенс противника понижается на 2, а того, кто пользуется этой опцией повышается на 1.
    public void CondemnAggressor(Authority starterAuth, Authority aggrAuth)
    {
        if (GetPrestige(starterAuth) < CondemnAggressorCost)
            return;

        foreach (CountryScript c in CountryScript.Countries())
        {
            c.AddInfluence(aggrAuth, -2, true);
            c.AddInfluence(starterAuth, 1, true);
        }

        AddPrestige(starterAuth, -CondemnAggressorCost);

        if (starterAuth == Authority.Amer)
            video3D.Video3D.V3Dinstance.AddTechNews(video3D.NonCitysAnim.UNOUSA, -1, GM.CurrentMonth, GM.GetPlayerByAuthority(starterAuth).MyCountry, "USA in UN condemned aggression of USSR. UN has supported this resolution ( -2 global influence for USA)", "", true);
        else
            video3D.Video3D.V3Dinstance.AddTechNews(video3D.NonCitysAnim.UNOUSSR, -1, GM.CurrentMonth, GM.GetPlayerByAuthority(starterAuth).MyCountry, "USSR in UN condemned aggression of USA. UN has supported this resolution ( -2 global influence for USSR)", "", true);
    }

    //При этой опции можно остановить войну в любой стране, в которой воюет противник (скриптом опозиция и суппорт в стране выравнивается 50 на 50 ).
    public void Peacekeeping(Authority starterAuth, CountryScript c)
    {
        if (GetPrestige(starterAuth) < PeacekeepingCost)
            return;

        c.Support = 50f;

        AddPrestige(starterAuth, -PeacekeepingCost);

        if (starterAuth == Authority.Amer)
            video3D.Video3D.V3Dinstance.AddTechNews(video3D.NonCitysAnim.UNOUSA, -1, GM.CurrentMonth, c, String.Format("USA in UN proposed peacekeeping mission to {0}. UN has supported this resolution ( 50/50 for opposition and support in {0})", c.Name), "", true);
        else
            video3D.Video3D.V3Dinstance.AddTechNews(video3D.NonCitysAnim.UNOUSSR, -1, GM.CurrentMonth, c, String.Format("USSR in UN proposed peacekeeping mission to {0}. UN has supported this resolution ( 50/50 for opposition and support in {0})", c.Name), "", true);
    }

    //При этой опции, наоборот, можно начать войну в любой нейтральной стране в которой свой ифлуенс меньше чем у противника ( скриптом опозиция становирся 90 и вводятся 3 милитари ( не игрока, а просто, с неба )). 
    //Особенность этого действия в том, что игрок может потом вводить туда войска, но страна не имеет статуса агрессора!
    public void Intervention(Authority starterAuth, CountryScript c)
    {
        if (GetPrestige(starterAuth) < InterventionCost)
            return;

        c.Support = 10f;
        c.AddMilitary(starterAuth, 3, true);

        AddPrestige(starterAuth, -InterventionCost);

        if (starterAuth == Authority.Amer)
            video3D.Video3D.V3Dinstance.AddTechNews(video3D.NonCitysAnim.UNOUSA, -1, GM.CurrentMonth, c, String.Format("USA in UN proposed mandate for intervention to {0}. UN has supported this resolution ( +3 military and opposition =90 in {0})", c.Name), "", true);
        else
            video3D.Video3D.V3Dinstance.AddTechNews(video3D.NonCitysAnim.UNOUSSR, -1, GM.CurrentMonth, c, String.Format("USSR in UN proposed mandate for intervention to {0}. UN has supported this resolution ( +3 military and opposition =90 in {0})", c.Name), "", true);
    }

    //При этой опции свой глобальный инфлуенс поднимается на 1 (в других странах забирается у противника, если у противника нет, то забирается нейтральный))
    public void Speech(Authority starterAuth)
    {
        if (GetPrestige(starterAuth) < SpeechCost)
            return;

        foreach (CountryScript c in CountryScript.Countries())
        {
            if (c.GetInfluense(Authority.Neutral) == 0) //Если нейтрального влияния нет, то сразу отнимается у противника
                c.AddInfluence(starterAuth, 1, true);
            else
            {
                //Если нейтральное влияние есть, то сначала добавляем вляиние инициатору, а потом отнимаем у противника ("взаиморасчёты" происходят через нейтральное влияние).
                c.AddInfluence(starterAuth, 1, true);
                c.AddInfluence(starterAuth == Authority.Amer ? Authority.Soviet : Authority.Amer, -1, true);
            }
        }

        AddPrestige(starterAuth, -SpeechCost);

        if (starterAuth == Authority.Amer)
            video3D.Video3D.V3Dinstance.AddTechNews(video3D.NonCitysAnim.UNOUSA, -1, GM.CurrentMonth, GM.GetPlayerByAuthority(starterAuth).MyCountry, "USA representative in UN has suggested new constructive ideas for global peace and prosperity. Most of UN members applause to new initiatives of USA", "", true);
        else
            video3D.Video3D.V3Dinstance.AddTechNews(video3D.NonCitysAnim.UNOUSSR, -1, GM.CurrentMonth, GM.GetPlayerByAuthority(starterAuth).MyCountry, "USSR representative in UN has suggested new constructive ideas for global peace and prosperity. Most of UN members applause to new initiatives of USSR", "", true);
    }
    #endregion

    public void PrestigeAddMonthly(Authority auth)
    {
        if (auth == Authority.Amer)
        {
            amPrestige += MonPrestigeIncValue;
        }
        else
        {
            sovPrestige += MonPrestigeIncValue;
        }
    }

    //auth - кто совершил действие
    //c - в какой стране
    public void AllliExpansion(Authority auth, CountryScript c)
    {
        AddPrestige(auth, AlliPrestigeIncValue);
        if(auth == GM.Player.Authority)
            AddPrestGain(c, GM.CurrentMonth, SingleGainType.AlliExpansion);

        UNViewTick();
    }

    //auth - кто совершил действие
    //c - в какой стране
    public void MilLiquidation(Authority auth, CountryScript c)
    {
        AddPrestige(auth, MilPrestigeIncValue);
        if (auth == GM.Player.Authority)
            AddPrestGain(c, GM.CurrentMonth, SingleGainType.MilLiquidation);

        UNViewTick();
    }

    //auth - кто совершил действие
    //c - в какой стране
    public void SpyLiquidation(Authority auth, CountryScript c)
    {
        AddPrestige(auth, SpyPrestigeIncValue);
        if (auth == GM.Player.Authority)
            AddPrestGain(c, GM.CurrentMonth, SingleGainType.SpyLiquidation);

        UNViewTick();
    }

    public int GetPrestige(Authority auth)
    {
        if (auth == Authority.Amer)
            return amPrestige;
        else
            return sovPrestige;
    }

    void AddPrestige(Authority auth, int addV)
    {
        if (auth == Authority.Amer)
        {
            amPrestige += addV;
            UNPrestigeUSA.text = amPrestige.ToString();
        }
        else
        {
            sovPrestige += addV;
            UNPrestigeUSSR.text = sovPrestige.ToString();
        }
    }
    
    //Определение возможностей для резолюций ООН
    public bool CheckLFConditions()
    {
        GameManagerScript GM = GameManagerScript.GM;

        int prest = GetPrestige(GM.Player.Authority);
        bool flagActiv = false;

        if (prest >= SpeechCost)
        {
            flagActiv = true;
        }

        if (!flagActiv && (prest >= CondemnAggressorCost || prest >= PeacekeepingCost || prest >= InterventionCost))
        {
            foreach (CountryScript c in CountryScript.Countries())
            {
                if (prest >= CondemnAggressorCost && c.AggressorAuth != GM.Player.Authority)
                {
                    flagActiv = true;
                    break;
                }

                if (prest >= PeacekeepingCost && c.OppForce > 0 && (c.Support < 40 || c.Support > 60))
                {
                    flagActiv = true; ;
                    break;
                }

                if (prest >= InterventionCost && c.Authority == Authority.Neutral && c.GetInfluense(GM.Player.Authority) < c.GetInfluense(GM.AI.AIPlayer.Authority) && c.OppForce == 0)
                {
                    flagActiv = true;
                    break;
                }
            }
        }

        return flagActiv;
    }

    //отображение флага на левой панели
    public void ShowFlag()
    {
        bool startSt = UNFlag.activeSelf;
        UNFlag.SetActive(CheckLFConditions());

        //Если флаг "включается", ставим его в начало.
        if (!startSt && UNFlag.activeSelf)
            UNFlag.transform.SetAsFirstSibling();
    }

    public bool CheckCondemnCondition(Authority auth)
    {
        foreach (CountryScript c in CountryScript.Countries())
        {
            if (GetPrestige(auth) >= CondemnAggressorCost && c.AggressorAuth != auth)
                return true;
        }
        return false;
    }

    public bool CheckPeacemakingCondition(Authority auth)
    {
        if (GetPrestige(auth) >= PeacekeepingCost)
            return true;
        else
            return false;
    }

    public bool CheckInterventionCondition(Authority auth)
    {
        if (GetPrestige(auth) >= InterventionCost)
            return true;
        else
            return false;
    }

    public bool CheckSpeechCondition(Authority auth)
    {
        if (GetPrestige(auth) >= SpeechCost)
            return true;
        else
            return false;
    }

    public List<CountryScript> GetPeacemakingCountries()
    {
        List<CountryScript> res = new List<CountryScript>();

        foreach (var c in CountryScript.Countries())
        {
            if (c.OppForce > 0 && (c.Support < 40 || c.Support > 60))
                res.Add(c);
        }

        return res;
    }

    public List<CountryScript> GetInterventionCountries()
    {
        List<CountryScript> res = new List<CountryScript>();

        foreach (var c in CountryScript.Countries())
        {
            if (c.Authority == Authority.Neutral && c.GetInfluense(GM.Player.Authority) < c.GetInfluense(GM.AI.AIPlayer.Authority) && c.OppForce == 0)
                res.Add(c);
        }

        return res;
    }

    //Только для игрока.
    //Сдвигает все строки вниз. Новую добавляет наверх.
    void AddPrestGain(CountryScript c, int month, SingleGainType tp)
    {
        for (int i = arPrestGains.GetLength(0) - 2; i > 0; i--)
        {
            arPrestGains[i + 1, 0] = arPrestGains[i, 0];
            arPrestGains[i + 1, 1] = arPrestGains[i, 1];
            arPrestGains[i + 1, 2] = arPrestGains[i, 2];
        }
        //страна
        arPrestGains[0, 0] = c.transform.GetSiblingIndex() + 1;
        //тип события
        arPrestGains[0, 0] = (int)tp;
        //месяц, когда произошло
        arPrestGains[0, 0] = month;

        UNMenu.AddPrestGainString(c, month, tp);
    }

    public void AIAction()
    {
        Authority auth = GM.AI.AIPlayer.Authority;

        if (CheckSpeechCondition(auth))
            Speech(auth);

        if (CheckCondemnCondition(auth))
            CondemnAggressor(auth, GM.Player.Authority);

        if (CheckPeacemakingCondition(auth))
        {
            foreach (var c in GetPeacemakingCountries())
            {
                if (c.Authority != GM.Player.Authority && GM.AI.AIPlayer.WinPercentForCountry(c) < 50)
                {
                    Peacekeeping(auth, c);
                    break;
                }
            }
        }

        if (CheckInterventionCondition(auth))
        {
            foreach (var c in GetInterventionCountries())
            {
                if (GM.AI.AIPlayer.WinPercentForCountry(c) >= 50 && c.GetInfluense(GM.Player.Authority) > 70 && c.Support < 30)
                {
                    Intervention(auth, c);
                    break;
                }
            }
        }
    }

    public enum SingleGainType
    {
        AlliExpansion,
        MilLiquidation,
        SpyLiquidation
    }
}
