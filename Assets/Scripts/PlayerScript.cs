using UnityEngine;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour {
    public double Budget;
    private int _Score;
    public Authority Authority;
    public CountryScript MyCountry;
    public CountryScript OppCountry;
    public Sprite SprMarker;

    bool[] TechStatus = new bool[41]; //true - технология исследована (технологий 40, в оригинале они нумеровались с единицы, чтобы не путаться и в массиве будем их хранить начиная с первого элемента, поэтому 41 элемент в массиве)

    public bool[] MilAirTechStatus = new bool[11];
    public bool[] MilGndTechStatus = new bool[11];
    public bool[] MilSeaTechStatus = new bool[11];
    public bool[] MilRocketTechStatus = new bool[11];

    public List<int> History = new List<int>();

    //количества военных, шпионов, дипломатов
    int militaryAmount = 0;
    int spyAmount = 0;
    int diplomatAmount = 0;
    //траты на открытие теххнологий, военныхх, дипломатов и шпионов
    Dictionary<OutlayField, UniOutlay> outlays;
    int outlayChangeDiscounter;

    // Use this for initialization
    void Start ()
    {
        GameManagerScript GM = GameManagerScript.GM;

        TechStatus[0] = true;   //Для доступности первой технологии

        outlays = new Dictionary<OutlayField, UniOutlay>();
        outlays.Add(OutlayField.air, new UniOutlay(this, GM.AirMilitaryCost));
        outlays.Add(OutlayField.ground, new UniOutlay(this, GM.GroundMilitaryCost));
        outlays.Add(OutlayField.sea, new UniOutlay(this, GM.SeaMilitaryCost));
        outlays.Add(OutlayField.military, new UniOutlay(this, GM.MILITARY_COST));
        outlays.Add(OutlayField.spy, new UniOutlay(this, GM.SPY_COST));
        outlays.Add(OutlayField.diplomat, new UniOutlay(this, GM.DiplomatCost));

        outlayChangeDiscounter = GameManagerScript.GM.OutlayChangesPerYear;
    }

    public int OutlayChangeDiscounter
    {
        get{ return outlayChangeDiscounter;}
        set { outlayChangeDiscounter = value; }
    }

    public Dictionary<OutlayField, UniOutlay> Outlays
    {
        get { return outlays; }
    }

    public int MilitaryPool
    {
        get { return militaryAmount; }
    }

    public int SpyPool
    {
        get { return spyAmount; }
    }

    public int DiplomatPool
    {
        get { return diplomatAmount; }
    }

    public int Score
    {
        get {
            GameObject Countries = GameObject.Find("Countries");
            CountryScript Country;
            int s = 0;

            for (int idx = 0; idx < Countries.transform.childCount; idx++)
            {
                Country = Countries.transform.GetChild(idx).GetComponent<CountryScript>();
                if(Country.Authority == Authority)
                    s += Country.Score;
            }
            _Score = s;
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
        Budget = ((Budget + _Score) * add);
        Budget = Mathf.RoundToInt((float)Budget);

        SoundManager.SM.PlaySound("sound/moneyin");

        //Сохранение истории показателей роста
        History.Add(AddProcent);
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


}

public class UniOutlay
{
    int cost;   //стоимость объекта трат
    int budget; //накопленные средства
    int outlay; //траты
    PlayerScript player;

    public UniOutlay(PlayerScript _player, int objectCost)
    {
        budget = 0;
        outlay = 0;
        player = _player;
        cost = objectCost;
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
        if (player.OutlayChangeDiscounter > 0)
        {
            outlay += amount;
            if (outlay < 0)
                outlay = 0;
            else
                player.OutlayChangeDiscounter -= 1;
        }
    }
}

public enum OutlayField
{
    air,
    ground,
    sea,
    military,
    spy,
    diplomat,
    rocket
}
