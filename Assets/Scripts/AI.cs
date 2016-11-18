using UnityEngine;
using System.Collections.Generic;

public class AI : MonoBehaviour {
    public PlayerScript AIPlayer;
    public LeaderScript AILeader;
    public int WinPercent;  //процент победы в войне, учитывающийся при присвоении режима war
    [Space(10)]
    public InvCommon[] AIInvestSettings = new InvCommon[8];
    public ActCommon[] AIActSettings = new ActCommon[8];
    [Space(10)]
    //Настройки читтерсва AI
    [Tooltip("Ежегодная прибавка к бюджету")]
    public int[] YearCheat;
    [Tooltip("Прибавка к бюджету в начале месяца, если AI проигрывает по очкам")]
    public int[] MonthCheat;


    bool gndSpaceTech = true;   //В космические технологии АИ вкладывается поочередно (ground/launches). Флаг хранит направление вложения.

    void Start ()
    {
        if (GameManagerScript.GM.AI == null)
            Destroy(gameObject);    //Играем против другого игрока, АИ не нужен

        //Назначаем сторону AI в зависимости от выбранной стороны игрока
        if (SettingsScript.Settings.playerSelected == Authority.Amer)
            AIPlayer = GameObject.Find("SovPlayer").GetComponent<PlayerScript>();
        else
            AIPlayer = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>();

        AIPlayer.MoonSwitchState = false;
    }

    // ход AI
    public void AIturn()
    {
        GameManagerScript GM = GameManagerScript.GM;
        //Если AI проигрывает, добавляемм ему бюджет в соответствие с настройками.
        if (AIPlayer.Score < GM.GetOpponentTo(AIPlayer).Score)
            AIPlayer.Budget += MonthCheat[SettingsScript.Settings.AIPower];

        AIPlayer.NewMonth();

        //каждые 10 лет случайным образом мменяем лидера
        //Ищем страны, где можно установить своё правительство.


        bool BudgetPlus;
        bool war1 = false;  //для инвестиций
        bool war2 = false;  //для действий
        int eventProc;      //для определения случайного действия

        InvCommon investing = null;
        ActCommon acting = null;

        investing = AIInvestSettings[(AIPlayer.Authority == Authority.Amer ? -1 : 3) + AILeader.LeaderID];
        acting = AIActSettings[(AIPlayer.Authority == Authority.Amer ? -1 : 3) + AILeader.LeaderID];

        List<CountryScript> countries = CountryScript.Countries();

        foreach (CountryScript country in countries)
        {
            if (country.CanAddMil(AIPlayer.Authority) && country.OppForce > 0)
            {
                war1 = true;
                break;
            }
        }

        if (war1)
            war2 = true;
        else
        {
            //Если есть страна, в которую можно ввести войска и процент победы в войне >= WinPercent, считаем, AI находящимся в состоянии войны (для выбора действий)
            foreach (CountryScript country in countries)
            {
                if (country.CanAddMil(AIPlayer.Authority) && AIPlayer.WinPercentForCountry(country) >= WinPercent)
                {
                    war2 = true;
                    break;
                }
            }
        }

        //Если последний прирост бюджета минус предполагаемые траты за этот год > 1, считаем бюджет положительным.
        if (AIPlayer.History2.Count > 0)
            BudgetPlus = (AIPlayer.History2[AIPlayer.History2.Count - 1] - AIPlayer.TotalYearSpendings()) > 1;
        else
            BudgetPlus = (0 - AIPlayer.TotalYearSpendings()) > 1;

        //Определение колонки таблицы
        int column;
        if (BudgetPlus)
            column = 0;
        else
            column = 2;

        if (!war1)
            column += 1;

        //Выбор инвестиций
        if (AIPlayer.OutlayChangeDiscounter > 0)
        {
            eventProc = Random.Range(0, 100);

            for (int i = 0; i < investing.dataArray.Length; i++)
            {
                eventProc -= investing.GetValue(i, column);
                if (eventProc < 0)
                {
                    DoInvest(i);
                    break;
                }
            }
        }

        //Повторный пересчёт бюджета (он мог измениться в результате предыдущего шага).
        //Если последний прирост бюджета минус предполагаемые траты за этот год > 1, считаем бюджет положительным.
        if (AIPlayer.History2.Count > 0)
            BudgetPlus = (AIPlayer.History2[AIPlayer.History2.Count - 1] - AIPlayer.TotalYearSpendings()) > 1;
        else
            BudgetPlus = (0 - AIPlayer.TotalYearSpendings()) > 1;

        //Определение колонки таблицы
        if (BudgetPlus)
            column = 0;
        else
            column = 2;

        if (!war2)
            column += 1;

        //Выбор действий
        eventProc = Random.Range(0, 100);
        for (int i = 0; i < acting.dataArray.Length; i++)
        {
            eventProc -= acting.GetValue(i, column);
            if (eventProc < 0)
            {
                DoAction(i, countries);
                break;
            }
        }

    }

