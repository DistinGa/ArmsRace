using UnityEngine;
using System.Collections.Generic;

public class CountryScript : MonoBehaviour
{
    static Color amColor = new Color(0, 0, 1);
    static Color sovColor = new Color(1, 0, 0);
    static Color neuColor = new Color(1, 1, 0);

    public Sprite FlagS;
    public Sprite FlagNs;

    [Space(10)]
    public string Name;
    public Region Region;
    public Authority Authority;
    public int Score;
    [SerializeField]
    private float support;
    [Space(10)]
    public int SovInf;
    public int AmInf;
    public int NInf;
    public Authority LastAut;   //чьё влияние было установлено последним
    [Space(10)]
    public int GovForce;
    public int OppForce;
    [Space(10)]
    public int KGB;
    public int CIA;
    [Space(10)]
    //особенности территории
    public int Air = 1;
    public int Ground = 1;
    public int Sea = 1;

    private Transform StatePanel;
    public List<StateSymbol> Symbols = new List<StateSymbol>();

    [HideInInspector]
    public int DiscounterUsaMeeting, DiscounterRusMeeting; //Сколько ждать до возможности следующего митинга протеста (0 - можно митинговать)
    [HideInInspector]
    public int DiscounterUsaParade, DiscounterRusParade; //Сколько ждать до возможности следующего парада (0 - можно)
    [HideInInspector]
    public int DiscounterUsaSpy, DiscounterRusSpy, DiscounterUsaInfl, DiscounterRusInfl; //Дискаунтер для возможности засылки шпионов или повышения влияния (0 - можно)

    // Use this for initialization
    void Start()
    {
        SetAuthority();
        StatePanel = transform.Find("Capital/Canvas/Panel");
    }

    //Возвращает список всех стран
    public static List<CountryScript> Countries()
    {
        List<CountryScript> result = new List<CountryScript>();

        GameObject Countries = GameObject.Find("Countries");
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            result.Add(Countries.transform.GetChild(idx).GetComponent<CountryScript>());
        }

