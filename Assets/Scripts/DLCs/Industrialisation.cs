using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Industrialisation : MonoBehaviour {
    public int IndustrCount;
    public GameObject IndustrButton;
    public IndustrMenu IndustrMenu;
    List<IndustryElement> Industrs = new List<IndustryElement>();
    IndustryElement curAmerIndustr, curSovIndustr;

    void Start ()
    {
	    bool IndustrCheck = SettingsScript.Settings.IndustrAvailable;
        IndustrButton.SetActive(IndustrCheck);
        if (IndustrCheck)
        {
            GameManagerScript.GM.ExtractUpMenu(35.55f);
            GameManagerScript.GM.SubscribeMonth(IndustrTick);
        }
    }

    public void IndustrTick()
    {
        if (curAmerIndustr != null)
        {
            if (curAmerIndustr.PayCost())
            {
                Industrs.Add(curAmerIndustr);

                if (GameManagerScript.GM.Player.Authority == Authority.Amer)
                {
                    curAmerIndustr.DoneContractRow = IndustrMenu.AddDoneContract(curAmerIndustr);
                }

                StartVideoNews(curAmerIndustr.Cntr1, curAmerIndustr.Cntr2);

                curAmerIndustr = null;
            }
        }

        if (curSovIndustr != null)
        {
            if (curSovIndustr.PayCost())
            {
                Industrs.Add(curSovIndustr);

                if (GameManagerScript.GM.Player.Authority == Authority.Soviet)
                {
                    curSovIndustr.DoneContractRow = IndustrMenu.AddDoneContract(curSovIndustr);
                }

                StartVideoNews(curSovIndustr.Cntr1, curSovIndustr.Cntr2);

                curSovIndustr = null;
            }
        }

        //Проверка актуальности предприятий. Если хотя бы одна из стран отошла врагу, предприятие разрушается.
        List<IndustryElement> listToDel = new List<IndustryElement>();
        foreach (var item in Industrs)
        {
            if (item.Player.Authority != item.Cntr1.Authority || (item.Player.Authority != item.Cntr2.Authority && item.Cntr2.Authority != Authority.Neutral))
                listToDel.Add(item);
        }

        foreach (var item in listToDel)
        {
            item.DelDoneContractRow();
            Industrs.Remove(item);
        }

        if(curAmerIndustr != null)
        {
            if (curAmerIndustr.Player.Authority != curAmerIndustr.Cntr1.Authority || (curAmerIndustr.Player.Authority != curAmerIndustr.Cntr2.Authority && curAmerIndustr.Cntr2.Authority != Authority.Neutral))
                curAmerIndustr = null;
        }

        if (curSovIndustr != null)
        {
            if (curSovIndustr.Player.Authority != curSovIndustr.Cntr1.Authority || (curSovIndustr.Player.Authority != curSovIndustr.Cntr2.Authority && curSovIndustr.Cntr2.Authority != Authority.Neutral))
                curSovIndustr = null;
        }

        ///Обновление меню.
        if (IndustrMenu.gameObject.activeSelf)
        {
            IndustrMenu.UpdateView();
        }
    }

    public void StartVideoNews(CountryScript country1, CountryScript country2)
    {
        string NewsText = string.Format("{0} and {1} has developed cooperative industrialization project", country1.Name, country2.Name);
        video3D.Video3D.V3Dinstance.AddNews(country1.CapitalScene, country1.GetAnimObject(video3D.CitysAnim.Industry), GameManagerScript.GM.CurrentMonth, country1, NewsText, "", true);
    }

    public void AnnualUnitsGain()
    {
        for (int i = Industrs.Count - 1; i >= 0; i--)
        {
            Industrs[i].UnitGain();
        }
    }

    public List<IndustryElement> GetIndustrList(PlayerScript pl)
    {
        List<IndustryElement> res = new List<IndustryElement>();

        foreach (var item in Industrs)
        {
            if (item.Player == pl)
                res.Add(item);
        }

        return res;
    }

    public IndustryElement GetCurIndustr(PlayerScript pl)
    {
        return GetCurIndustr(pl.Authority);
    }

    public IndustryElement GetCurIndustr(Authority aut)
    {
        if (aut == Authority.Amer)
            return curAmerIndustr;
        else
            return curSovIndustr;
    }

    public void SetCurIndustr(IndustryElement industrEl)
    {
        if (industrEl.Player.Authority == Authority.Amer)
            curAmerIndustr = industrEl;
        else
            curSovIndustr = industrEl;
    }

    public void DestroyCurIndustr(PlayerScript pl)
    {
        if (pl.Authority == Authority.Amer)
            curAmerIndustr = null;
        else
            curSovIndustr = null;
    }

    //Основная страна может быть только своя страна или страна альянса.
    public List<CountryScript> GetAvailableCountriesPrimary(Authority auth, CountryScript outCountry = null)
    {
        List<CountryScript> resList;

        resList = CountryScript.Countries(auth);
        if(outCountry != null)
            resList.Remove(outCountry);

        foreach (var item in Industrs)
        {
            resList.Remove(item.Cntr1);
            resList.Remove(item.Cntr2);
        }

        return resList;
    }

    //Страной партнером может быть или страна альянса или нейтральная страна.
    public List<CountryScript> GetAvailableCountriesSecondary(Authority auth, CountryScript outCountry = null)
    {
        List<CountryScript> resList;

        resList = CountryScript.Countries(auth);
        resList.AddRange(CountryScript.Countries(Authority.Neutral));
        resList.Remove(GameManagerScript.GM.GetPlayerByAuthority(auth).MyCountry);
        if (outCountry != null)
            resList.Remove(outCountry);

        foreach (var item in Industrs)
        {
            resList.Remove(item.Cntr1);
            resList.Remove(item.Cntr2);
        }

        return resList;
    }

    public bool StartIndustrialization(CountryScript cntr1, CountryScript cntr2, IndustryType indstrType, PlayerScript pl)
    {
        IndustryElement curIndstr = GetCurIndustr(pl);
        if (curIndstr != null)
            return false;   //есть недостроенная индустриализация

        //проверка доступности стран для строительства
        if (cntr1.Authority == pl.Authority && (cntr2.Authority == pl.Authority) || cntr2.Authority == Authority.Neutral)
        {
            SetCurIndustr(new IndustryElement(cntr1, cntr2, indstrType, pl));
            return true;
        }
        else
            //До постройки у какой-либо страны сменился альянс.
            return false;
    }

    public int YearSpendings(PlayerScript pl)
    {
        int res = 0;

        if (pl.Authority == Authority.Amer)
        {
            if(curAmerIndustr != null)
                res = curAmerIndustr.Cntr1.Score + curAmerIndustr.Cntr2.Score;
        }
        else
        {
            if (curSovIndustr != null)
                res = curSovIndustr.Cntr1.Score + curSovIndustr.Cntr2.Score;
        }

        return res * 12;
    }

    //////////////
    //AI actions
    //////////////
    public void AIActions(PlayerScript pl)
    {
        IndustryElement curIndustr = GetCurIndustr(pl);
        if (curIndustr != null)
            return;

        List<CountryScript> primaryList = GetAvailableCountriesPrimary(pl.Authority);
        if (primaryList.Count == 0)
            return;

        //сортируем список стран по убыванию Influense
        primaryList.Sort((x1, x2) => x1.GetInfluense(pl.Authority) > x2.GetInfluense(pl.Authority) ? -1 : 1);
        List<CountryScript> secondaryList = GetAvailableCountriesSecondary(pl.Authority, primaryList[0]);
        if (secondaryList.Count == 0)
            return;

        //сортируем список стран по убыванию Influense
        secondaryList.Sort((x1, x2) => x1.GetInfluense(pl.Authority) > x2.GetInfluense(pl.Authority) ? -1 : 1);

        IndustryType industrType = IndustryType.Military;
        switch (pl.PlayerLeader.LeaderID)
        {
            case 1:
                //Труман, Сталин
                industrType = IndustryType.Military;
                break;
            case 2:
                //Кеннеди, Хрущев
                industrType = IndustryType.Military;
                break;
            case 3:
                //Никсон, Брежнев
                industrType = IndustryType.Spy;
                break;
            case 4:
                //Рейган, Горбачев
                industrType = IndustryType.Diplomat;
                break;
        }

        StartIndustrialization(primaryList[0], secondaryList[0], industrType, pl);
    }

    public IndustrializationData GetSavedData()
    {
        IndustrializationData res = new IndustrializationData();
        List<IndustryElement.SerializableInd> serIndustrs = new List<IndustryElement.SerializableInd>();
        foreach (var item in Industrs)
        {
            serIndustrs.Add(IndustryElement.SerSavedData(item));
        }

        res.Industrs = serIndustrs;
        res.curAmerIndustr = IndustryElement.SerSavedData(curAmerIndustr);
        res.curSovIndustr = IndustryElement.SerSavedData(curSovIndustr);

        return res;
    }

    public void SetSavedData(IndustrializationData indData)
    {
        curAmerIndustr = IndustryElement.DeserSavedData(indData.curAmerIndustr);
        curSovIndustr = IndustryElement.DeserSavedData(indData.curSovIndustr);

        foreach (var item in indData.Industrs)
        {
            Industrs.Add(IndustryElement.DeserSavedData(item));

            if (GameManagerScript.GM.Player.Authority == Industrs[Industrs.Count - 1].Player.Authority)
                Industrs[Industrs.Count-1].DoneContractRow = IndustrMenu.AddDoneContract(Industrs[Industrs.Count - 1]);
        }
    }
}

