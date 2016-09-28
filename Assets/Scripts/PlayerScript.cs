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
    
    public List<int> History = new List<int>();

	// Use this for initialization
	void Start () {
        TechStatus[0] = true;   //Для доступности первой технологии
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
}
