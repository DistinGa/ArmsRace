using UnityEngine;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
    public const int spaceTechCount = 41, milTechCount = 11;
    public double Budget;
    public Authority Authority;
    public CountryScript MyCountry;
    public CountryScript OppCountry;
    public Sprite SprMarker;

    bool[] TechStatus = new bool[spaceTechCount]; //true - технология исследована (технологий 40, в оригинале они нумеровались с единицы, чтобы не путаться и в массиве будем их хранить начиная с первого элемента, поэтому 41 элемент в массиве)

    public bool[] MilAirTechStatus = new bool[milTechCount];
    public bool[] MilGndTechStatus = new bool[milTechCount];
    public bool[] MilSeaTechStatus = new bool[milTechCount];
    public bool[] MilRocketTechStatus = new bool[milTechCount];

    public List<int> History = new List<int>(); //история процентов прироста бюджета
    public List<int> History2 = new List<int>();//история прироста бюджета

    //количества военных, шпионов, дипломатов
    [SerializeField]
    int militaryAmount = 0;
    [SerializeField]
    int spyAmount = 0;
    [SerializeField]
    int diplomatAmount = 0;
    //траты на открытие теххнологий, военныхх, дипломатов и шпионов
    Dictionary<OutlayField, UniOutlay> outlays;
    int outlayChangeDiscounter;

    // Use this for initialization
    void Start()
    {
        GameManagerScript GM = GameManagerScript.GM;

        TechStatus[0] = true;   //Для доступности первой технологии

        outlays = new Dictionary<OutlayField, UniOutlay>();
        outlays.Add(OutlayField.air, new UniOutlay(this, OutlayField.air, GM.MDInstance.GetTechCost(OutlayField.air, 1)));
        outlays.Add(OutlayField.ground, new UniOutlay(this, OutlayField.ground, GM.MDInstance.GetTechCost(OutlayField.ground, 1)));
        outlays.Add(OutlayField.sea, new UniOutlay(this, OutlayField.sea, GM.MDInstance.GetTechCost(OutlayField.sea, 1)));
        outlays.Add(OutlayField.rocket, new UniOutlay(this, OutlayField.rocket, GM.MDInstance.GetTechCost(OutlayField.rocket, 1)));
        outlays.Add(OutlayField.military, new UniOutlay(this, OutlayField.military, GM.MILITARY_COST));
        outlays.Add(OutlayField.spy, new UniOutlay(this, OutlayField.spy, GM.SPY_COST));
        outlays.Add(OutlayField.diplomat, new UniOutlay(this, OutlayField.diplomat, GM.DiplomatCost));
        outlays.Add(OutlayField.spaceLaunches, new UniOutlay(this, OutlayField.spaceLaunches, GM.SRInstance.GetTechCost(OutlayField.spaceLaunches, 1)));
        outlays.Add(OutlayField.spaceGround, new UniOutlay(this, OutlayField.spaceGround, GM.SRInstance.GetTechCost(OutlayField.spaceGround, 1)));

        outlayChangeDiscounter = GameManagerScript.GM.OutlayChangesPerYear;
    }

    public int OutlayChangeDiscounter
    {
        get { return outlayChangeDiscounter; }
        set { outlayChangeDiscounter = value; }
    }

    public Dictionary<OutlayField, UniOutlay> Outlays
    {
        get { return outlays; }
    }

    public int GetPool(OutlayField field)
    {
        int amount = 0;
        switch (field)
        {
            case OutlayField.military:
                amount = militaryAmount;
                break;
            case OutlayField.spy:
                amount = spyAmount;
                break;
            case OutlayField.diplomat:
                amount = diplomatAmount;
                break;
            default:
                break;
        }
        return amount;
    }

    public void SetPool(OutlayField field, int amount)
    {
        switch (field)
        {
            case OutlayField.military:
                militaryAmount = amount;
                break;
            case OutlayField.spy:
                spyAmount = amount;
                break;
            case OutlayField.diplomat:
                diplomatAmount = amount;
                break;
            default:
                break;
        }
    }

    public int MilitaryPool
    {
        get { return militaryAmount; }
        set { militaryAmount = value; }
    }

    public int SpyPool
    {
        get { return spyAmount; }
        set { spyAmount = value; }
    }

    public int DiplomatPool
    {
        get { return diplomatAmount; }
        set { diplomatAmount = value; }
    }

    public int Score
    {
        get
        {
            GameObject Countries = GameObject.Find("Countries");
            CountryScript Country;
            int s = 0;

            for (int idx = 0; idx < Countries.transform.childCount; idx++)
            {
                Country = Countries.transform.GetChild(idx).GetComponent<CountryScript>();
                if (Country.Authority == Authority)
                    s += Country.Score;
            }
            return s;
        }
    }

    public void AnnualGrowthBudget()
    {
        int AddProcent = Random.Range(5, 10 + 1); // с 5% до 10%

        // если у игрока больше 700 ( бюджет ) то 
        // ежегодной прирост для этого игрока не от 5 до 10% а от 2% до 5%
        if (Budget > 700) AddProcent = Random.Range(2, 5 + 1);

        double add = 1 + AddProcent / 100.0;
        double newB = ((Budget + Score) * add);
        //Сохранение истории показателей роста
        History.Add(AddProcent);
        History2.Add(Mathf.RoundToInt((float)(newB - Budget)));

        Budget = Mathf.RoundToInt((float)newB);
        SoundManager.SM.PlaySound("sound/moneyin");
    }

    //
    public void SetTechStatus(int idx)
    {
        TechStatus[idx] = true;
    }

    public bool GetTechStatus(int idx)
    {
        return TechStatus[idx];
    }

    public void SetMilTechStatus(OutlayField field, int idx)
    {
        switch (field)
        {
            case OutlayField.air:
                MilAirTechStatus[idx] = true;
                break;
            case OutlayField.ground:
                MilGndTechStatus[idx] = true;
                break;
            case OutlayField.sea:
                MilSeaTechStatus[idx] = true;
                break;
            case OutlayField.rocket:
                MilRocketTechStatus[idx] = true;
                break;
            default:
                break;
        }
    }

    public bool GetMilTechStatus(OutlayField field, int idx)
    {
        bool status = false;

        switch (field)
        {
            case OutlayField.air:
                status = MilAirTechStatus[idx];
                break;
            case OutlayField.ground:
                status = MilGndTechStatus[idx];
                break;
            case OutlayField.sea:
                status = MilSeaTechStatus[idx];
                break;
            case OutlayField.rocket:
                status = MilRocketTechStatus[idx];
                break;
            default:
                break;
        }

        return status;
    }

    //Получить изучаемую в данный момент технологию
    public int GetCurMilTech(OutlayField field)
    {
        int res = 0;

        int i = 0;
        while (i < milTechCount)
        {
            if (!GetMilTechStatus(field, i))
            {
                res = i;
                break;
            }
            i++;
        }

        return res;
    }

    public int FirePower(OutlayField field)
    {
        if (field != OutlayField.air && field != OutlayField.ground && field != OutlayField.sea)
            return 0;

        int fp = 0;
        bool[] TechStatus = null;

        switch (field)
        {
            case OutlayField.air:
                TechStatus = MilAirTechStatus;
                break;
            case OutlayField.ground:
                TechStatus = MilGndTechStatus;
                break;
            case OutlayField.sea:
                TechStatus = MilSeaTechStatus;
                break;
        }

        foreach (var item in TechStatus)
        {
            if (item)
                fp += GameManagerScript.GM.FirePowerPerTech;
        }

        return fp;
    }

    public void NewMonth()
    {
        foreach (var item in outlays)
        {
            item.Value.MakeOutlet();
        }
    }

    public void NewYear()
    {
        outlayChangeDiscounter = GameManagerScript.GM.OutlayChangesPerYear;
        AnnualGrowthBudget();
    }

    public int TotalYearSpendings()
    {
        return Outlays[OutlayField.air].YearSpendings() + Outlays[OutlayField.diplomat].YearSpendings() + Outlays[OutlayField.ground].YearSpendings() + Outlays[OutlayField.military].YearSpendings() + Outlays[OutlayField.rocket].YearSpendings() + Outlays[OutlayField.sea].YearSpendings() + Outlays[OutlayField.spy].YearSpendings() + Outlays[OutlayField.spaceLaunches].YearSpendings() + Outlays[OutlayField.spaceGround].YearSpendings();
    }

    //Сохранение/загрузка
    public SavedPlayerData GetSavedData()
    {
        SavedPlayerData res = new SavedPlayerData();

        res.aut = Authority;
        res.Budget = Budget;
        res.TechStatus = TechStatus;
        res.MilAirTechStatus = MilAirTechStatus;
        res.MilGndTechStatus = MilGndTechStatus;
        res.MilSeaTechStatus = MilSeaTechStatus;
        res.MilRocketTechStatus = MilRocketTechStatus;

        res.History = History;
        res.History2 = History2;
        res.militaryAmount = militaryAmount;
        res.spyAmount = spyAmount;
        res.diplomatAmount = diplomatAmount;
        res.outlayChangeDiscounter = outlayChangeDiscounter;

        res.outlays = outlays;
        //res.outlays = new Dictionary<OutlayField, UniOutlay>();
        //foreach (var item in outlays)
        //{
        //    res.outlays.Add(item.Key, new UniOutlay(this, item.Key, item.Value.Cost));
        //}

        return res;
    }

    public void SetSavedData(SavedPlayerData sd)
    {
        Budget = sd.Budget;
        TechStatus = sd.TechStatus;
        MilAirTechStatus = sd.MilAirTechStatus;
        MilGndTechStatus = sd.MilGndTechStatus;
        MilSeaTechStatus = sd.MilSeaTechStatus;
        MilRocketTechStatus = sd.MilRocketTechStatus;

        History = sd.History;
        History2 = sd.History2;
        militaryAmount = sd.militaryAmount;
        spyAmount = sd.spyAmount;
        diplomatAmount = sd.diplomatAmount;

        outlays = sd.outlays;
        outlayChangeDiscounter = sd.outlayChangeDiscounter;
    }
}