    //Выполнение выбранной инвестиции
    private void DoInvest(int i)
    {
        switch (i)
        {
            case 0:
                //diplomat
                AIPlayer.Outlays[OutlayField.diplomat].ChangeOutlet(1);
                break;
            case 1:
                //military
                //выбираем, куда вкладывать
                int way = Random.Range(0, 4);

                switch (way)
                {
                    case 0:
                        AIPlayer.Outlays[OutlayField.military].ChangeOutlet(1);
                        break;
                    case 1:
                        if(AIPlayer.GetCurMilTech(OutlayField.air) > 0) //если изучены все технологии в линии, не добавляем инвестиций
                            AIPlayer.Outlays[OutlayField.air].ChangeOutlet(1);
                        else if (AIPlayer.GetCurMilTech(OutlayField.ground) > 0)    //если все технологии в линии изучены, пробуем изучить другую технологию
                            AIPlayer.Outlays[OutlayField.ground].ChangeOutlet(1);
                        else if (AIPlayer.GetCurMilTech(OutlayField.sea) > 0)
                            AIPlayer.Outlays[OutlayField.sea].ChangeOutlet(1);
                        else if (AIPlayer.GetCurMilTech(OutlayField.military) > 0)  //если все линии изучены полностью, вкладываем в войска
                            AIPlayer.Outlays[OutlayField.military].ChangeOutlet(1);

                        break;
                    case 2:
                        if (AIPlayer.GetCurMilTech(OutlayField.ground) > 0)
                            AIPlayer.Outlays[OutlayField.ground].ChangeOutlet(1);
                        else if (AIPlayer.GetCurMilTech(OutlayField.sea) > 0)
                            AIPlayer.Outlays[OutlayField.sea].ChangeOutlet(1);
                        else if (AIPlayer.GetCurMilTech(OutlayField.air) > 0)
                            AIPlayer.Outlays[OutlayField.air].ChangeOutlet(1);
                        else if (AIPlayer.GetCurMilTech(OutlayField.military) > 0)
                            AIPlayer.Outlays[OutlayField.military].ChangeOutlet(1);

                        break;
                    case 3:
                        if (AIPlayer.GetCurMilTech(OutlayField.sea) > 0)
                            AIPlayer.Outlays[OutlayField.sea].ChangeOutlet(1);
                        else if (AIPlayer.GetCurMilTech(OutlayField.air) > 0)
                            AIPlayer.Outlays[OutlayField.air].ChangeOutlet(1);
                        else if (AIPlayer.GetCurMilTech(OutlayField.ground) > 0)    //если все технологии в линии изучены, пробуем изучить другую технологию
                            AIPlayer.Outlays[OutlayField.ground].ChangeOutlet(1);
                        else if (AIPlayer.GetCurMilTech(OutlayField.military) > 0)
                            AIPlayer.Outlays[OutlayField.military].ChangeOutlet(1);

                        break;
                }
                break;
            case 2:
                AIPlayer.Outlays[OutlayField.spy].ChangeOutlet(1);
                //spy
                break;
            case 3:
                //space
                if (gndSpaceTech)
                {
                    if(AIPlayer.CurGndTechIndex != -1)  //добавляем инвестиции, если изучены не все технологии в линии
                        AIPlayer.Outlays[OutlayField.spaceGround].ChangeOutlet(1);
                    else if (AIPlayer.CurLnchTechIndex != -1)   //иначе изучаем технологию другой линии
                        AIPlayer.Outlays[OutlayField.spaceLaunches].ChangeOutlet(1);
                }
                else
                {
                    if(AIPlayer.CurLnchTechIndex != -1)
                        AIPlayer.Outlays[OutlayField.spaceLaunches].ChangeOutlet(1);
                    else if (AIPlayer.CurGndTechIndex != -1)
                        AIPlayer.Outlays[OutlayField.spaceGround].ChangeOutlet(1);
                }

                gndSpaceTech = !gndSpaceTech;   //в следующий раз будем инвестировать в другую линию
                break;
            default:
                //ничего не делаем
                break;
        }
    }