public class IndustryElement
{
    public int CountDown { get; set; }
    public IndustrDoneContractsPrefab DoneContractRow { get; set; }
    CountryScript cntr1;
    CountryScript cntr2;
    IndustryType indType;
    int cost;
    PlayerScript pl;

    public CountryScript Cntr1
    {
        get { return cntr1; }
    }

    public CountryScript Cntr2
    {
        get { return cntr2; }
    }

    public PlayerScript Player
    {
        get { return pl; }
    }

    public IndustryType IndustryType
    {
        get { return indType; }
    }

    public IndustryElement(CountryScript cntr1, CountryScript cntr2, IndustryType indType, PlayerScript pl)
    {
        this.pl = pl;
        this.cntr1 = cntr1;
        this.cntr2 = cntr2;
        this.CountDown = GameManagerScript.GM.DLC_Industrialisation.IndustrCount;
        this.indType = indType;
        this.cost = cntr1.Score + cntr2.Score;
    }

    public void DelDoneContractRow()
    {
        GameObject.Destroy(DoneContractRow.gameObject);
    }

    //Оплата
    //Возвращает true, если строительство закончено
    public bool PayCost()
    {
        if (CountDown > 0)
        {
            CountDown--;
            pl.Budget -= cost;
            if (CountDown == 0)
            {
                cntr1.Score++;
                cntr2.Score++;
                return true;
            }
        }

        return false;
    }

