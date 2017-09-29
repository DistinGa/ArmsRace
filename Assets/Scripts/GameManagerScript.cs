﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public delegate void dlgtMonthTick();

[RequireComponent(typeof(VideoQueue))]
public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript GM;
    public PlayerScript Player;
    public AI AI;

    private Camera MainCamera;
    private CameraScriptXZ CameraRig;

    private RectTransform DownMenu;
    private RectTransform UpMenu;
    private RectTransform WarFlagsPanel;
    private CountryScript Country;  //Выбранная в данный момент страна
    public VideoQueue VQueue;  //Видео-очередь

    public GameObject[] Menus;
    [Space(10)]
    public GameObject Marker;    //Маркер указывающий на страну, с которой работаем.
    public GameObject PausePlate;    //Надпись "Pause"
    public RectTransform StatePrefab;   //префаб значка состояния страны
    public RectTransform FlagButtonPrefab;   //префаб флага в правой панели
    public GameObject WarAnimationPrefab;   //префаб глобальной анимации войны
    public GInfPanelScript LeaderPanelUSA;
    public GInfPanelScript LeaderPanelUSSR;
    [Space(10)]
    public Sprite SignUSA;
    public Sprite SignSU;

    [SerializeField]
    int mMonthCount = -1;     // счетчик месяцев с нуля (-1 потому что в первом кадре значение уже увеличивается)
    float TickCount;        //время одного тика
    public bool IsPaused;  //игра на паузе
    public bool delayedStop = false;   //игра фактически закончена, но сцена пока не выгружается, например из-за анимации
    bool turnBased;  //игра в пошаговом режиме

    [Space(10)]
    [Tooltip("время (сек) между итерациями")]
    public float Tick = 6;   //время (сек) между итерациями
    [Tooltip("начальный бюджет игрока")]
    public int START_BUDGET = 300; // начальный бюджет игрока
    [Tooltip("мин.бюджет")]
    public int MIN_BUDGET = 200; // мин.бюджет
    [Tooltip("общее число месяцев игры")]
    public int MAX_MONTHS_NUM = 600; // общее число месяцев игры

    [Tooltip("раз во сколько месяцев можно повышать влияние")]
    public int MAX_INFLU_CLICK = 1;
    [Tooltip("раз во сколько месяцев можно засылать шпиона")]
    public int MAX_SPY_CLICK = 1;
    [Tooltip("раз во сколько месяцев можно поддерживать восстания")]
    public int MAX_RIOT_MONTHS = 3;
    [Tooltip("раз во сколько месяцев можно поддерживать парады")]
    public int MAX_PARAD_MONTHS = 3;

    [Tooltip("ежемесячный рост поддержки")]
    public float SUPPORT_GROW = 0.1f;
    [Tooltip("ежемесячный рост оппозиции")]
    public float OPPO_GROW = 0.1f;
    [Tooltip("необходимый % pro-влияния для смены правительства")]
    public float INSTALL_PUPPER_INFLU = 80;
    [Tooltip("необходимый % оппозиции для смены правительства")]
    public float INSTALL_PUPPER_OPPO = 80;
    [Tooltip("необходимый % оппозиции для ввода революционеров")]
    public float INSTALL_PUPPER_REVOL = 80;
    [SerializeField]
    [Tooltip("стоимость вооруженных сил")]
    private int MILITARY_COST = 3;
    [SerializeField]
    [Tooltip("стоимость добавления шпиона")]
    private int SPY_COST = 1;
    ////////////
    //новые параметры
    public MilDepMenuScript MDInstance;
    public SpaceRace SRInstance;
    [Tooltip("прирост firepower за одну открытую технологию")]
    public int FirePowerPerTech = 10;
    [SerializeField]
    [Tooltip("стоимость обучения дипломата")]
    private int DiplomatCost = 25;
    [Tooltip("количество изменений трат в год")]
    public int OutlayChangesPerYear;
    //делегат для запуска событий конца месяца
    //(только обновление выводимой информации, не нужно сюда добавлять действия)
    dlgtMonthTick monthSubscribers;
    //Массив с продолжительностями тиков основного цикла
    [Tooltip("Массив с продолжительностями тиков основного цикла")]
    public int[] GameSpeedPrefs = new int[3];
    //Индекс текущей скорости. По умолчанию 1 - средняя скорость.
    [Tooltip("Индекс текущей скорости")]
    public int curSpeedIndex = 1;
    [Tooltip("Бюджет, при достижении которого наступает финансовый кризис")]
    public double CrisisBudget = 500;
    //Ссылка на ScriptableObject с настройками бонусов лидеров
    public SOLP LeaderPrefs;
    public GameObject CountryLabelPrefab;
    [SerializeField]
    RectTransform MakeTurnButton;

    [Space(10)]
    public Armageddon DLC_Armageddon;

    public CountryScript CurrentCountry
    {
        get { return Country; }
    }

    public int CurrentSpeed
    {
        get { return curSpeedIndex; }
        set
        {
            curSpeedIndex = Mathf.Clamp(value, 0, GameSpeedPrefs.Length - 1);
            Tick = GameSpeedPrefs[curSpeedIndex];
        }
    }

    public bool IsTurnBased
    {
        get { return turnBased; }
        set
        {
            turnBased = value;
            MakeTurnButton.gameObject.SetActive(value);
        }
    }

    public void Awake()
    {
        GM = this;
    }

    // Use this for initialization
    void Start()
    {
        if(SettingsScript.Settings.playerSelected == Authority.Amer)
            Player = transform.Find("AmerPlayer").GetComponent<PlayerScript>();
        else
            Player = transform.Find("SovPlayer").GetComponent<PlayerScript>();

        Player.PlayerLeader = SettingsScript.Settings.PlayerLeader;

        //MainCamera = FindObjectOfType<Camera>();
        MainCamera = FindObjectOfType<Camera>();
        CameraRig = FindObjectOfType<CameraScriptXZ>();
        DownMenu = GameObject.Find("DownMenu").GetComponent<RectTransform>();
        UpMenu = GameObject.Find("UpMenu").GetComponent<RectTransform>();
        WarFlagsPanel = GameObject.Find("WarFlagsPanel/Panel/Flags").GetComponent<RectTransform>();
        Marker.GetComponent<Image>().sprite = Player.GetComponent<PlayerScript>().SprMarker;

        SnapToCountry(Player.MyCountry);
        VQueue = FindObjectOfType<VideoQueue>();

        Tick = GameSpeedPrefs[curSpeedIndex];
        Player.Budget = START_BUDGET;
        Player.History2.Add(START_BUDGET);
        Player.History.Add(5);

        IsTurnBased = SettingsScript.Settings.mTurnBaseOn;
    }

    void Update()
    {
        //Загрузка, если необходима (в первый update, чтобы все скрипты успели проинициализироваться)
        if (SettingsScript.Settings.NeedLoad)
        {
            Load();
            SettingsScript.Settings.NeedLoad = false;
        }

        //первый update -особый
        if (mMonthCount == -1)
        {
            NextTick();
            return;
        }

        //регулирование скорости игры: "+" - увеличение, "-" - уменьшение.
        if (Input.GetKeyUp(KeyCode.KeypadPlus) || Input.GetKeyUp(KeyCode.Equals) || Input.GetKeyUp(KeyCode.UpArrow))
            ChangeSpeed(1);
        if (Input.GetKeyUp(KeyCode.Minus) || Input.GetKeyUp(KeyCode.KeypadMinus) || Input.GetKeyUp(KeyCode.DownArrow))
            ChangeSpeed(-1);

        //Пауза
        if (Input.GetKeyDown(KeyCode.P))
        {
            IsPaused = !IsPaused;
            PausePlate.SetActive(IsPaused);

        }

        if (turnBased)
            MakeTurnButton.GetComponent<Button>().interactable = video3D.Video3D.V3Dinstance.NewsListIsEmpty;

        if (IsPaused || delayedStop || turnBased)
            return;

        TickCount -= Time.deltaTime;
        if (TickCount <= 0)
        {
            NextTick();
        }
    }

    public void NextTick()
    {
        TickCount = Tick;

        //Проверяем на конец игры по времени
        if (mMonthCount > MAX_MONTHS_NUM)
        {
            StopGame();
            return;
        }

        NextMonth();

        // прошел год?
        if (mMonthCount % 12 == 0 && mMonthCount > 0)
            NewYear();

        //Обновление информации в верхнем меню
        ShowHighWinInfo();

        //Проверяем конец игры по доминации
        PlayerScript winPlayer = DLC_Armageddon.GetWinner(SettingsScript.Settings.AIPower);
        //Обновление информации в нижнем меню
        SnapToCountry();
    }

    public int GetSpyCost(PlayerScript pl)
    {
        float res = SPY_COST;

        //бонус типа лидера
        res -= res * pl.PlayerLeader.GetSpyDiscount();

        return Mathf.RoundToInt(res);
    }

    public int GetDiplomatCost(PlayerScript pl)
    {
        float res = DiplomatCost;

        //бонус типа лидера
        res -= res * pl.PlayerLeader.GetDipDiscount();

        return Mathf.RoundToInt(res);
    }

    public int GetMilitaryCost(PlayerScript pl)
    {
        int res = MILITARY_COST;

        return res;
    }

    public void ToggleTechMenu(GameObject Menu)
    {
        //Если меню активно - выключаем.
        if (Menu.activeSelf)
            Menu.SetActive(false);
        else
        //Если меню не активно - включаем его и выключаем другие меню.
        {
            foreach (var item in Menus)
            {
                item.SetActive(item == Menu);
            }
        }
    }

    public void ToggleGameObject(GameObject GO)
    {
        GO.SetActive(!GO.activeSelf);
    }

    public void LoadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    //Переход к карте страны (в столицу)
    public void SnapToCountry(CountryScript c)
    {
        CameraRig.SetNewPosition(c.Capital);
        SnapToCountry(c.Capital.position, c);
    }

    //Переход к карте под курсором
    public void SnapToCountry(Vector3 MarkerPosition, CountryScript c)
    {
        Marker.transform.position = new Vector3(MarkerPosition.x, Marker.transform.position.y, MarkerPosition.z);
        Country = c;
        SnapToCountry();
    }

    //Переход к текущей карте (обновление выводимой информации)
    private void SnapToCountry()
    {
        //Заполнение значений в нижнем меню
        DownMenu.FindChild("Flag").GetComponent<Image>().sprite = Country.Authority == Authority.Soviet ? Country.FlagS : Country.FlagNs;
        DownMenu.FindChild("Score").GetComponent<Text>().text = Country.Score + " score";
        DownMenu.FindChild("Sign").GetComponent<Image>().enabled = (Country.Authority != Authority.Neutral);
        string CountryState = "";
        switch (Country.Authority)
        {
            case Authority.Neutral:
                CountryState = "NEUTRAL";
                break;
            case Authority.Amer:
                CountryState = "AMERICAN";
                DownMenu.FindChild("Sign").GetComponent<Image>().sprite = SignUSA;
                break;
            case Authority.Soviet:
                CountryState = "SOVIET";
                DownMenu.FindChild("Sign").GetComponent<Image>().sprite = SignSU;
                break;
        }
        DownMenu.FindChild("CountryState").GetComponent<Text>().text = Country.Name;// + ": GOVERNMENT - PRO " + CountryState;
        DownMenu.FindChild("Page1/Support").GetComponent<Text>().text = Country.Support.ToString("g3");
        DownMenu.FindChild("Page1/Riots").GetComponent<Text>().text = (100 - Country.Support).ToString("g3");
        DownMenu.FindChild("Page1/Budget").GetComponent<Text>().text = Player.Budget.ToString("f0");
        DownMenu.FindChild("Page1/InfAmer").GetComponent<Text>().text = Country.AmInf.ToString("f0");
        DownMenu.FindChild("Page1/InfNeutral").GetComponent<Text>().text = Country.NInf.ToString("f0");
        DownMenu.FindChild("Page1/InfSoviet").GetComponent<Text>().text = Country.SovInf.ToString("f0");
        //пулы
        GameManagerScript GM = GameManagerScript.GM;
        DownMenu.FindChild("Page1/DipPool").GetComponent<Text>().text = GM.Player.DiplomatPool.ToString();
        DownMenu.FindChild("Page1/MilPool").GetComponent<Text>().text = GM.Player.MilitaryPool.ToString();
        DownMenu.FindChild("Page1/SpyPool").GetComponent<Text>().text = GM.Player.SpyPool.ToString();
        //особенности территории
        DownMenu.FindChild("Page2/Air2").GetComponent<Text>().text = Country.Air.ToString();
        DownMenu.FindChild("Page2/Gnd2").GetComponent<Text>().text = Country.Ground.ToString();
        DownMenu.FindChild("Page2/Sea2").GetComponent<Text>().text = Country.Sea.ToString();
        DownMenu.FindChild("Air1").GetComponent<Text>().text = Country.Air.ToString();
        DownMenu.FindChild("Gnd1").GetComponent<Text>().text = Country.Ground.ToString();
        DownMenu.FindChild("Sea1").GetComponent<Text>().text = Country.Sea.ToString();
        //firepower
        PlayerScript usaPlayer = GetPlayerByAuthority(Authority.Amer);
        PlayerScript suPlayer = GetPlayerByAuthority(Authority.Soviet);
        DownMenu.FindChild("Page2/USAAirFP").GetComponent<Text>().text = "(" + Country.Air.ToString() + ") " + (Country.Air * usaPlayer.FirePower(OutlayField.air)).ToString();
        DownMenu.FindChild("Page2/USAGndFP").GetComponent<Text>().text = "(" + Country.Ground.ToString() + ") " + (Country.Ground * usaPlayer.FirePower(OutlayField.ground)).ToString();
        DownMenu.FindChild("Page2/USASeaFP").GetComponent<Text>().text = "(" + Country.Sea.ToString() + ") " + (Country.Sea * usaPlayer.FirePower(OutlayField.sea)).ToString();
        DownMenu.FindChild("Page2/SUAirFP").GetComponent<Text>().text = "(" + Country.Air.ToString() + ") " + (Country.Air * suPlayer.FirePower(OutlayField.air)).ToString();
        DownMenu.FindChild("Page2/SUGndFP").GetComponent<Text>().text = "(" + Country.Ground.ToString() + ") " + (Country.Ground * suPlayer.FirePower(OutlayField.ground)).ToString();
        DownMenu.FindChild("Page2/SUSeaFP").GetComponent<Text>().text = "(" + Country.Sea.ToString() + ") " + (Country.Sea * suPlayer.FirePower(OutlayField.sea)).ToString();

        int usaFP = Country.Air * usaPlayer.FirePower(OutlayField.air) + Country.Ground * usaPlayer.FirePower(OutlayField.ground) + Country.Sea * usaPlayer.FirePower(OutlayField.sea);
        int suFP = Country.Air * suPlayer.FirePower(OutlayField.air) + Country.Ground * suPlayer.FirePower(OutlayField.ground) + Country.Sea * suPlayer.FirePower(OutlayField.sea);
        DownMenu.FindChild("Page2/USA_FP").GetComponent<Text>().text = usaFP.ToString();
        DownMenu.FindChild("Page2/SU_FP").GetComponent<Text>().text = suFP.ToString();

        //проценты на победу
        DownMenu.FindChild("Page2/USA%").GetComponent<Text>().text = Mathf.RoundToInt(100f * usaFP / (usaFP + suFP)).ToString();
        DownMenu.FindChild("Page2/SU%").GetComponent<Text>().text = (100 - Mathf.RoundToInt(100f * usaFP / (usaFP + suFP))).ToString();

        DownMenu.FindChild("Page1/SpyLeft").GetComponent<Image>().fillAmount = Country.CIA * 0.2f;
        DownMenu.FindChild("Page1/SpyRight").GetComponent<Image>().fillAmount = Country.KGB * 0.2f;

        ShowMilitary();

        //Доступность кнопок
        //Влияние
        DownMenu.FindChild("Page1/AddInfButton").GetComponent<Button>().interactable = Country.CanAddInf(Player.Authority);
        //Войска
        DownMenu.FindChild("Page1/AddMilButton").GetComponent<Button>().interactable = Country.CanAddMil(Player.Authority);
        //Шпионы
        DownMenu.FindChild("Page1/AddSpyButton").GetComponent<Button>().interactable = Country.CanAddSpy(Player.Authority);
        //Организация парада
        DownMenu.FindChild("SupParadeButton").GetComponent<Button>().interactable = Country.CanOrgParade(Player.Authority);
        //Организация восстания
        DownMenu.FindChild("SupRiotButton").GetComponent<Button>().interactable = Country.CanOrgMeeting(Player.Authority);
        //Смена правительства
        DownMenu.FindChild("NewGovButton").GetComponent<Button>().interactable = Country.CanChangeGov(Player.Authority);

        //Включение 3Д анимаций
        if (video3D.Video3D.V3Dinstance != null)
            video3D.Video3D.V3Dinstance.ShowDefaultAnim(Country);
    }

    public void ShowMilitary()
    {
        DownMenu.FindChild("Page1/MilitaryLeft").GetComponent<Image>().fillAmount = 0;
        DownMenu.FindChild("Page1/MilitaryLeft_n").GetComponent<Image>().fillAmount = 0;
        DownMenu.FindChild("Page1/MilitaryRight_n").GetComponent<Image>().fillAmount = 0;
        DownMenu.FindChild("Page1/MilitaryRight").GetComponent<Image>().fillAmount = 0;

        switch (Country.Authority)
        {
            case Authority.Neutral:
                if (Country.SovInf > Country.AmInf)
                {
                    DownMenu.FindChild("Page1/MilitaryRight_n").GetComponent<Image>().fillAmount = Country.GovForce * 0.1f;
                    //Силы оппозиции видны если есть шпионы либо если оппозиция - своя (в данном случае - американская).
                    if (Player.Authority == Authority.Amer || Country.KGB > 0)
                        DownMenu.FindChild("Page1/MilitaryLeft").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                else
                {
                    DownMenu.FindChild("Page1/MilitaryLeft_n").GetComponent<Image>().fillAmount = Country.GovForce * 0.1f;
                    //Силы оппозиции видны если есть шпионы либо если оппозиция - своя (в данном случае - советсткая).
                    if (Player.Authority == Authority.Soviet || Country.CIA > 0)
                        DownMenu.FindChild("Page1/MilitaryRight").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                break;
            case Authority.Amer:
                DownMenu.FindChild("Page1/MilitaryLeft").GetComponent<Image>().fillAmount = Country.GovForce * 0.1f;
                if (Player.Authority == Authority.Amer)
                {
                    //Если нет шпионов, то силы противника не видны
                    if (Country.CIA > 0)
                        DownMenu.FindChild("Page1/MilitaryRight").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                else
                {
                    DownMenu.FindChild("Page1/MilitaryRight").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                break;
            case Authority.Soviet:
                DownMenu.FindChild("Page1/MilitaryRight").GetComponent<Image>().fillAmount = Country.GovForce * 0.1f;
                if (Player.Authority == Authority.Soviet)
                {
                    //Если нет шпионов, то силы противника не видны
                    if (Country.KGB > 0)
                        DownMenu.FindChild("Page1/MilitaryLeft").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                else
                {
                    DownMenu.FindChild("Page1/MilitaryLeft").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                break;
        }
    }

    public void AddInfluence()
    {
        if (Player.DiplomatPool == 0)
            return;

        Country.AddInfluence(Player.Authority, 1, false);

        SnapToCountry();
    }

    public void AddSpy()
    {
        if (Player.SpyPool == 0)
            return;

        Country.AddSpy(Player.Authority, 1);
        SnapToCountry();
    }

    public void AddMilitary()
    {
        if (Player.MilitaryPool == 0)
            return;

        bool mil = Country.AddMilitary(Player.Authority, 1);
        SnapToCountry();

        VQueue.AddRolex(GetMySideVideoType(Player.Authority), VideoQueue.V_PRIO_NULL, mil?VideoQueue.V_PUPPER_MIL_ADDED:VideoQueue.V_PUPPER_REV_ADDED, Country);
    }

    public void OrganizeRiot()
    {
        if (!CallMeeting(Country, Player, false))
            return;

        SoundManager.SM.PlaySound("sound/riot");
        SnapToCountry();
    }

    public void OrganizeParade()
    {
        if (!CallMeeting(Country, Player, true))
            return;

        SoundManager.SM.PlaySound("sound/parad");
        SnapToCountry();
    }

    //Организация митинга в поддержку правительства или против
    //с - страна, в которой организуем
    //p - игрок, который организует
    //parade: true - парад, false - восстание
    public bool CallMeeting(CountryScript c, PlayerScript p, bool parade)
    {
        if (p.SpyPool > 0)
            p.SpyPool--;
        else
            return false;

        //if (!PayCost(p, parade? PARADE_COST: RIOT_COST))
        //    return false; //Не хватило денег

        if (p.Authority == Authority.Amer)
        {
            if (parade)
            {
                c.DiscounterUsaParade = MAX_RIOT_MONTHS;
                c.Support += c.CIA * p.PlayerLeader.GetMeetingBoost(); //увеличиваем поддержку на 1% за каждого шпиона (бонусы лидера могут влиять на это увеличение)
                if (c.Support > 100) c.Support = 100;
                c.AddState(CountryScript.States.SYM_PARAD, Authority.Amer, 3);
                VQueue.AddRolex(VQueue.LocalType(Authority.Amer), VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_SUPPORT, c);
            }
            else
            {
                c.DiscounterUsaMeeting = MAX_RIOT_MONTHS;
                c.Support -= c.CIA * p.PlayerLeader.GetMeetingBoost(); //увеличиваем оппозицию на 1% за каждого шпиона (бонусы лидера могут влиять на это увеличение)
                if (c.Support < 0) c.Support = 0;
                c.AddState(CountryScript.States.SYM_RIOT, Authority.Amer, 3);
                VQueue.AddRolex(VQueue.LocalType(Authority.Amer), VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_RIOTS, c);
            }
        }
        else if (p.Authority == Authority.Soviet)   //то же самое за советскую сторону
        {
            if (parade)
            {
                c.DiscounterRusParade = MAX_RIOT_MONTHS;
                c.Support += c.KGB * p.PlayerLeader.GetMeetingBoost();
                c.AddState(CountryScript.States.SYM_PARAD, Authority.Soviet, 3);
                VQueue.AddRolex(VQueue.LocalType(Authority.Soviet), VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_SUPPORT, c);
            }
            else
            {
                c.DiscounterRusMeeting = MAX_RIOT_MONTHS;
                c.Support -= c.KGB * p.PlayerLeader.GetMeetingBoost();
                c.AddState(CountryScript.States.SYM_RIOT, Authority.Soviet, 3);
                VQueue.AddRolex(VQueue.LocalType(Authority.Soviet), VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_RIOTS, c);
            }
        }

        if (c.Support < 0)
            c.Support = 0;
        if (c.Support > 100)
            c.Support = 100;

        // проверка разоблачения шпиона, организовавшего акцию
        if (c.CIA > 0 && c.KGB > 0)
        {
            int shot = Random.Range(0, c.CIA + c.KGB);

            if (p.Authority == Authority.Amer)
            {
                if (shot > c.CIA)
                {
                    c.CIA--;
                    c.AddInfluence(Authority.Amer, -2);    //обществу не нравиться когда в их стране орудуют чужие шпионы
                    c.AddState(CountryScript.States.SYM_SPY, Authority.Amer, 3);
                }
            }
            else //проверка разоблачения шпиона игрока за СССР
            {
                if (shot > c.KGB)
                {
                    c.KGB--;
                    c.AddInfluence(Authority.Soviet, -2);  //обществу не нравиться когда в их стране орудуют чужие шпионы
                    c.AddState(CountryScript.States.SYM_SPY, Authority.Soviet, 3);
                }
            }
        }

        return true;
    }

    void Revolution(CountryScript Country)
    {
        Authority NewAut = 0;

        switch (Country.Authority)
        {
            case Authority.Neutral:
                //В нейтральной стране побеждает тот, у кого было меньше влияния
                if (Country.AmInf < Country.SovInf)
                    NewAut = Authority.Amer;
                else
                    NewAut = Authority.Soviet;
                break;
            case Authority.Amer:
                NewAut = Authority.Soviet;
                break;
            case Authority.Soviet:
                NewAut = Authority.Amer;
                break;
        }

        ChangeGovernment(Country, NewAut, true);
    }

    //Смена власти
    //revolution = true - в результате революции, false - мирным способом
    public void ChangeGovernment(CountryScript Country, Authority NewAut, bool revolution = false, bool checkAbility = true)
    {
        if (!revolution && (checkAbility && !Country.CanChangeGov(NewAut)))
            return;

        //Почистить ролики
        VQueue.ClearVideoQueue(Country, VideoQueue.V_PUPPER_REVOLUTION);

        Country.ChangeGov(NewAut);
        SoundManager.SM.PlaySound("sound/cuop");
        VQueue.AddRolex(VQueue.LocalType(Country.Authority), VideoQueue.V_PRIO_NULL, revolution ? VideoQueue.V_PUPPER_WAR : VideoQueue.V_PUPPER_PEACE, Country);

        //Steam achievments
        //Ачивка за дипломатическую смену власти (в любой стране)
        if (Country.Authority == Player.Authority && !revolution && CurrentMonth < 120) //до 1960 года
            SteamManager.UnLockAchievment("Coup a country diplomatically");

        //Если в главной стране правительство сменилось, тогда победа нокаутом
        if (Player.MyCountry == Country || Player.OppCountry == Country)
        {
            //Steam achievments
            //Ачивка связанная с переворотом в стране противника (мирным или вооруженным)
            if (Player.OppCountry.Authority == Player.Authority && CurrentMonth < 504) //до 1992 года
                SteamManager.UnLockAchievment("Coup your main enemy");

            StopGame();
        }
    }

    //обработка нажатия кнопки "NewGovButton"
    public void NewGovernment()
    {
        if (Country.CanChangeGov(Player.Authority))
        {
            ChangeGovernment(Country, Player.Authority, false);
            SnapToCountry();
        }
    }

    //Возвращает tru в случае победы
    public bool CheckGameResult()
    {
        //Проверка победы ядерной доминацией
        if (DLC_Armageddon.GetWinner(SettingsScript.Settings.AIPower) == Player)
            return true;
        else if (DLC_Armageddon.GetWinner(SettingsScript.Settings.AIPower) == AI.AIPlayer)
            return false;

        //Проверка победы нокаутом
        if (Player.MyCountry.Authority != Player.Authority)
            return false;

        if(Player.OppCountry.Authority == Player.Authority)
            return true;

        //Если очки равны, проверяем по бюджету
        if (Player.Score == GetOpponentTo(Player).Score)
            return (Player.Budget > GetOpponentTo(Player).Budget);

        //проверка выигрыша по счёту
        return (Player.Score > GetOpponentTo(Player).Score);
    }

    //Ежемесячное обновление информации
    void NextMonth()
    {
        mMonthCount++;
        if (mMonthCount == 0) return;   //первый месяц не считаем

        GameObject Countries = GameObject.Find("Countries");
        CountryScript Country;
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            Country = Countries.transform.GetChild(idx).GetComponent<CountryScript>();

            Country.NextMonth();

            //Боевые действия
            if (Country.OppForce > 0)
            {
                //проценты на победу
                PlayerScript usaPlayer = GetPlayerByAuthority(Authority.Amer);
                PlayerScript suPlayer = GetPlayerByAuthority(Authority.Soviet);
                int usaFP = Country.Air * usaPlayer.FirePower(OutlayField.air) + Country.Ground * usaPlayer.FirePower(OutlayField.ground) + Country.Sea * usaPlayer.FirePower(OutlayField.sea);
                int suFP = Country.Air * suPlayer.FirePower(OutlayField.air) + Country.Ground * suPlayer.FirePower(OutlayField.ground) + Country.Sea * suPlayer.FirePower(OutlayField.sea);
                int usaPercent = Mathf.RoundToInt(100f * usaFP / (usaFP + suFP));
                int suPercent = 100 - Mathf.RoundToInt(100f * usaFP / (usaFP + suFP));
                int usaScore = Random.Range(0, usaPercent);
                int suScore = Random.Range(0, suPercent);

                if (usaScore != suScore) //если равны - никто не погиб
                {
                    if (Country.GovForce > 0)
                    {
                        if ((Country.Authority == Authority.Amer && usaScore > suScore) 
                        || (Country.Authority == Authority.Soviet && usaScore < suScore) 
                        || (Country.Authority == Authority.Neutral && ((Country.SovInf > Country.AmInf && suScore > usaScore) || (Country.SovInf <= Country.AmInf && suScore < usaScore))))
                            Country.OppForce--;
                        else
                            Country.GovForce--;
                    }

                    if (Country.GovForce == 0)  //революция
                    {
                        SoundManager.SM.PlaySound("sound/thecall");
                        Revolution(Country);
                    }
                    else
                    {
                        VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_REVOLUTION, Country);
                    }
                }
            }

            // разборки шпионов раз в год
            TestSpyCombat(Country);

            Country.TestStates();
        }
        //Ход AI
        if(AI != null)
            AI.AIturn();

        Player.NewMonth();

        //Случайные события
        TestRandomEvent();
        
        //события по подписке (обновление выводимой информации)
        if (monthSubscribers != null)
            monthSubscribers();
    }

    //Ежегодное обновление информации
    void NewYear()
    {
        GameObject.Find("AmerPlayer").GetComponent<PlayerScript>().NewYear();
        GameObject.Find("SovPlayer").GetComponent<PlayerScript>().NewYear();
    }

    //Окончание игры и показ окна, говорящего об этом.
    void StopGame()
    {
        string SceneName = "";

        if (CheckGameResult())
        {
            SceneName = "WinScreen";

            //Steam achievments
            //Ачивка за первую победу
            SteamManager.UnLockAchievment("Win the game");
        }
        else
            SceneName = "LostScreen";

        //проверка выполнения миссий
        if (AI != null && SceneName == "WinScreen")
        {
            //Игра за СССР
            if (Player.Authority == Authority.Soviet)
            {
                //Проверка победы в космической гонке
                if (!SavedSettings.Mission1SU)
                {
                    bool WinSR = true;
                    for (int i = 1; i < SpaceRace.TechCount; i++)
                    {
                        if (!Player.GetTechStatus(i))
                        {
                            WinSR = false;
                            break;
                        }
                    }
                    SavedSettings.Mission1SU = WinSR;
                }
                //Проверка победы с 50 очками или более
                if (!SavedSettings.Mission2SU)
                {
                    SavedSettings.Mission2SU = (Player.Score >= 50 && SettingsScript.Settings.AIPower == 1);
                }
                //Проверка победы с переворотом в стране оппонента
                if (!SavedSettings.Mission3SU)
                {
                    if(Player.OppCountry.Authority == Player.Authority && SettingsScript.Settings.AIPower == 2)
                        {
                        SavedSettings.Mission3SU = true;
                        //Steam achievments
                        //Ачивка за выполнение вссех миссий за СССР
                        SteamManager.UnLockAchievment("Soviet glory");
                    }
                }
            }

            //Игра за США
            if (Player.Authority == Authority.Amer)
            {
                //Проверка победы в космической гонке
                if (!SavedSettings.Mission1USA)
                {
                    bool WinSR = true;
                    for (int i = 1; i < SpaceRace.TechCount; i++)
                    {
                        if (!Player.GetTechStatus(i))
                        {
                            WinSR = false;
                            break;
                        }
                    }
                    SavedSettings.Mission1USA = WinSR;
                }
                //Проверка победы с 50 очками или более
                if (!SavedSettings.Mission2USA)
                {
                    SavedSettings.Mission2USA = (Player.Score >= 50 && SettingsScript.Settings.AIPower == 1);
                }
                //Проверка победы с переворотом в стране оппонента
                if (!SavedSettings.Mission3USA)
                {
                    if (Player.OppCountry.Authority == Player.Authority && SettingsScript.Settings.AIPower == 2)
                    {
                        SavedSettings.Mission3USA = true;
                        //Steam achievments
                        //Ачивка за выполнение вссех миссий за США
                        SteamManager.UnLockAchievment("American dream");
                    }
                }
            }
        }

        LoadScene(SceneName);
    }

    // эмуляция схватки шпионов в стране раз в год.
    public void TestSpyCombat(CountryScript c)
    {
        if (Random.Range(0, 12) != 0) return;

        // случайно раз в год схватки шпионов:
        int r = Random.Range(1, 100);
        if (c.KGB == 0 || c.CIA == 0 || r < 34) return;

        // Если шпион погибает, то influence страны, к которой принадлежал шпион, 
        // понижается на 2% ( обществу не нравится когда в их стране орудуют чужие шпионы ). 
        if (r < 67)
        {
            c.KGB--;
            c.AddInfluence(Authority.Soviet, -2);
            
        }
        else
        {
            c.CIA--;
            c.AddInfluence(Authority.Amer, -2);
        }

        c.AddState(CountryScript.States.SYM_SPY, Authority.Amer, 3);
    }

    // проверка случайного события раз в год
    void TestRandomEvent()
    {
        if (Random.Range(0, 12) != 1)
            return;

        //Выбор страны, в которой произойдёт случайное событие
        Transform TCountries = GameObject.Find("Countries").transform;
        int cn = Random.Range(0, TCountries.childCount - 1);
        CountryScript c = TCountries.GetChild(cn).GetComponent<CountryScript>();
        
        int n = Random.Range(0, 7);
        switch (n)
        {
            case 0: // Наводнение ( повышается оппозиция + 10 )
                c.Support -= 10f;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_FLOOD, c);
                break;

            case 1: // Индустриализация ( повышается value страны + 1 )
                c.Score++;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_INDUSTR, c);
                break;

            case 2: // Нобелевский лауреат  ( повышается support на +20 )
                c.Support += 20f;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_NOBEL, c);
                break;

            case 3: // Финансовый кризис ( повышается оппозиция на + 50 )
                c.Support -= 50f;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_FINANCE, c);
                break;

            case 4: // Политический кризис ( повышается оппозиция на +25 )
                c.Support -= 25f;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_POLITIC, c);
                break;

            case 5: // Национализм ( повышается нейтральность на + 30 )
                if (c.NInf >= 99) return;
                c.AddInfluence(Authority.Neutral, 30);
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_NAZI, c);
                break;

            case 6: // Коммунистическое движение ( советский influence + 30 )
                c.AddInfluence(Authority.Soviet, 30);
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_COMMI, c);
                break;

            case 7: // Демократическое движение ( американский influence + 30 )
                c.AddInfluence(Authority.Amer, 30);
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_DEMOCR, c);
                break;
        }
    }
    
    //Обновление информации в верхнем меню
    public void ShowHighWinInfo()
    {
        string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        int m = mMonthCount % 12;
        int y = mMonthCount / 12;
        string CurrentDate = months[m] + " " + (1950 + y);

        PlayerScript amerPlayer = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>();
        PlayerScript sovPlayer = GameObject.Find("SovPlayer").GetComponent<PlayerScript>();

        UpMenu.Find("Date").GetComponent<Text>().text = CurrentDate;
        UpMenu.Find("USScore").GetComponent<Text>().text = amerPlayer.Score.ToString("f0");
        UpMenu.Find("USBudget").GetComponent<Text>().text = amerPlayer.Budget.ToString("f0");
        UpMenu.Find("SovScore").GetComponent<Text>().text = sovPlayer.Score.ToString("f0");
        UpMenu.Find("SovBudget").GetComponent<Text>().text = sovPlayer.Budget.ToString("f0");
        UpMenu.Find("AmPP").GetComponent<Text>().text = amerPlayer.PoliticalPoints.ToString("f0");
        UpMenu.Find("SovPP").GetComponent<Text>().text = sovPlayer.PoliticalPoints.ToString("f0");

        //Панели лидеров (внизу)
        int usaInf = 0, ussrInf = 0;
        foreach (CountryScript c in CountryScript.Countries())
        {
            usaInf += c.AmInf;
            ussrInf += c.SovInf;
        }

        LeaderPanelUSA.SetSuit((int)amerPlayer.PlayerLeader.ActualLeaderType);
        LeaderPanelUSA.SetHead(amerPlayer.PlayerLeader.LeaderID - 1);   //лидеры нумеруются с 1
        LeaderPanelUSA.SetName(amerPlayer.PlayerLeader.GetLeaderName(amerPlayer));
        LeaderPanelUSA.SetInfPercent(Mathf.RoundToInt(100f * usaInf/(usaInf + ussrInf)));
        LeaderPanelUSA.SetFPBonus(amerPlayer.GetInfFPbonus());
        LeaderPanelUSA.SetInfValue(usaInf);

        LeaderPanelUSSR.SetSuit((int)sovPlayer.PlayerLeader.ActualLeaderType);
        LeaderPanelUSSR.SetHead(sovPlayer.PlayerLeader.LeaderID - 1);   //лидеры нумеруются с 1
        LeaderPanelUSSR.SetName(sovPlayer.PlayerLeader.GetLeaderName(sovPlayer));
        LeaderPanelUSSR.SetInfPercent(Mathf.RoundToInt(100f * ussrInf / (usaInf + ussrInf)));
        LeaderPanelUSSR.SetFPBonus(sovPlayer.GetInfFPbonus());
        LeaderPanelUSSR.SetInfValue(ussrInf);

        if (SettingsScript.Settings.ArmageddonAvailable)
        {
            DLC_Armageddon.SetNuclearPercent(Player.Authority, Player.RelativeNuclearPower());
            DLC_Armageddon.SetNuclearPercent(AI.AIPlayer.Authority, AI.AIPlayer.RelativeNuclearPower());
        }

#if DEBUG
        //UpMenu.Find("testLeaderNameUSA").GetComponent<Text>().text = amerPlayer.PlayerLeader.GetLeaderName(amerPlayer);
        //UpMenu.Find("testLeaderTypeUSA").GetComponent<Text>().text = amerPlayer.PlayerLeader.GetLeaderTypeName();
        //UpMenu.Find("testResourcesUSA").GetComponent<Text>().text = string.Format("M:{0} S:{1} D:{2}", amerPlayer.MilitaryPool.ToString(), amerPlayer.SpyPool.ToString(), amerPlayer.DiplomatPool.ToString());
        //UpMenu.Find("testLeaderTypeUSSR").GetComponent<Text>().text = sovPlayer.PlayerLeader.GetLeaderTypeName();
        //UpMenu.Find("testLeaderNameUSSR").GetComponent<Text>().text = sovPlayer.PlayerLeader.GetLeaderName(sovPlayer);
        //UpMenu.Find("testResourcesUSSR").GetComponent<Text>().text = string.Format("M:{0} S:{1} D:{2}", sovPlayer.MilitaryPool.ToString(), sovPlayer.SpyPool.ToString(), sovPlayer.DiplomatPool.ToString());
        //UpMenu.Find("testInfUSSR").GetComponent<Text>().text = string.Format("Sov Inf: {0}", ussrInf.ToString());
        //UpMenu.Find("testInfUSA").GetComponent<Text>().text = string.Format("Amer Inf: {0}", usaInf.ToString());
#endif
    }

    public bool PayCost(Authority Aut, int Money)
    {
        if(Aut == Authority.Neutral)
            return false;

        PlayerScript Player = null;

        switch (Aut)
        {
            case Authority.Amer:
                Player = transform.FindChild("AmerPlayer").GetComponent<PlayerScript>();
                break;
            case Authority.Soviet:
                Player = transform.FindChild("SovPlayer").GetComponent<PlayerScript>();
                break;
        }

        if (Player.Budget - Money < MIN_BUDGET)
            return false;

        Player.Budget -= Money;
        ShowHighWinInfo();

        return true;
    }

    public bool PayCost(PlayerScript Player, int Money)
    {
        if (Player.Budget - Money < MIN_BUDGET)
            return false;

        Player.Budget -= Money;
        ShowHighWinInfo();

        return true;
    }

    // текущая эпоха (тег видеоролика)
    public int GetCurrentEpoch()
    {
        if (mMonthCount <= 20 * 12)
            return 1;
        else
            return 2;
    }

    // принадлежит ли эпоха (тег видеоролика) текущей эпохе
    // 1- 1950-1970, 2- 1970-2000 0-не проверяем
    internal bool IsCurrentEpoch(int epoch)
    {
        return (epoch == 0 || epoch == GetCurrentEpoch());
    }

    public CountryScript FindCountryById(int Id)
    {
        GameObject GoCountry = GameObject.Find("C (" + Id.ToString() + ")");
        if (GoCountry != null)
            return GoCountry.GetComponent<CountryScript>();
        else
            return null;
    }

    public CountryScript FindCountryByName(string Name)
    {
        foreach (var item in GameObject.Find("Countries").GetComponentsInChildren<CountryScript>())
        {
            if (item.Name == Name)
                return item;
        }

        return null;
    }


    public int CurrentMonth
    {
        get { return mMonthCount; }
        set { mMonthCount = value; }
    }

    //Установка текста новости и страны в нижнем меню.
    public void SetInfo(string InfoText, string CountryName = "")
    {
        DownMenu.Find("Info").GetComponent<Text>().text = InfoText;
        DownMenu.Find("InfoCountry").GetComponent<Text>().text = CountryName;
    }

    // определить локальный тип видеоролика
    public int GetMySideVideoType(Authority aut)
    {
        return aut == Authority.Amer ? VideoQueue.V_TYPE_USA : VideoQueue.V_TYPE_USSR;
    }

    //Добавление флага на правую панель, где показываются флаги стран, в которых идёт война
    public void AddWarFlag(CountryScript Country)
    {
        //Если флаг этой страны уже есть в списке, второй раз не добавляем
        for (int i = 0; i < WarFlagsPanel.childCount; i++)
        {
            if (WarFlagsPanel.GetChild(i).GetComponent<FlagButton>().Country == Country)
                return;
        }

        RectTransform fb = Instantiate<RectTransform>(FlagButtonPrefab);
        fb.SetParent(WarFlagsPanel);
        //Новый флаг должен появиться вверху списка
        fb.SetAsFirstSibling();
        //Установка страны
        fb.GetComponent<FlagButton>().Country = Country;

        //Влияние на глобальные последствия
        GlobalEffects.GlobalEffectsManager.GeM.ChangeCountersOnWar();

        //проигрывание звука войны
        SoundManager.SM.TurnWarSound(true);
    }

    //Удаление флага из правой панели
    public void RemoveWarFlag(CountryScript Country)
    {
        for (int i = 0; i < WarFlagsPanel.childCount; i++)
        {
            if (WarFlagsPanel.GetChild(i).GetComponent<FlagButton>().Country == Country)
                Destroy(WarFlagsPanel.GetChild(i).gameObject);
        }

        //остановка звука войны, если нигде войны нет
        if (WarFlagsPanel.childCount == 0)
            SoundManager.SM.TurnWarSound(false);

    }

    //Определение оппонента
    public PlayerScript GetOpponentTo(PlayerScript pl)
    {
        PlayerScript AmP = transform.Find("AmerPlayer").GetComponent<PlayerScript>();
        PlayerScript SovP = transform.Find("SovPlayer").GetComponent<PlayerScript>();
        PlayerScript retValue = null;

        if (pl == AmP)
            retValue = SovP;
        if (pl == SovP)
            retValue = AmP;

        return retValue;
    }

    //Получить игрока по Authority
    public PlayerScript GetPlayerByAuthority(Authority aut)
    {
        PlayerScript AmP = transform.Find("AmerPlayer").GetComponent<PlayerScript>();
        PlayerScript SovP = transform.Find("SovPlayer").GetComponent<PlayerScript>();
        PlayerScript retValue = null;

        if (aut == AmP.Authority)
            retValue = AmP;
        if (aut == SovP.Authority)
            retValue = SovP;

        return retValue;
    }

    // повысить влияние в странах при открытии технологий
    //govType - в каких странах повышаем влияние (нейтральная -- глобально)
    //Aut - чьё влияние повышаем
    //proc - величина повышения
    public void AddInfluenceInCountries(Authority govType, Authority Aut, int proc)
    {
        GameObject Countries = GameObject.Find("Countries");
        CountryScript c;
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            c = Countries.transform.GetChild(idx).GetComponent<CountryScript>();
            if (c.Authority == govType || govType == Authority.Neutral)
            {
                c.AddInfluence(Aut, proc);
                //Если повысилось влияние страны, которая отображается в нижнем меню, обновляем отображение
                if (c == Country)
                    SnapToCountry();
            }
        }
    }

    public void SubscribeMonth(dlgtMonthTick mt)
    {
        monthSubscribers += mt;
    }

    public void UnsubscribeMonth(dlgtMonthTick mt)
    {
        monthSubscribers -= mt;
    }

    public void Load()
    {
        SaveManager.LoadGame(Player.Authority == Authority.Amer?"Amer":"Sov");
        ShowHighWinInfo();
        SnapToCountry();
        SRInstance.UpdateView();
    }

    public void Save()
    {
        SaveManager.SaveGame(Player.Authority == Authority.Amer ? "Amer" : "Sov");
    }

    public void ChangeSpeed(int dir)
    {
        curSpeedIndex += dir;
        curSpeedIndex = Mathf.Clamp(curSpeedIndex, 0, GameSpeedPrefs.Length - 1);
        
        Tick = GameSpeedPrefs[curSpeedIndex];
    }

    public void SetPause(int state)
    {
        if (state == -1)
            IsPaused = !IsPaused;
        if(state == 0)
            IsPaused = false;
        if (state == 1)
            IsPaused = true;

        PausePlate.SetActive(IsPaused);
    }

    public void DeferredStopGame(PlayerScript pl)
    {
        delayedStop = true;
        //Закрываем все меню
        foreach (var item in Menus)
        {
            item.SetActive(false);
        }

        //переключаемся к проигравшей стране
        SnapToCountry(pl.OppCountry);
        //и показываем ядерный взрыв
        DLC_Armageddon.StartNukeAnim(pl.OppCountry.Capital.position);
        Marker.SetActive(false);
        //отложенное завершение игры (чтобы показать анимацию взрыва)
        Invoke("StopGame", 7);
    }
}

public enum Region
{
    USSR = 1,
    USA,
    Europe,
    Asia,
    Other
}

public enum Authority
{
    Neutral,
    Amer,
    Soviet
}