    //Выполнение выбранного действия
    private void DoAction(int i, List<CountryScript> countries)
    {
        List<CountryScript> selectedCountries = new List<CountryScript>();
        CountryScript actCountry;
        GameManagerScript GM = GameManagerScript.GM;
        float borderInf = GM.INSTALL_PUPPER_INFLU;  //необходимый % pro-влияния для смены правительства 
        float borderSupport = 100 - GM.INSTALL_PUPPER_REVOL;    //% поддержки для ввода революционеров

        switch (i)
        {
            case 0:
            //увеличиваем влияние
                if (AIPlayer.DiplomatPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.CanAddInf(AIPlayer.Authority) && c.Authority != AIPlayer.Authority && c.OppForce == 0 && c.GetInfluense(AIPlayer.Authority) <= borderInf && c.GetInfluense(AIPlayer.Authority) > c.GetInfluense(GM.GetOpponentTo(AIPlayer).Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0) {
                    //сортируем список по возрастанию Support и берём первую страну
                    selectedCountries.Sort((x1,x2) => x1.Support < x2.Support?-1:1);
                    actCountry = selectedCountries[0];

                    actCountry.AddInfluence(AIPlayer.Authority, 1, false);
                }
                break;
            case 1:
            //Ввод войск в мирное время
                //В нейтральную страну
                if (AIPlayer.MilitaryPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.Authority == Authority.Neutral && c.GetInfluense(AIPlayer.Authority) > c.GetInfluense(GM.GetOpponentTo(AIPlayer).Authority) && c.CanAddMil(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0)
                {
                    //сортируем список по возрастанию Support и берём первую страну
                    selectedCountries.Sort((x1, x2) => x1.Support < x2.Support ? -1 : 1);
                    actCountry = selectedCountries[0];

                    actCountry.AddMilitary(AIPlayer.Authority, 1);
                }

                //В страну своего альянса
                if (AIPlayer.MilitaryPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.Authority == AIPlayer.Authority && c.CanAddMil(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0)
                {
                    //сортируем список по возрастанию Support и берём первую страну (первая - с максимальной оппозицией)
                    selectedCountries.Sort((x1, x2) => x1.Support < x2.Support ? -1 : 1);
                    actCountry = selectedCountries[0];

                    actCountry.AddMilitary(AIPlayer.Authority, 1);
                }
                break;
            case 2:
            //Ввод войск в военное время
                //Защита своих
                foreach (CountryScript c in countries)
                {
                    if (c.Authority == AIPlayer.Authority && c.OppForce > 0 && AIPlayer.WinPercentForCountry(c) >= 30 && c.CanAddMil(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                //Защита нейтральных
                foreach (CountryScript c in countries)
                {
                    if (c.Authority == Authority.Neutral && c.OppForce > 0 && AIPlayer.WinPercentForCountry(c) >= 40 && c.CanAddMil(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                //Нападение
                if (AIPlayer.MilitaryPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.Authority != AIPlayer.Authority && AIPlayer.WinPercentForCountry(c) >= 50 && c.Support < borderSupport && c.CanAddMil(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0)
                {
                    //Добавляем военных.
                    foreach (CountryScript c in selectedCountries)
                    {
                        if (AIPlayer.MilitaryPool == 0)
                            break;
                            
                        if (c.Authority == AIPlayer.Authority)
                            c.AddMilitary(AIPlayer.Authority, 3);
                        else
                            c.AddMilitary(AIPlayer.Authority, 2);
                    }
                }
                break;
            case 3:
            //Засылка шпиона (мирное время)
                //своё влияние больше
                if (AIPlayer.SpyPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.Authority == Authority.Neutral && c.OppForce == 0 && c.GetInfluense(AIPlayer.Authority) > c.GetInfluense(GM.GetOpponentTo(AIPlayer).Authority) && c.GetInfluense(AIPlayer.Authority) <= borderInf && c.CanAddSpy(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0)
                {
                    //сортируем список по возрастанию Support и берём первую страну (первая - с максимальной оппозицией)
                    selectedCountries.Sort((x1, x2) => x1.Support < x2.Support ? -1 : 1);
                    actCountry = selectedCountries[0];

                    actCountry.AddSpy(AIPlayer.Authority, 1);
                }

                //своё влияние меньше
                if (AIPlayer.SpyPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.Authority == Authority.Neutral && c.OppForce == 0 && c.GetInfluense(GM.GetOpponentTo(AIPlayer).Authority) > 60 && c.CanAddSpy(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0)
                {
                    //сортируем список по возрастанию Support и берём первую страну (первая - с максимальной оппозицией)
                    selectedCountries.Sort((x1, x2) => x1.Support < x2.Support ? -1 : 1);
                    actCountry = selectedCountries[0];

                    actCountry.AddSpy(AIPlayer.Authority, 1);
                }

                //засылка в свои страны
                if (AIPlayer.SpyPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.Authority == AIPlayer.Authority && c.GetInfluense(AIPlayer.Authority) < c.GetInfluense(GM.GetOpponentTo(AIPlayer).Authority) && c.CanAddSpy(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0)
                {
                    //выбираем любую
                    actCountry = selectedCountries[Random.Range(0, selectedCountries.Count)];
                    actCountry.AddSpy(AIPlayer.Authority, 1);
                }

                //засылка в страны противника
                if (AIPlayer.SpyPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.Authority == GM.GetOpponentTo(AIPlayer).Authority && c.CanAddSpy(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0)
                {
                    //сортируем список по возрастанию Support и берём первую страну (первая - с максимальной оппозицией)
                    selectedCountries.Sort((x1, x2) => x1.Support < x2.Support ? -1 : 1);
                    actCountry = selectedCountries[0];

                    actCountry.AddSpy(AIPlayer.Authority, 1);
                }

                break;
            case 4:
            //забастовка
                if (AIPlayer.SpyPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.Authority != AIPlayer.Authority && c.GetInfluense(AIPlayer.Authority) > c.GetInfluense(GM.GetOpponentTo(AIPlayer).Authority) && c.GetInfluense(AIPlayer.Authority) <= 90 && c.SpyCount(AIPlayer.Authority) > 3 && c.CanOrgMeeting(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0)
                {
                    //выбираем любую
                    actCountry = selectedCountries[Random.Range(0, selectedCountries.Count)];
                    GM.CallMeeting(actCountry, AIPlayer, false);
                }

            //парад
                if (AIPlayer.SpyPool == 0)
                    return;

                selectedCountries.Clear();
                foreach (CountryScript c in countries)
                {
                    if (c.Authority != GM.GetOpponentTo(AIPlayer).Authority && c.GetInfluense(AIPlayer.Authority) < c.GetInfluense(GM.GetOpponentTo(AIPlayer).Authority) && c.SpyCount(AIPlayer.Authority) > 3 && c.CanOrgParade(AIPlayer.Authority))
                        selectedCountries.Add(c);
                }

                if (selectedCountries.Count > 0)
                {
                    //сортируем список по возрастанию Support и берём первую страну (первая - с максимальной оппозицией)
                    selectedCountries.Sort((x1, x2) => x1.Support < x2.Support ? -1 : 1);
                    actCountry = selectedCountries[0];

                    actCountry.AddSpy(AIPlayer.Authority, 1);
                }
                break;
            default:
                break;
        }
    }

    public double NewYearBonus()
    {
        return YearCheat[SettingsScript.Settings.AIPower];
    }
}

