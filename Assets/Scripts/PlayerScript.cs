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
    public bool[] MilGroundTechStatus = new bool[11];
    public bool[] MilSeaTechStatus = new bool[11];

    public List<int> History = new List<int>();

    //количества военных, шпионов, дипломатов
    int militaryAmount = 0;
    int spyAmount = 0;
    int diplomatAmount = 0;
    //траты на открытие теххнологий, военныхх, дипломатов и шпионов
    Dictionary<OutlayField, UniOutlay> outlays;

    // Use this for initialization
    void Start ()
    {
        TechStatus[0] = true;   //Для доступности первой технологии

        outlays = new Dictionary<OutlayField, UniOutlay>();
        outlays.Add(OutlayField.air, new UniOutlay(GameManagerScript.GM.OutlayChangesPerYear));
        outlays.Add(OutlayField.ground, new UniOutlay(GameManagerScript.GM.OutlayChangesPerYear));
        outlays.Add(OutlayField.sea, new UniOutlay(GameManagerScript.GM.OutlayChangesPerYear));
        outlays.Add(OutlayField.military, new UniOutlay(GameManagerScript.GM.OutlayChangesPerYear));
        outlays.Add(OutlayField.spy, new UniOutlay(GameManagerScript.GM.OutlayChangesPerYear));
        outlays.Add(OutlayField.diplomat, new UniOutlay(GameManagerScript.GM.OutlayChangesPerYear));
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

    public void SetMilAirTechStatus(int idx)
    {
        MilAirTechStatus[idx] = true;
    }

    public bool GetMilAirTechStatus(int idx)
    {
        return MilAirTechStatus[idx];
    }
    public void SetMilGroundTechStatus(int idx)
    {
        MilGroundTechStatus[idx] = true;
    }

    public bool GetMilGroundTechStatus(int idx)
    {
        return MilGroundTechStatus[idx];
    }
    public void SetMilSeaTechStatus(int idx)
    {
        MilSeaTechStatus[idx] = true;
    }

    public bool GetMilSeaTechStatus(int idx)
    {
        return MilSeaTechStatus[idx];
    }

}

public class UniOutlay
{
    int budget; //накопленные средства
    int outlay; //траты
    int changeDiscounter; //счетчик количества изменений трат в год

    public UniOutlay(int discounter)
    {
        budget = 0;
        outlay = 0;
        changeDiscounter = discounter;
    }

    public int Budget
    {
        get { return budget; }
    }

    public int Outlay
    {
        get { return outlay; }
    }

    public int ChangeDiscounter
    {
        get { return changeDiscounter; }
    }

    public void ChangeOutlet(int amount)
    {
        if (changeDiscounter > 0)
        {
            outlay += amount;
            if (outlay < 0)
                outlay = 0;

            changeDiscounter -= 1;
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
    diplomat
}