        return result;
    }

    //Возвращает список всех стран определённой принадлежности
    public static List<CountryScript> Countries(Authority aut)
    {
        List<CountryScript> result = new List<CountryScript>();

        GameObject Countries = GameObject.Find("Countries");
        CountryScript Country;
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            Country = Countries.transform.GetChild(idx).GetComponent<CountryScript>();
            if (Country.Authority == aut)
                result.Add(Countries.transform.GetChild(idx).GetComponent<CountryScript>());
        }

        return result;
    }

    //Установка цвета границы.
    public void SetAuthority()
    {
        Material mat = GetComponent<Renderer>().material;
        Renderer rend = GetComponent<Renderer>();

        switch (Authority)
        {
            case Authority.Neutral:
                rend.enabled = false;
                //mat.SetColor("_TintColor", new Color());
                break;
            case Authority.Amer:
                rend.enabled = true;
                mat.SetColor("_TintColor", new Color(amColor.r, amColor.g, amColor.b, mat.GetColor("_TintColor").a));
                break;
            case Authority.Soviet:
                rend.enabled = true;
                mat.SetColor("_TintColor", new Color(sovColor.r, sovColor.g, sovColor.b, mat.GetColor("_TintColor").a));
                break;
            default:
                break;
        }
    }

    public void OnMouseUpAsButton()
    {
        CameraScriptXZ cam = FindObjectOfType<CameraScriptXZ>();
        if (!cam.setOverMenu)
            GameManagerScript.GM.SnapToCountry(Camera.main.ScreenToWorldPoint(Input.mousePosition), this);
    }

    //Добавление влияния.
    //Inf - чьё влияние добавляется.
    //Auto - true: влияние добавляется в результате случайного события или от космогонки.
    //      false: явное добавление
    public void AddInfluence(Authority Inf, int Amount, bool Auto = true)
    {
        int diplomatsDecrease = 1;
        PlayerScript pl = GameManagerScript.GM.GetPlayerByAuthority(Inf);

        switch (Inf)
        {
            case Authority.Neutral:
                //определяем максимально допустимое количество влияния, которое может быть добавлено/отнято
                if (Amount > 0)
                    Amount = Mathf.Min(Amount, 100 - NInf);
                else
                    Amount = -Mathf.Min(-Amount, NInf);

                if (!Auto)
                {
                    diplomatsDecrease = Amount;
                    //бонус лидера
                    Amount *= pl.PlayerLeader.GetInfluenceBoost();
                }

                NInf += Amount;

                //Распределяем "минус" по другим влияниям.
                AmInf -= Amount / 2;
                SovInf -= (Amount - Amount / 2);  //чтобы не накапливалось расхождение из-за округления
                //Если американского влияния было мало, отнимаем остаток от советсткого, а американское обнуляем
                if (AmInf < 0)
                {
                    SovInf += AmInf;
                    AmInf = 0;
                }

                //Если советсткого влияния было мало, отнимаем остаток от американского, а советское обнуляем
                if (SovInf < 0)
                {
                    AmInf += SovInf;
                    SovInf = 0;
                }
                break;
            case Authority.Amer:
                //определяем максимально допустимое количество влияния, которое может быть добавлено/отнято
                if (Amount > 0)
                    Amount = Mathf.Min(Amount, 100 - AmInf);
                else
                    Amount = -Mathf.Min(-Amount, AmInf);

                if (!Auto)
                {
                    diplomatsDecrease = Amount;
                    //бонус лидера
                    Amount *= pl.PlayerLeader.GetInfluenceBoost();
                }

                AmInf += Amount;
                DiscounterUsaInfl = GameManagerScript.GM.MAX_INFLU_CLICK;

                //Распределяем "минус" по другим влияниям.
                NInf -= Amount; //Сначала отнимаем от нейтрального влияния
                if (NInf < 0)    //Если нейтрального влияния не хватило, отнимаем от влияния соперника.
                {
                    SovInf += NInf; //NInf отрицательное
                    NInf = 0;
                    if (SovInf < 0) //Если и влияния соперника не хватило, значит была попытка добавить слишком большое влияние.
                    {
                        SovInf = 0;
                        AmInf = 100;
                    }
                }
                break;
            case Authority.Soviet:
                //определяем максимально допустимое количество влияния, которое может быть добавлено/отнято
                if (Amount > 0)
                    Amount = Mathf.Min(Amount, 100 - SovInf);
                else
                    Amount = -Mathf.Min(-Amount, SovInf);

                if (!Auto)
                {
                    diplomatsDecrease = Amount;
                    //бонус лидера
                    Amount *= pl.PlayerLeader.GetInfluenceBoost();
                }

                SovInf += Amount;
                DiscounterRusInfl = GameManagerScript.GM.MAX_INFLU_CLICK;

                //Распределяем "минус" по другим влияниям.
                NInf -= Amount; //Сначала отнимаем от нейтрального влияния
                if (NInf < 0)    //Если нейтрального влияния не хватило, отнимаем от влияния соперника.
                {
                    AmInf += NInf; //NInf отрицательное
                    NInf = 0;
                    if (AmInf < 0) //Если и влияния соперника не хватило, значит была попытка добавить слишком большое влияние.
                    {
                        SovInf = 100;
                        AmInf = 0;
                    }
                }
                break;
        }

        //SovInf = Mathf.Clamp(SovInf, 0, 100);
        //AmInf = Mathf.Clamp(AmInf, 0, 100);

        if (!Auto)
        {
            LastAut = Inf;
            if(Amount > 0)
                pl.DiplomatPool -= diplomatsDecrease;
        }
    }

    //Добавление шпионов.
    //Inf - чей шпион добавляется.
    //cheat - если true, шпионы не вычитаются из пула, и не изменяется дискаунтер (читинг)
    public void AddSpy(Authority Inf, int Amount, bool cheat = false)
    {
        GameManagerScript GM = GameManagerScript.GM;

        if (Inf == Authority.Amer)
        {
            if(CIA + Amount > 5)
                Amount = Amount - (CIA + Amount - 5);

            CIA += Amount;
        }
        if (Inf == Authority.Soviet)
        {
            if (KGB + Amount > 5)
                Amount = Amount - (KGB + Amount - 5);

            KGB += Amount;
        }

        if (CIA < 0) CIA = 0;
        if (KGB < 0) KGB = 0;

        if (!cheat)
        {
            if (Amount > 0)
                GM.GetPlayerByAuthority(Inf).SpyPool -= Amount;

            //устанавливаем дискаунтер, чтобы отключить возможность повторного повышения в пределах отведённого периода.
            if (Inf == Authority.Amer)
                DiscounterUsaSpy = GM.MAX_SPY_CLICK;
            if (Inf == Authority.Soviet)
                DiscounterRusSpy = GM.MAX_SPY_CLICK;
        }
    }

    //Добавление вооружённых сил.
    //Inf - чьи силы добавляются.
    //cheat - если true, военные не вычитаются из пула
    //возвращает true, если добавили правительственные силы и false - если оппозиционные (нужно для показа новостей)
    public bool AddMilitary(Authority Inf, int Amount, bool cheat = false)
    {
        PlayerScript pl = GameManagerScript.GM.GetPlayerByAuthority(Inf);

        bool res = false;

        if (Authority == Authority.Neutral)
        {
            if (SovInf > AmInf) //Если советсткое влияние, то советский игрок добавляет нейтральные силы, американский - оппозиционные.
            {
                if (Inf == Authority.Soviet)
                {
                    GovForce += Amount;
                    res = true;
                }
                else
                    OppForce += Amount;
            }
            else //Если влияние проамериканское, то американский игрок добавляет нейтральные силы, советский - оппозиционные.
            {
                if (Inf == Authority.Amer)
                {
                    GovForce += Amount;
                    res = true;
                }
                else
                    OppForce += Amount;
            }
        }
        else    //Если режим страны не нейтральный, то игрок, чей режим установлен, добавляет правительственные силы. Другой игрок добавляет оппозиционные силы.
        {
            if (Inf == Authority)
            {
                GovForce += Amount;
                res = true;
            }
            else
                OppForce += Amount;
        }

        if(Amount > 0 && !cheat)
            pl.MilitaryPool -= Amount;

        //Устранение выхода за допустимую границу.
        int delta;
        if (res)
        {
            delta = 10 - GovForce;
            if (delta < 0)
                pl.MilitaryPool -= delta;   //превысили предел 10, возвращаем в пул разницу
        }
        else
        {
            delta = 10 - OppForce;
            if (delta < 0)
                pl.MilitaryPool -= delta;   //превысили предел 10, возвращаем в пул разницу
        }

        GovForce = Mathf.Clamp(GovForce, 0, 10);
        OppForce = Mathf.Clamp(OppForce, 0, 10);

        return res;
    }

    public Transform Capital
    {
        get { return transform.FindChild("Capital"); }
    }

    public float Support
    {
        get {return support;}

        set
        {
            support = value;
            if (support < 0f) support = 0f;
            if (support > 100f) support = 100f;
        }
    }

    //Проверка возможности добавить влияние
    public bool CanAddInf(Authority Aut)
    {
        return (GameManagerScript.GM.GetPlayerByAuthority(Aut).DiplomatPool > 0 && ((Aut == Authority.Amer && DiscounterUsaInfl == 0 && AmInf < 100) || (Aut == Authority.Soviet && DiscounterRusInfl == 0 && SovInf < 100)));
    }

    //Проверка возможности добавить войска
    public bool CanAddMil(Authority Aut)
    {
        GameManagerScript GM = GameManagerScript.GM;

        return (GameManagerScript.GM.GetPlayerByAuthority(Aut).MilitaryPool > 0 &&
            ((Aut == Authority && GovForce < 10) ||  //свои войска 
            (Authority == Authority.Neutral && Aut == Authority.Amer && (AmInf > SovInf) && GovForce < 10) ||   //поддержка нейтрального правительства
            (Authority == Authority.Neutral && Aut == Authority.Soviet && (AmInf < SovInf) && GovForce < 10) || //поддержка нейтрального правительства
            (Support <= (100 - GM.INSTALL_PUPPER_REVOL) && Authority != Authority.Neutral && Authority != Aut && OppForce < 10) ||    //оппозиция в чужой стране
            (Support <= (100 - GM.INSTALL_PUPPER_REVOL) && Authority == Authority.Neutral && Aut == Authority.Amer && (AmInf < SovInf) && OppForce < 10) ||  //оппозиция в нейтральной стране
            (Support <= (100 - GM.INSTALL_PUPPER_REVOL) && Authority == Authority.Neutral && Aut == Authority.Soviet && (AmInf > SovInf) && OppForce < 10)    //оппозиция в нейтральной стране
            ));
    }

    //Проверка возможности начать войну
    public bool CanAgression(Authority Aut)
    {
        GameManagerScript GM = GameManagerScript.GM;

        return (Aut != Authority && OppForce == 0 && Support <= (100 - GM.INSTALL_PUPPER_REVOL)) &&
            ((Authority != Authority.Neutral) ||    //оппозиция в чужой стране
            (Authority == Authority.Neutral && Aut == Authority.Amer && (AmInf < SovInf)) ||    //американская оппозиция в нейтральной стране
            (Authority == Authority.Neutral && Aut == Authority.Soviet && (AmInf > SovInf))     //советская оппозиция в нейтральной стране
            );
    }

    //Проверка возможности добавить шпиона
    public bool CanAddSpy(Authority Aut)
    {
        return (GameManagerScript.GM.GetPlayerByAuthority(Aut).SpyPool > 0 && ((Aut == Authority.Amer && DiscounterUsaSpy == 0 && CIA < 5) || (Aut == Authority.Soviet && DiscounterRusSpy == 0 && KGB < 5)));
    }

    //Проверка возможности организовать восстание
    public bool CanOrgMeeting(Authority Aut)
    {
        return (HaveSpy(Aut) && Authority != Aut && ((Aut == Authority.Amer && DiscounterUsaMeeting == 0) || (Aut == Authority.Soviet && DiscounterRusMeeting == 0)));
    }

    //Проверка возможности организовать парад
    public bool CanOrgParade(Authority Aut)
    {
        return (HaveSpy(Aut) && (Authority == Aut || Authority == Authority.Neutral) && ((Aut == Authority.Amer && DiscounterUsaParade == 0) || (Aut == Authority.Soviet && DiscounterRusParade == 0)));
    }

    //Проверка возможности сменить правительство
    //Aut - на какое правительство хотим поменять
    public bool CanChangeGov(Authority Aut)
    {
        return (Authority != Aut && Support <= (100 - GameManagerScript.GM.INSTALL_PUPPER_OPPO) && GetInfluense(Aut) >= GameManagerScript.GM.INSTALL_PUPPER_INFLU);
    }

    //Проверка наличия шпионов
    //Aut - чьих шпионов проверяем
    public bool HaveSpy(Authority Aut)
    {
        return (Aut == Authority.Amer && CIA > 0) || (Aut == Authority.Soviet && KGB > 0);
    }

    //Возвращает количество шпионов заданной стороны
    public int SpyCount(Authority Aut)
    {
        int res = 0;

        switch (Aut)
        {
            case Authority.Amer:
                res = CIA;
                break;
            case Authority.Soviet:
                res = KGB;
                break;
            default:
                break;
        }
        return res;
    }

    //Смена правительства
    public void ChangeGov(Authority NewAut)
    {
        Support = 50;

        if (Authority == Authority.Neutral && GovForce >= 8)
        {
            //Если в нейтральной стране на момент 80 оппозиция и 80 инфлуенс еще есть 8 или более нейтральных милитари то переворот-революция происходит без войны
            Authority = NewAut;
        }
        else
        {
            Authority = NewAut;

            //меняем местами войска
            int mil = GovForce;
            GovForce = OppForce;
            OppForce = mil;

            //Читтинг. Добавляем 3 военных и 1 шпиона. (Чтобы не возникало ситуации, когда после смены власти сразу происходит революция, т.к. нет правительственных сил)
            if (GovForce < 3)
                GovForce = 3;
            if (NewAut != Authority.Neutral && !HaveSpy(NewAut))
            {
                GameManagerScript.GM.GetPlayerByAuthority(NewAut).SpyPool += 1;
                AddSpy(NewAut, 1);
            }
        }

        SetAuthority(); //Смена цвета границ

        ////Steam achievments
        //GameManagerScript GM = GameManagerScript.GM;

        //if (Name == "East Germany" || Name == "West Germany")
        //{
        //    if(GM.FindCountryById(12).Authority == GM.Player.Authority && GM.FindCountryById(24).Authority == GM.Player.Authority)
        //        SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_2");
        //}

        //if (Name == "South Korea" || Name == "North Korea")
        //{
        //    if (GM.FindCountryById(53).Authority == GM.Player.Authority && GM.FindCountryById(54).Authority == GM.Player.Authority)
        //        SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_3");
        //}
    }

    //Обработка начала месяца
    public void NextMonth()
    {
        GameManagerScript GM = GameManagerScript.GM;

        //Уменьшаем дискаунтеры
        if (DiscounterRusInfl > 0) DiscounterRusInfl--;
        if (DiscounterRusMeeting > 0) DiscounterRusMeeting--;
        if (DiscounterRusParade > 0) DiscounterRusParade--;
        if (DiscounterRusSpy > 0) DiscounterRusSpy--;
        if (DiscounterUsaInfl > 0) DiscounterUsaInfl--;
        if (DiscounterUsaMeeting > 0) DiscounterUsaMeeting--;
        if (DiscounterUsaParade > 0) DiscounterUsaParade--;
        if (DiscounterUsaSpy > 0) DiscounterUsaSpy--;

        //Обработка значков состояний
        for(int indx = Symbols.Count-1; indx >=0; indx--)
        {
            StateSymbol item = Symbols[indx];

            if (--item.MonthsToShow <= 0)
            {
                Destroy(item.Symbol.gameObject);
                Symbols.Remove(item);
            }
        }

        //Если влияние соответствует правительству, поддержка увеличивается.
        if ((Authority == Authority.Amer && AmInf > 50) || (Authority == Authority.Soviet && SovInf > 50))
        {
            Support += GM.SUPPORT_GROW;
            if (Support > 100) Support = 100;
        }

        //Если влияние не соответствует правительству, растёт оппозиция.
        if ((Authority == Authority.Amer && SovInf > 50) ||
            (Authority == Authority.Soviet && AmInf > 50) ||
            (Authority == Authority.Neutral && (SovInf + AmInf) > 50))
        {
            Support -= GM.OPPO_GROW;
            if (Support < 0) Support = 0;
        }
    }

    //Добавление состояния
    public void AddState(States state, Authority aut, int lifeTime)
    {
        bool exist = false;

        foreach (var item in Symbols)
        {
            if (state == item.State && aut == item.Authority)
            {
                exist = true;
                break;
            }
        }

        if(!exist)
            Symbols.Add(new StateSymbol(state, aut, lifeTime, this));
    }

    //Удаление состояния
    public void DelState(States state, Authority aut)
    {
        bool exist = false;
        StateSymbol ss = null;

        foreach (var item in Symbols)
        {
            if (state == item.State && aut == item.Authority)
            {
                exist = true;
                ss = item;
                break;
            }
        }

        if (exist)
        {
            Destroy(ss.Symbol.gameObject);
            Symbols.Remove(ss);
        }
    }

    //Проверка состояний в стране
    public void TestStates()
    {
        //Проверка возможности мирно сменить власть обеими державами
        if (CanChangeGov(Authority.Amer))
            AddState(States.SYM_PEACE, Authority.Amer, 10000);
        else
            DelState(States.SYM_PEACE, Authority.Amer);

        if (CanChangeGov(Authority.Soviet))
            AddState(States.SYM_PEACE, Authority.Soviet, 10000);
        else
            DelState(States.SYM_PEACE, Authority.Soviet);

        //Проверка возможности ввода оппозиционных войск
        Authority Aut = Authority.Neutral;
        //Сначала убираем значки обеих держав
        DelState(States.SYM_REVOL, Authority.Amer);
        DelState(States.SYM_REVOL, Authority.Soviet);

        //Возможность начала революции
        if (Support <= 100 - GameManagerScript.GM.INSTALL_PUPPER_REVOL)
        {
            switch (Authority)
            {
                case Authority.Neutral:
                    if (AmInf < SovInf)
                        Aut = Authority.Amer;
                    if (SovInf < AmInf)
                        Aut = Authority.Soviet;
                    break;
                case Authority.Amer:
                    Aut = Authority.Soviet;
                    break;
                case Authority.Soviet:
                    Aut = Authority.Amer;
                    break;
            }
        }

        if (Aut != Authority.Neutral)
        {
            AddState(States.SYM_REVOL, Aut, 10000);
        }

        //Проверка состояния войны
        if (GovForce > 0 && OppForce > 0)
        {
            AddState(States.SYM_WAR, Authority.Amer, 10000);
            //Добавление страны в правый список
            GameManagerScript.GM.AddWarFlag(this);
        }
        else
        {
            //Война закончилась
            DelState(States.SYM_WAR, Authority.Amer);
            //Удаление страны из правого списка
            GameManagerScript.GM.RemoveWarFlag(this);

            //Удаление роликов о войне
            GameManagerScript.GM.VQueue.ClearVideoQueue(this, VideoQueue.V_PUPPER_REVOLUTION);
        }

        //Удаление "просроченных" состояний
        foreach (var item in Symbols)
        {
            if (item.MonthsToShow <= 0)
                DelState(item.State, item.Authority);
        }
    }

    //Возвращает влияние заданной стороны
    public int GetInfluense(Authority aut)
    {
        int result = 0;

        switch (aut)
        {
            case Authority.Neutral:
                result = NInf;
                break;
            case Authority.Amer:
                result = AmInf;
                break;
            case Authority.Soviet:
                result = SovInf;
                break;
        }

        return result;
    }

    //Возвращает количество военных альянса
    public int GetMilitary(Authority aut)
    {
        int res = 0;

        if (Authority == aut)
            res = GovForce; //свои
        else
        {
            if (Authority != Authority.Neutral)
                res = OppForce; //противника
            else
            {
                if (GetInfluense(aut) > GetInfluense(GameManagerScript.GM.GetOpponentTo(GameManagerScript.GM.GetPlayerByAuthority(aut)).Authority))
                    res = GovForce; //если страна нейтральная и запрашиваемое влияние больше - правительственные войска
                else
                    res = OppForce;
            }
        }

        return res;
    }

    //Сохранение/загрузка
    public SavedCountryData GetSavedData()
    {
        SavedCountryData res = new SavedCountryData();

        res.Name = name;
        res.support = support;
        res.SovInf = SovInf;
        res.AmInf = AmInf;
        res.NInf = NInf;
        res.LastAut = LastAut;
        res.CurAut = Authority;
        res.GovForce = GovForce;
        res.OppForce = OppForce;
        res.KGB = KGB;
        res.CIA = CIA;
        //res.Symbols = Symbols;
        res.DiscounterUsaMeeting = DiscounterUsaMeeting;
        res.DiscounterRusMeeting = DiscounterRusMeeting;
        res.DiscounterUsaParade = DiscounterUsaParade;
        res.DiscounterRusParade = DiscounterRusParade;
        res.DiscounterUsaSpy = DiscounterUsaSpy;
        res.DiscounterRusSpy = DiscounterRusSpy;
        res.DiscounterUsaInfl = DiscounterUsaInfl;
        res.DiscounterRusInfl = DiscounterRusInfl;

        return res;
    }

    public void SetSavedData(SavedCountryData sd)
    {
        support = sd.support;
        SovInf = sd.SovInf;
        AmInf = sd.AmInf;
        NInf = sd.NInf;
        LastAut = sd.LastAut;
        Authority = sd.CurAut;
        SetAuthority();
        GovForce = sd.GovForce;
        OppForce = sd.OppForce;
        KGB = sd.KGB;
        CIA = sd.CIA;
        //Symbols = sd.Symbols;
        DiscounterUsaMeeting = sd.DiscounterUsaMeeting;
        DiscounterRusMeeting = sd.DiscounterRusMeeting;
        DiscounterUsaParade = sd.DiscounterUsaParade;
        DiscounterRusParade = sd.DiscounterRusParade;
        DiscounterUsaSpy = sd.DiscounterUsaSpy;
        DiscounterRusSpy = sd.DiscounterRusSpy;
        DiscounterUsaInfl = sd.DiscounterUsaInfl;
        DiscounterRusInfl = sd.DiscounterRusInfl;

        TestStates();
    }

    //перечисление состояний страны
    public enum States
    {
        SYM_PEACE, // дипломат-смена мирным путем возможна
        SYM_REVOL, // автомат -смена военным путем возможна (ввод революционеров)
        SYM_SPY, // шпион   -пойман
        SYM_WAR, // идут военные действия
        SYM_PARAD, // парад
        SYM_RIOT // митинг
    }

    // описание значка для состояния страны
    [System.Serializable]
    public class StateSymbol
    {
        public States State;    //Состояние
        public Authority Authority; // за какую сторону
        public int MonthsToShow; //сколько месяцев показывать (discaunter)
        public RectTransform Symbol; // сам значок

        // конструктор
        public StateSymbol(States state, Authority authority, int life, CountryScript Country)
        {
            State = state;
            Authority = authority;
            MonthsToShow = life;

            Symbol = Instantiate(GameManagerScript.GM.StatePrefab);
            Symbol.SetParent(Country.StatePanel);
            Symbol.GetComponent<StatePrefab>().Init((int)state, authority);
        }

    }
}