//класс для работы с расходами
[System.Serializable]
public class UniOutlay
{
    int cost;   //стоимость объекта трат
    int budget; //накопленные средства
    int outlay; //траты
    Authority authority;
    //PlayerScript player;
    OutlayField field;  //вид трат

    int[] outlayHistory = new int[GameManagerScript.GM.MAX_MONTHS_NUM]; //история трат (1 based)

    public UniOutlay(PlayerScript _player, OutlayField fld, int objectCost)
    {
        budget = 0;
        outlay = 0;
        //player = _player;
        authority = _player.Authority;
        cost = objectCost;
        field = fld;
    }

    public int Budget
    {
        get { return budget; }
    }

    public int Outlay
    {
        get { return outlay; }
    }

    public int Cost
    {
        get { return cost; }
    }

    public void ChangeOutlet(int amount)
    {
        PlayerScript player = GameManagerScript.GM.GetPlayerByAuthority(authority);

        if (player.OutlayChangeDiscounter > 0)
        {
            outlay += amount;
            if (outlay < 0)
                outlay = 0;
            else
                player.OutlayChangeDiscounter -= 1;
        }
    }

    //ежемесячные отчисления
    public void MakeOutlet()
    {
        PlayerScript player = GameManagerScript.GM.GetPlayerByAuthority(authority);

        budget += outlay;
        player.Budget -= outlay;

        //запоминаем историю расходов
        outlayHistory[GameManagerScript.GM.CurrentMonth] = outlay;

        while (budget >= cost) //накопили нужное количество денег, добавляем юнит/технологию
        {
            switch (field)
            {
                case OutlayField.air:
                    TakeNextTech(field);
                    break;
                case OutlayField.ground:
                    TakeNextTech(field);
                    break;
                case OutlayField.sea:
                    TakeNextTech(field);
                    break;
                case OutlayField.military:
                    budget -= cost;
                    player.MilitaryPool++;
                    break;
                case OutlayField.spy:
                    budget -= cost;
                    player.SpyPool++;
                    break;
                case OutlayField.diplomat:
                    budget -= cost;
                    player.DiplomatPool++;
                    break;
                case OutlayField.rocket:
                    TakeNextTech(field);
                    break;
                default:    //остались космические технологии
                    TakeNextSpaceTech(field);
                    break;
            }
        }
    }

