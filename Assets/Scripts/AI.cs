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
    [Tooltip("Стартовый бюджет")]
    public int[] StartBudget;
    //Настройки читтерсва AI
    [Tooltip("Ежегодная прибавка к бюджету")]
    public int[] YearCheat;
    [Tooltip("Прибавка к бюджету в начале месяца, если AI проигрывает по очкам")]
    public int[] MonthCheat;


    bool gndSpaceTech = true;   //В космические технологии АИ вкладывается поочередно (ground/launches). Флаг хранит направление вложения.

    void Start ()
    {
        if (GameManagerScript.GM.AI == null)
        {
            Destroy(gameObject);    //Играем против другого игрока, АИ не нужен
            return;
        }

        //Назначаем сторону AI в зависимости от выбранной стороны игрока
        if (SettingsScript.Settings.playerSelected == Authority.Amer)
            AIPlayer = GameObject.Find("SovPlayer").GetComponent<PlayerScript>();
        else
            AIPlayer = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>();

        AIPlayer.MoonSwitchState = false;
        AIPlayer.Budget = StartBudget[SettingsScript.Settings.AIPower];
        AIPlayer.History2.Add(StartBudget[SettingsScript.Settings.AIPower]);
        AIPlayer.History.Add(5);
        AIPlayer.LastRevenue = (int)(AIPlayer.Budget - GameManagerScript.GM.CrisisBudget);
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
        if (GM.CurrentMonth % 120 == 0)
        {
            AILeader.LeaderID = Random.Range(1, 5);
            AILeader.LeaderType = (LeaderType)Random.Range(0, 4);
        }

        //Ищем страны, где можно установить своё правительство и устанавливаем.
        foreach (var c in CountryScript.Countries())
        {
            if (c.Authority != AIPlayer.Authority)
            {
                GM.ChangeGovernment(c, AIPlayer.Authority);
            }
        }

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
        int balance = AIPlayer.LastRevenue - AIPlayer.TotalYearSpendings();
        BudgetPlus = balance > 1;

        //Определение колонки таблицы
        int column;
        if (BudgetPlus)
            column = 0;
        else
            column = 2;

        if (!war1)
            column += 1;

        //Выбор инвестиций
        if (AIPlayer.PoliticalPoints > 0)
        {
            eventProc = Random.Range(0, 100);

            for (int i = 0; i < investing.dataArray.Length; i++)
            {
                eventProc -= investing.GetValue(i, column);
                if (eventProc < 0)
                {
                    DoInvest(i);
                    ////Если бюджет положительный и будущая инвестиция не выведет его в минус, выполняем эту инвестицию
                    ////Если бюджет отрицательный, уменьшаем инвестицию
                    //if (!BudgetPlus || (balance - 1 * (12 - GM.CurrentMonth % 12) > 1))
                    //    DoInvest(i, BudgetPlus);

                    break;
                }
            }
        }

        //Повторный пересчёт бюджета (он мог измениться в результате предыдущего шага).
        //Если последний прирост бюджета минус предполагаемые траты за этот год > 1, считаем бюджет положительным.
        BudgetPlus = (AIPlayer.LastRevenue - AIPlayer.TotalYearSpendings()) > 1;

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
        int amount = 1;

        //if (BudgetPlus)
        //    amount = 1;
        //else
        //    amount = -1;

        switch (i)
        {
            case 0:
                //diplomat
                AIPlayer.Outlays[OutlayField.diplomat].ChangeOutlet(amount);
                break;
            case 1:
                //military
                //выбираем, куда вкладывать
                int way = Random.Range(0, 4);

                switch (way)
                {
                    case 0:
                        AIPlayer.Outlays[OutlayField.military].ChangeOutlet(amount);
                        break;
                    case 1:
                        if(AIPlayer.GetCurMilTech(OutlayField.air) > 0) //если изучены все технологии в линии, не добавляем инвестиций
                            AIPlayer.Outlays[OutlayField.air].ChangeOutlet(amount);
                        else if (AIPlayer.GetCurMilTech(OutlayField.ground) > 0)    //если все технологии в линии изучены, пробуем изучить другую технологию
                            AIPlayer.Outlays[OutlayField.ground].ChangeOutlet(amount);
                        else if (AIPlayer.GetCurMilTech(OutlayField.sea) > 0)
                            AIPlayer.Outlays[OutlayField.sea].ChangeOutlet(amount);
                        else if (AIPlayer.GetCurMilTech(OutlayField.military) > 0)  //если все линии изучены полностью, вкладываем в войска
                            AIPlayer.Outlays[OutlayField.military].ChangeOutlet(amount);

                        break;
                    case 2:
                        if (AIPlayer.GetCurMilTech(OutlayField.ground) > 0)
                            AIPlayer.Outlays[OutlayField.ground].ChangeOutlet(amount);
                        else if (AIPlayer.GetCurMilTech(OutlayField.sea) > 0)
                            AIPlayer.Outlays[OutlayField.sea].ChangeOutlet(amount);
                        else if (AIPlayer.GetCurMilTech(OutlayField.air) > 0)
                            AIPlayer.Outlays[OutlayField.air].ChangeOutlet(amount);
                        else if (AIPlayer.GetCurMilTech(OutlayField.military) > 0)
                            AIPlayer.Outlays[OutlayField.military].ChangeOutlet(amount);

                        break;
                    case 3:
                        if (AIPlayer.GetCurMilTech(OutlayField.sea) > 0)
                            AIPlayer.Outlays[OutlayField.sea].ChangeOutlet(amount);
                        else if (AIPlayer.GetCurMilTech(OutlayField.air) > 0)
                            AIPlayer.Outlays[OutlayField.air].ChangeOutlet(amount);
                        else if (AIPlayer.GetCurMilTech(OutlayField.ground) > 0)    //если все технологии в линии изучены, пробуем изучить другую технологию
                            AIPlayer.Outlays[OutlayField.ground].ChangeOutlet(amount);
                        else if (AIPlayer.GetCurMilTech(OutlayField.military) > 0)
                            AIPlayer.Outlays[OutlayField.military].ChangeOutlet(amount);

                        break;
                }
                break;
            case 2:
                AIPlayer.Outlays[OutlayField.spy].ChangeOutlet(amount);
                //spy
                break;
            case 3:
                //space
                if (gndSpaceTech)
                {
                    if(AIPlayer.CurGndTechIndex != -1)  //добавляем инвестиции, если изучены не все технологии в линии
                        AIPlayer.Outlays[OutlayField.spaceGround].ChangeOutlet(amount);
                    else if (AIPlayer.CurLnchTechIndex != -1 && AIPlayer.CurGndTechIndex > 1)   //иначе изучаем технологию другой линии
                        AIPlayer.Outlays[OutlayField.spaceLaunches].ChangeOutlet(amount);
                }
                else
                {
                    if(AIPlayer.CurLnchTechIndex != -1 && AIPlayer.CurGndTechIndex > 1)
                        AIPlayer.Outlays[OutlayField.spaceLaunches].ChangeOutlet(amount);
                    else if (AIPlayer.CurGndTechIndex != -1)
                        AIPlayer.Outlays[OutlayField.spaceGround].ChangeOutlet(amount);
                }

                gndSpaceTech = !gndSpaceTech;   //в следующий раз будем инвестировать в другую линию
                break;
            case 5:
                //инвестирование в дополнительный прирост бюджета в начале следующего года
                AIPlayer.AddBudgetGrow();
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
                    if (c.CanAddInf(AIPlayer.Authority) && c.Authority != AIPlayer.Authority && c.OppForce == 0 && c.GetInfluense(AIPlayer.Authority) <= borderInf && c.GetInfluense(AIPlayer.Authority) > 50)
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

                    AddMilitary(actCountry, 1);
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

                    AddMilitary(actCountry, 1);
                }
                break;
            case 2:
            //Ввод войск в военное время
                if (AIPlayer.MilitaryPool == 0)
                    return;

                selectedCountries.Clear();

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
                            AddMilitary(c, 2);
                        else
                            AddMilitary(c, 2);
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
                    if (c.Authority != AIPlayer.Authority && c.GetInfluense(AIPlayer.Authority) > 50 && c.GetInfluense(AIPlayer.Authority) <= 90 && c.SpyCount(AIPlayer.Authority) > 3 && c.CanOrgMeeting(AIPlayer.Authority) && c.Support > 5)
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
                    if (c.Authority != GM.GetOpponentTo(AIPlayer).Authority && c.GetInfluense(AIPlayer.Authority) < c.GetInfluense(GM.GetOpponentTo(AIPlayer).Authority) && c.SpyCount(AIPlayer.Authority) > 3 && c.CanOrgParade(AIPlayer.Authority) && c.Support < 60)
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

    public void AddMilitary(CountryScript Country, int Amount)
    {
        if (AIPlayer.MilitaryPool == 0)
            return;

        GameManagerScript GM = GameManagerScript.GM;
        bool milType = false;

        Amount = Mathf.Min(Amount, AIPlayer.MilitaryPool);  //Если, вдруг, пытаемся ввести больше, чем есть в пуле.
        milType = Country.AddMilitary(AIPlayer.Authority, Amount);

        //Видео
        GM.VQueue.AddRolex(GM.GetMySideVideoType(AIPlayer.Authority), VideoQueue.V_PRIO_NULL, milType ? VideoQueue.V_PUPPER_MIL_ADDED : VideoQueue.V_PUPPER_REV_ADDED, Country);
    }


    public int NewYearBonus()
    {
        return YearCheat[SettingsScript.Settings.AIPower];
    }
}

