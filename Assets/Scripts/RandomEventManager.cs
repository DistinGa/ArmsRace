using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public delegate void dlgtEventAction();

public class RandomEventManager : MonoBehaviour
{
    public static RandomEventManager REMInstance;

    [Tooltip("сколько месяцев будет висеть оповещение (флаг слева)")]
    public int EventDuration = 12;  //сколько месяцев будет висеть оповещение (флаг слева)
    [Tooltip("периодичность событий (раз во сколько месяцев)")]
    public int EventPeriod = 24;    //периодичность событий (раз во сколько месяцев)
    public RectTransform EventFlag;
    public GameObject EventMenu;

    RandomEvent rEvent;

    public void Awake()
    {
        REMInstance = this;
    }

    void Start()
    {
        GameManagerScript.GM.SubscribeMonth(MonthTick);
    }

    public void MonthTick()
    {
        if (rEvent != null && rEvent.eventDurationDiscounter > 0)
        {
            if (--rEvent.eventDurationDiscounter == 0)
            {
                //время события закончилось, убираем флаг
                EventFlag.gameObject.SetActive(false);
                rEvent.eventDurationDiscounter = 0;
            }
        }
        else
        {
            int r = Random.Range(0, EventPeriod);
            if (r == 0)
                InitEvent();
        }

    }

    public void InitEvent(RandomEvent re = null)
    {
        GameManagerScript GM = GameManagerScript.GM;
        if (re != null)
            rEvent = re;
        else
        {
            //Выбираем случайную страну, где есть наши шпионы.
            CountryScript selCountry = null;

            List<CountryScript> countries = new List<CountryScript>();
            foreach (var c in CountryScript.Countries())
            {
                if (c.HaveSpy(GM.Player.Authority) && GM.Player.MyCountry != c)
                    countries.Add(c);
            }

            if (countries.Count > 0)
                selCountry = countries[Random.Range(0, countries.Count)];

            if (selCountry == null)
                return;

            //Выбираем случайное событие.
            int evendID = Random.Range(0, 5);
            switch (evendID)
            {
                case 0:
                    rEvent = new RandomEvent();
                    rEvent.Country = selCountry;
                    rEvent.Description = "Would you like to support local government";
                    rEvent.Description2 = "+20 support";
                    rEvent.Amount = 20;
                    rEvent.Action = new dlgtEventAction(rEvent.AddSupport);
                    break;
                case 1:
                    rEvent = new RandomEvent();
                    rEvent.Country = selCountry;
                    rEvent.Description = "Would you like to support local terrorists";
                    rEvent.Description2 = "+20 opposition";
                    rEvent.Amount = -20;
                    rEvent.Action = new dlgtEventAction(rEvent.AddSupport);
                    break;
                case 2:
                    rEvent = new RandomEvent();
                    rEvent.Country = selCountry;
                    rEvent.Description = "Would you like to support local nationalists";
                    rEvent.Description2 = "+20 neutral influence";
                    rEvent.Authority = Authority.Neutral;
                    rEvent.Amount = -20;
                    rEvent.Action = new dlgtEventAction(rEvent.AddInfluence);
                    break;
                case 3:
                    rEvent = new RandomEvent();
                    rEvent.Country = selCountry;
                    if (GM.Player.Authority == Authority.Amer)
                    {
                        rEvent.Description = "Would you like to support local democratic party";
                        rEvent.Description2 = "+20 american influence";
                        rEvent.Authority = Authority.Amer;
                    }
                    else
                    {
                        rEvent.Description = "Would you like to support local communist party";
                        rEvent.Description2 = "+20 soviet influence";
                        rEvent.Authority = Authority.Soviet;
                    }
                    rEvent.Amount = 20;
                    rEvent.Action = new dlgtEventAction(rEvent.AddInfluence);
                    break;
                case 4:
                    rEvent = new RandomEvent();
                    rEvent.Country = selCountry;
                    if (GM.Player.Authority == Authority.Amer)
                    {
                        rEvent.Description = "Would you like to discredit Soviet ambassador";
                        rEvent.Description2 = "-10 soviet influence";
                        rEvent.Authority = Authority.Soviet;
                    }
                    else
                    {
                        rEvent.Description = "Would you like to discredit American ambassador";
                        rEvent.Description2 = "-10 american influence";
                        rEvent.Authority = Authority.Amer;
                    }
                    rEvent.Amount = -10;
                    rEvent.Action = new dlgtEventAction(rEvent.AddInfluence);
                    break;
            }

            //Выбираем случайную стоимость.
            rEvent.Cost = Random.Range(4, 11) * 5;
            rEvent.eventDurationDiscounter = EventDuration;
        }

        //Установка флага
        EventFlag.GetComponent<Image>().sprite = (rEvent.Country.Authority == Authority.Soviet ? rEvent.Country.FlagS : rEvent.Country.FlagNs);
        EventFlag.gameObject.SetActive(true);
    }

    public void OpenEvent()
    {
        GameManagerScript.GM.SnapToCountry(rEvent.Country.Capital.position);
        if (!EventMenu.activeSelf)
            GameManagerScript.GM.ToggleTechMenu(EventMenu);
        EventMenu.GetComponent<RandomEventQuestion>().Init(rEvent);

        string _color = "";
        switch (rEvent.Country.Authority)
        {
            case Authority.Neutral:
                _color = "yellow";
                break;
            case Authority.Amer:
                _color = "blue";
                break;
            case Authority.Soviet:
                _color = "red";
                break;
        }
        EventMenu.transform.Find("CountryName").GetComponent<Text>().text = "<color=" + _color + ">" + rEvent.Country.Name + "</color>";
        EventMenu.transform.Find("DescriptionPanel/Description").GetComponent<Text>().text = rEvent.Description;
        EventMenu.transform.Find("Description2").GetComponent<Text>().text = rEvent.Description2 + ":";
        EventMenu.transform.Find("Cost").GetComponent<Text>().text = "$" + rEvent.Cost.ToString();
    }

    public void CloseEvent(RandomEvent re)
    {
        //Если закрывается то, событие, которое активно в данный момент, скрываем флаг.
        //Закрываться может неактивное событие, если со времени открытия окна с вопросом, возникло новое событие.
        if (ReferenceEquals(rEvent, re))
        {
            EventFlag.gameObject.SetActive(false);
        }
    }

    public RandomEvent GetSavedData()
    {
        return rEvent;
    }

    public void SetSavedData(RandomEvent re)
    {
        InitEvent(re);
    }
}

[System.Serializable]
public class RandomEvent
{
    public string CountryName;
    public string Description;
    public string Description2;
    public int Cost;
    public dlgtEventAction Action;
    public Authority Authority;
    public int Amount;
    public int eventDurationDiscounter;

    public void AddSupport()
    {
        Country.Support += Amount;
    }

    public void AddInfluence()
    {
        Country.AddInfluence(Authority, Amount);
    }

    public CountryScript Country
    {
        get { return GameObject.Find(CountryName).GetComponent<CountryScript>(); }
        set { CountryName = value.gameObject.name; }
    }
}


