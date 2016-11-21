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
        transform.Find("Toggles/Toggle1").GetComponent<Toggle>().isOn = true;
        ShowTechInfo(1);
    }

    public void OnDisable()
    {
        GameManagerScript.GM.UnsubscribeMonth(new dlgtMonthTick(UpdateView));
    }

    public void UpdateView()
    {
        PaintTechButtons();
        LaunchesPanel.SetActive(GameManagerScript.GM.Player.GetTechStatus(1));
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
        GameManagerScript GM = GameManagerScript.GM;
        SoundManager.SM.PlaySound("sound/click2");

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

        Price.text = "$" + Techs[ind].mCost.ToString();

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

    //Запуск технологии
    //Player - игрок, запускающий технологию
    //TechInd - индекс технологии
    //Возвращаем стоимость следующей технологии или ноль, если в этой линии всё изучено.
    public int LaunchTech(PlayerScript Player, int TechInd)
    {
        GameManagerScript GM = GameManagerScript.GM;

        //Отметка открытой технологии
        Player.SetTechStatus(TechInd);

        if (TechInd <= GndTechCount)
        {
            //Уменьшение стоимости технологий запуска на 2%
            for (int i = GndTechCount + 1; i < TechCount; i++)
            {
                Techs[i].mCost = Mathf.RoundToInt(Techs[i].mCost * 0.98f);
            }
            Player.Outlays[OutlayField.spaceLaunches].SetNewCost(Mathf.RoundToInt(Player.Outlays[OutlayField.spaceLaunches].Cost *  0.98f)); //уменьшение стоимости изучаемой в данный момент технологии

            if (TechInd < GndTechCount) //Увеличиваем счётчик, если изучили не последнюю наземную технологию.
                Player.CurGndTechIndex++;
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
            else if (!Player.GetTechStatus(40) && Player.GetTechStatus(35)) //высадка на Луну
            {
                if (Player.MoonSwitchState) //переключатель включен - сразу изучаем высадку
                    Player.CurLnchTechIndex = 40;
                else    //переключатель выключен - изучаем высадку, если оппонент её ещё не изучил или, если это последняя неизученная технология в линии запусков.
                {
                    if (!GM.GetOpponentTo(Player).GetTechStatus(40) || Player.GetTechStatus(39))
                        Player.CurLnchTechIndex = 40;
                }
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
            else //Во всех остальных случаях изучаем следующую технологию. Если это 33-я, то ждём благоприятных условий.
            {
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

        ////Steam achievments
        //if (GM.Player == Player)
        //{
        //    if (TechInd == 19)
        //        SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_0");
        //    if (TechInd == 25)
        //        SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_1");
        //    if (TechInd == 39)
        //        SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_4");
        //}

        // показать видео
        GM.VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_NULL,
                                         VideoQueue.V_PUPPER_TECHNOLOW_START + TechInd - 1,
                                         Player.MyCountry);

        int nextCost = 0;
        if (TechInd < GndTechCount)
            nextCost = Techs[Player.CurGndTechIndex].mCost;
        if (Player.GetTechStatus(GndTechCount))    //была запущена последняя технология в линии
        {
            nextCost = 0;
            Player.CurGndTechIndex = -1;    //сигнал о том, что все технологии в линии изучены
        }
        if (TechInd != Player.CurLnchTechIndex) //если они равны, значит не было переключения на новую технологию (переключаются только технологии запусков)
        {
            if (TechInd > GndTechCount && TechInd < TechCount)
                nextCost = Techs[Player.CurLnchTechIndex].mCost;
        }
        else
        {
            nextCost = 0;
            if (Player.GetTechStatus(TechCount - 1) && Player.GetTechStatus(TechCount - 2))    //все технологии запусков изучены
            {
                Player.CurLnchTechIndex = -1;    //сигнал о том, что все технологии в линии изучены
            }
        }

        return nextCost;
    }

    ////Возвращает номер предыдущей технологии
    //public int GetPrevTechNumber(int ind)
    //{
    //    return Techs[ind].mPrevTechNumber;
    //}

    //стоимость изучения технологии
    public int GetTechCost(OutlayField field, int indx)
    {
        return Techs[indx].mCost;
    }

    public void MoonSwitchSet()
    {
        GameManagerScript.GM.Player.MoonSwitchState = !GameManagerScript.GM.Player.MoonSwitchState;
        MoonSwithButton.GetComponent<Image>().sprite = GameManagerScript.GM.Player.MoonSwitchState ? MoonSwith_on : MoonSwith_off;
    }
}

[System.Serializable]
class Technology
{
    public int mPrevTechNumber = 0; // предыдущая технология
    public Sprite mUsaSprite = null, mRusSprite = null;
    [TextArea(2, 5)]
    public string mUsaDescr, mRusDescr; // текстовое описание
    public int mCost; // стоимость технологии
    public int mLocalInfl, mGlobalInfl, mLocalInfl_1, mGlobalInfl_1; // % прироста лок-глоб infl, и если открыли первыми
}
