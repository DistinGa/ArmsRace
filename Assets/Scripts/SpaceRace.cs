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
    public Button LaunchButton;

    public Sprite sprLocInf;    //Плашка с надписью "increase alliance influence"
    public Sprite sprGlobInf;   //Плашка с надписью "increase global influence"

    public Sprite BlueIcon;
    public Sprite RedIcon;
    public Sprite GreenIcon;
    public Sprite BlueIconOp;
    public Sprite RedIconOp;
    public Sprite GreenIconOp;

    public const int TechCount = 41;
    public const int GndTechCount = 15; //количество наземных технологий

    [SerializeField]
    Technology[] Techs = new Technology[TechCount]; //элементов в массиве на 1 больше, чем технологий, чтобы была возможность нумеровать их с единицы (чтобы использовать исходный код с минимальными изменениями)

    public void OnEnable()
    {
        PaintTechButtons();
        transform.Find("Toggles/Toggle1").GetComponent<Toggle>().isOn = true;
        ShowTechInfo(1);
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
                    BackImage.sprite = BlueIconOp;  //технология открыта оппонентом
                else
                    BackImage.sprite = BlueIcon;
            }
            else
            {
                if (GM.Player.GetTechStatus(Techs[i].mPrevTechNumber))
                {//технология доступна
                    if (GM.GetOpponentTo(GM.Player).GetTechStatus(i))
                        BackImage.sprite = GreenIconOp;  //технология открыта оппонентом
                    else
                        BackImage.sprite = GreenIcon;
                }
                else
                {//технология недоступна
                    if (GM.GetOpponentTo(GM.Player).GetTechStatus(i))
                        BackImage.sprite = RedIconOp;  //технология открыта оппонентом
                    else
                        BackImage.sprite = RedIcon;
                }
            }
        }
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
            if(!GM.GetOpponentTo(GM.Player).GetTechStatus(ind))
                txtInf.text = "+" + Techs[ind].mLocalInfl_1.ToString() + "%";
            else
                txtInf.text = "+" + Techs[ind].mLocalInfl.ToString() + "%";
        }
        else
        {
            //Вертикальные ветки технологий
            InfluencePlate.sprite = sprGlobInf;
            //Если технология не открыта оппонентом, показываем прибавку влияния при первом открытии
            if(!GM.GetOpponentTo(GM.Player).GetTechStatus(ind))
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

        if (TechInd <= GndTechCount)
        {
            //Уменьшение стоимости технологий запуска на 2%
            for (int i = GndTechCount + 1; i < TechCount; i++)
            {
                Techs[i].mCost -= Mathf.RoundToInt(Techs[i].mCost * 0.02f);
            }

            if (TechInd < GndTechCount) //Увеличиваем счётчик, если изучили не последнюю наземную технологию.
                Player.CurGndTechIndex++;
        }
        else
        {
            //Здесь будет хитрый способ выбора следующей технологии.
            Player.CurLnchTechIndex++;
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

        //Отметка открытой технологии
        Player.SetTechStatus(TechInd);

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
        if (TechInd == GndTechCount)    //была запущена последняя тенология в линии
            nextCost = 0;
        if (TechInd > GndTechCount && TechInd < TechCount)
            nextCost = Techs[Player.CurLnchTechIndex].mCost;
        if (TechInd == TechCount)    //была запущена последняя тенология в линии
            nextCost = 0;

        return nextCost;
    }

    //Возвращает номер предыдущей технологии
    public int GetPrevTechNumber(int ind)
    {
        return Techs[ind].mPrevTechNumber;
    }

    //стоимость изучения технологии
    public int GetTechCost(OutlayField field, int indx)
    {
        int res = 0;

        switch (field)
        {
            case OutlayField.spaceLaunches:
                res = Techs[indx].mCost;
                break;
            case OutlayField.spaceGround:
                res = Techs[indx].mCost;
                break;
            default:
                break;
        }

        return res;
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