    //переход к изучению следующей технологии
    void TakeNextTech(OutlayField field)
    {
        PlayerScript player = GameManagerScript.GM.GetPlayerByAuthority(authority);

        int curTech = player.GetCurMilTech(field);      //изучаемая в данный момент технология
        int tCount = player.MilAirTechStatus.Length;    //костыль, чтобы не плодить константу

        if (curTech > 0)
        {
            budget -= cost;
            player.SetMilTechStatus(field, curTech);
            //если не последняя технология, берём стоимость следующей
            if (curTech < tCount - 1)
                cost = GameManagerScript.GM.MDInstance.GetTechCost(field, curTech + 1);
            else//если последняя, прекращаем инвестиции в технологию
                outlay = 0;
        }
    }

    //переход к изучению следующей космической технологии
    void TakeNextSpaceTech(OutlayField field)
    {
    }

    public int YearSpendings()
    {
        int res = 0;
        int curMonth = GameManagerScript.GM.CurrentMonth; //(0 based)
        int yearMonth = curMonth % 12;    //(0 based)

        for (int i = curMonth; i > curMonth - yearMonth; i--)   //цикл по месяцам этого года
        {
            res += outlayHistory[i];
        }

        res += outlay * (12 - yearMonth);   //оставшиеся месяца года дополняем текущиим значением трат

        return res;
    }
}

[System.Serializable]
public enum OutlayField
{
    air,
    ground,
    sea,
    military,
    spy,
    diplomat,
    rocket,
    spaceLaunches,
    spaceGround
}
