using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpaceRace : MonoBehaviour
{
    public Image Image;
    public Text Description;
    public Text txtInf;
    public Text Price;
    public Image InfluencePlate;
    public GameObject LaunchesPanel;
    public GameObject GroundPanel;

    public Sprite sprLocInf;    //Плашка с надписью "increase alliance influence"
    public Sprite sprGlobInf;   //Плашка с надписью "increase global influence"

    public Sprite BlueIcon;
    public Sprite RedIcon;
    public Sprite GreenIcon;
    public Sprite BlueIconOp;
    public Sprite RedIconOp;
    public Sprite GreenIconOp;

    public Sprite BlueIconGnd;
    public Sprite RedIconGnd;
    public Sprite GreenIconGnd;
    public Sprite BlueIconGndOp;
    public Sprite RedIconGndOp;
    public Sprite GreenIconGndOp;

    public const int TechCount = 41;
    public const int GndTechCount = 15; //количество наземных технологий

    public RectTransform MoonSwithButton;
    public Sprite MoonSwith_on, MoonSwith_off;

    [SerializeField]
    Technology[] Techs = new Technology[TechCount]; //элементов в массиве на 1 больше, чем технологий, чтобы была возможность нумеровать их с единицы (чтобы использовать исходный код с минимальными изменениями)

    public void OnEnable()
    {
        GameManagerScript.GM.SubscribeMonth(new dlgtMonthTick(UpdateView));
        UpdateView();
    }

    public void OnDisable()
    {
        GameManagerScript.GM.UnsubscribeMonth(new dlgtMonthTick(UpdateView));
    }

    public void UpdateView()
    {
        PaintTechButtons();
        LaunchesPanel.SetActive(GameManagerScript.GM.Player.GetTechStatus(1) && GameManagerScript.GM.Player.Outlays[OutlayField.spaceLaunches].Cost != 0);
        GroundPanel.SetActive(GameManagerScript.GM.Player.Outlays[OutlayField.spaceGround].Cost != 0);
        //Обновление информации о "нажатом" элементе
        Transform tgr = transform.Find("Toggles");
        int i = 0;
        while (i < tgr.childCount)
        {
            if (tgr.GetChild(i).GetComponent<Toggle>().isOn)
                break;

            i++;
        }

        if (i < tgr.childCount)
            showTechInfo(i + 1);
    }

    //Установка соответствующих спрайтов на кнопки технологий
    private void PaintTechButtons()
    {
        Image BackImage;
        GameManagerScript GM = GameManagerScript.GM;

        for (int i = 1; i < 41; i++)
        {
            BackImage = transform.Find("Toggles/Toggle" + i.ToString() + "/Background").GetComponent<Image>();
            if (GM.Player.GetTechStatus(i))
            {//технология открыта
                if (GM.GetOpponentTo(GM.Player).GetTechStatus(i))
                    BackImage.sprite = i > 15 ? BlueIconOp : BlueIconGndOp;  //технология открыта оппонентом
                else
                    BackImage.sprite = i > 15 ? BlueIcon : BlueIconGnd;
            }
            else
            {
                if (GM.Player.CurGndTechIndex == i || GM.Player.CurLnchTechIndex == i)
                {//технология доступна (изучается)
                    if (GM.GetOpponentTo(GM.Player).GetTechStatus(i))
                        BackImage.sprite = i > 15 ? GreenIconOp : GreenIconGndOp;  //технология открыта оппонентом
                    else
                        BackImage.sprite = i > 15 ? GreenIcon : GreenIconGnd;
                }
                else
                {//технология недоступна
                    if (GM.GetOpponentTo(GM.Player).GetTechStatus(i))
                        BackImage.sprite = i > 15 ? RedIconOp : RedIconGndOp;  //технология открыта оппонентом
                    else
                        BackImage.sprite = i > 15 ? RedIcon : RedIconGnd;
                }
            }
        }

        //лунный переключатель
        MoonSwithButton.GetComponent<Image>().sprite = GM.Player.MoonSwitchState ? MoonSwith_on : MoonSwith_off;
    }

    //Заполнение отображаемых полей при нажатии кнопки технологии
    public void ShowTechInfo(int ind)
    {
        SoundManager.SM.PlaySound("sound/click2");
        showTechInfo(ind);
    }

    //Заполнение отображаемых полей (реальные действия)
    void showTechInfo(int ind)
    {
        GameManagerScript GM = GameManagerScript.GM;

        if (GM.Player.Authority == Authority.Amer)
        {
            Description.text = Techs[ind].mUsaDescr;
            Image.sprite = Techs[ind].mUsaSprite;
        }
        else
        {
            Description.text = Techs[ind].mRusDescr;
            Image.sprite = Techs[ind].mRusSprite;
        }

        Price.text = "$" + GetTechCost(ind, GM.Player).ToString();

        //Технологии из нижнего ряда добавляют влияние только в союзных странах, технологии из вертикальных веток - во всех странах.
        if (ind < 16)
        {
            //Нижний ряд
            InfluencePlate.sprite = sprLocInf;
            //Если технология не открыта оппонентом, показываем прибавку влияния при первом открытии
            if (!GM.GetOpponentTo(GM.Player).GetTechStatus(ind))
                txtInf.text = "+" + Techs[ind].mLocalInfl_1.ToString() + "%";
            else
                txtInf.text = "+" + Techs[ind].mLocalInfl.ToString() + "%";
        }
        else
        {
            //Вертикальные ветки технологий
            InfluencePlate.sprite = sprGlobInf;
            //Если технология не открыта оппонентом, показываем прибавку влияния при первом открытии
            if (!GM.GetOpponentTo(GM.Player).GetTechStatus(ind))
                txtInf.text = "+" + Techs[ind].mGlobalInfl_1.ToString() + "%";
            else
                txtInf.text = "+" + Techs[ind].mGlobalInfl.ToString() + "%";
        }
    }

    public void SetTechno(int TechInd, string mUsaDescr, string mRusDescr, int mCost, int mLocalInfl, int mGlobalInfl, int mLocalInfl_1, int mGlobalInfl_1)
    {
        Techs[TechInd].mUsaDescr = mUsaDescr;
        Techs[TechInd].mRusDescr = mRusDescr;
        Techs[TechInd].mCost = mCost;
        Techs[TechInd].mLocalInfl = mLocalInfl;
        Techs[TechInd].mGlobalInfl = mGlobalInfl;
        Techs[TechInd].mLocalInfl_1 = mLocalInfl_1;
        Techs[TechInd].mGlobalInfl_1 = mGlobalInfl_1;
    }

    public void SetTechno(int TechInd, string mUsaDescr, string mRusDescr, int mCost, int mLocalInfl, int mGlobalInfl, int mLocalInfl_1, int mGlobalInfl_1, Sprite mUsaSprite, Sprite mRusSprite)
    {
        Techs[TechInd].mUsaDescr = mUsaDescr;
        Techs[TechInd].mRusDescr = mRusDescr;
        Techs[TechInd].mCost = mCost;
        Techs[TechInd].mLocalInfl = mLocalInfl;
        Techs[TechInd].mGlobalInfl = mGlobalInfl;
        Techs[TechInd].mLocalInfl_1 = mLocalInfl_1;
        Techs[TechInd].mGlobalInfl_1 = mGlobalInfl_1;
        Techs[TechInd].mUsaSprite = mUsaSprite;
        Techs[TechInd].mRusSprite = mRusSprite;
    }

    public Technology GetTechno(int TechInd)
    {
        return (Techs[TechInd]);
    }

    //Запуск технологии
    //Player - игрок, запускающий технологию
    //TechInd - индекс технологии
    //Возвращаем стоимость следующей технологии или ноль, если в этой линии всё изучено.
    public int LaunchTech(PlayerScript Player, int TechInd)
    {
        if (TechInd == -1)
            return 0;

        GameManagerScript GM = GameManagerScript.GM;

        //Отметка открытой технологии
        Player.SetTechStatus(TechInd);

        if (TechInd > 0 && TechInd <= GndTechCount)
        {
            Player.CurGndTechIndex++;
            if(Player.Outlays[OutlayField.spaceLaunches].Cost != 0)
                Player.Outlays[OutlayField.spaceLaunches].SetNewCost(GetTechCost(Player.CurLnchTechIndex, Player)); //пересчёт стоимости изучаемой в данный момент технологии запусков с учётом скидки
        }
        else
        {
            //Хитрый способ выбора следующей технологии запусков.
            if (!Player.GetTechStatus(34) && Player.GetTechStatus(4) && Player.GetTechStatus(22))
            {
                Player.CurLnchTechIndex = 34;
            }
            else if (!Player.GetTechStatus(35) && Player.GetTechStatus(6) && Player.GetTechStatus(24) && Player.GetTechStatus(34))
            {
                Player.CurLnchTechIndex = 35;
            }
            else if (!Player.GetTechStatus(40) && Player.GetTechStatus(35) && (Player.MoonSwitchState || Player.GetTechStatus(39))) //высадка на Луну
            {
                //переключатель включен или это последняя неизученная технология в линии запусков.
                Player.CurLnchTechIndex = 40;
            }
            else if (!Player.GetTechStatus(40) && Player.GetTechStatus(35) && (!Player.MoonSwitchState && Player == GM.AI.AIPlayer && !GM.GetOpponentTo(Player).GetTechStatus(40))) //высадка на Луну
            {
                //переключатель выключен...
                //Для ИИ - изучаем высадку, если оппонент её ещё не изучил
                //Для игрока - не изучаем, пока не изучим все остальные технологии
                Player.CurLnchTechIndex = 40;
            }
            else if (!Player.GetTechStatus(36) && Player.GetTechStatus(10) && Player.GetTechStatus(28))
            {
                Player.CurLnchTechIndex = 36;
            }
            else if (!Player.GetTechStatus(37) && Player.GetTechStatus(12) && Player.GetTechStatus(30))
            {
                Player.CurLnchTechIndex = 37;
            }
            else if (!Player.GetTechStatus(38) && Player.GetTechStatus(15) && Player.GetTechStatus(33))
            {
                Player.CurLnchTechIndex = 38;
            }
            else if (!Player.GetTechStatus(39) && Player.GetTechStatus(38))
            {
                Player.CurLnchTechIndex = 39;
            }
            else if (Player.GetTechStatus(39) && Player.GetTechStatus(40))
            {
                //изучены все технологии
                Player.CurLnchTechIndex = -1;
            }
            else //Во всех остальных случаях изучаем следующую технологию. Если это 33-я, то ждём благоприятных условий.
            {
                Player.CurLnchTechIndex = 0;    //нулевая - технология ожидания, когда 33-я технология изучена, то для изучения "внутренних" технологий нужно дождаться изучения земных технологий
                //Находим первую неизученную тенологию на линии 22-33.
                for (int i = 16; i <= 33; i++)
                {
                    if (!Player.GetTechStatus(i))
                    {
                        Player.CurLnchTechIndex = i;
                        break;
                    }
                }
            }
        }

        int nextCost = 0;
        if (TechInd != Player.CurLnchTechIndex && TechInd != 0) //если они равны, значит не было переключения на новую технологию (внимание на технологии запусков, наземные технологии переключаются просто линейно)
        {
            int InfAmount = 0;
            //Если игрок первый, кто открывает данную технологию, бонус к влиянию повышенный
            if (GM.GetOpponentTo(Player).GetTechStatus(TechInd))
                InfAmount = Mathf.Max(Techs[TechInd].mLocalInfl, Techs[TechInd].mGlobalInfl);
            else
                InfAmount = Mathf.Max(Techs[TechInd].mLocalInfl_1, Techs[TechInd].mGlobalInfl_1);

            //Увеличение влияния в странах
            if (TechInd < 16)
                GM.AddInfluenceInCountries(Player.Authority, Player.Authority, InfAmount);  //увеличение в союзных странах
            else
                GM.AddInfluenceInCountries(Authority.Neutral, Player.Authority, InfAmount); //Во всех нейтральных странах

            //Steam achievments
            if (GM.Player == Player)
            {
                if (TechInd == 20 && GM.CurrentMonth < 144) //до 1962 года
                    SteamManager.UnLockAchievment("Man in Space");
                if (TechInd == 40 && GM.CurrentMonth < 240) //до 1970 года
                    SteamManager.UnLockAchievment("Land on the Moon");
                if (TechInd == 38 && GM.CurrentMonth < 444) //до 1987 года
                    SteamManager.UnLockAchievment("Space Station");
            }

            // показать видео
            GM.VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_NULL,
                                             VideoQueue.V_PUPPER_TECHNOLOW_START + TechInd - 1,
                                             Player.MyCountry);

            if (TechInd <= GndTechCount)
            {
                if (Player.GetTechStatus(GndTechCount))    //была изучена последняя технология в линии
                {
                    nextCost = 0;
                    Player.CurGndTechIndex = -1;    //сигнал о том, что все технологии в линии изучены
                }
                else
                    nextCost = GetTechCost(Player.CurGndTechIndex, Player);
            }

            if (TechInd > GndTechCount && TechInd < TechCount)
            {
                if (Player.CurLnchTechIndex <= 0)
                    nextCost = 0;
                else
                    nextCost = GetTechCost(Player.CurLnchTechIndex, Player);
            }
        }
        else //ожидание изучения технологий запусков или выход из этого ожидания (имеем дело только с технологиями запусков)
        {
            if(Player.CurLnchTechIndex > 0)
                nextCost = GetTechCost(Player.CurLnchTechIndex, Player);
        }

        return nextCost;
    }

    ////Возвращает номер предыдущей технологии
    //public int GetPrevTechNumber(int ind)
    //{
    //    return Techs[ind].mPrevTechNumber;
    //}

    //стоимость изучения технологии
    public int GetTechCost(int indx, PlayerScript pl)
    {
        int initCost = Techs[indx].mCost;
        int dscMulty = 0;
        float res = initCost;

        if (indx > GndTechCount)
        {
            //технологии запусков
            //учитываем скидки от изученных наземных технологий
            if (pl.CurGndTechIndex == -1)   //изучили все наземные технологии
                dscMulty = GndTechCount;
            else
                dscMulty = pl.CurGndTechIndex - 1;

            res -= initCost * (dscMulty * 0.01f);
        }

        //бонус лидера
        res -= initCost * pl.PlayerLeader.GetSpaceDiscount();
        //прочие бонусы (от глобальных последствий, например)
        res -= initCost * (pl.SpaceDiscount / 100f);

        return Mathf.RoundToInt(res);
    }

    public void MoonSwitchSet()
    {
        GameManagerScript.GM.Player.MoonSwitchState = !GameManagerScript.GM.Player.MoonSwitchState;
        MoonSwithButton.GetComponent<Image>().sprite = GameManagerScript.GM.Player.MoonSwitchState ? MoonSwith_on : MoonSwith_off;
    }
}

[System.Serializable]
public class Technology
{
    public int mPrevTechNumber = 0; // предыдущая технология
    public Sprite mUsaSprite = null, mRusSprite = null;
    [TextArea(2, 5)]
    public string mUsaDescr, mRusDescr; // текстовое описание
    public int mCost; // стоимость технологии
    public int mLocalInfl, mGlobalInfl, mLocalInfl_1, mGlobalInfl_1; // % прироста лок-глоб infl, и если открыли первыми
}