    public void UnitGain()
    {
        if (CountDown > 0)
            return;

        switch (indType)
        {
            case IndustryType.Diplomat:
                pl.DiplomatPool++;
                break;
            case IndustryType.Military:
                pl.MilitaryPool++;
                break;
            case IndustryType.Spy:
                pl.SpyPool++;
                break;
            default:
                break;
        }
    }

    public static SerializableInd SerSavedData(IndustryElement IndustrEl)
    {
        if (IndustrEl == null)
            return null;

        SerializableInd res = new SerializableInd();
        res.CountDown = IndustrEl.CountDown;
        res.cntr1 = IndustrEl.Cntr1.Name;
        res.cntr2 = IndustrEl.Cntr2.Name;
        res.indType = IndustrEl.IndustryType;
        res.plAuth = IndustrEl.pl.Authority;

        return res;
    }

    public static IndustryElement DeserSavedData(SerializableInd serIndustr)
    {
        if (serIndustr == null)
            return null;

        GameManagerScript GM = GameManagerScript.GM;
        return new IndustryElement(GM.FindCountryByName(serIndustr.cntr1), GM.FindCountryByName(serIndustr.cntr2), serIndustr.indType, GM.GetPlayerByAuthority(serIndustr.plAuth));
    }

    [System.Serializable]
    public class SerializableInd
    {
        public int CountDown;
        public string cntr1;
        public string cntr2;
        public IndustryType indType;
        public Authority plAuth;
    }
}

public enum IndustryType
{
    Diplomat,
    Military,
    Spy
}
