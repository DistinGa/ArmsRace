using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MilDepMenuScript : MonoBehaviour
{
    public Image Image;
    public Text Description;
    public Text FirePower;
    public Text Price;

    public Sprite BlueAirIcon;
    public Sprite RedAirIcon;
    public Sprite GreenAirIcon;
    public Sprite BlueAirIconOp;
    public Sprite RedAirIconOp;
    public Sprite GreenAirIconOp;
    public Sprite BlueGndIcon;
    public Sprite RedGndIcon;
    public Sprite GreenGndIcon;
    public Sprite BlueGndIconOp;
    public Sprite RedGndIconOp;
    public Sprite GreenGndIconOp;
    public Sprite BlueSeaIcon;
    public Sprite RedSeaIcon;
    public Sprite GreenSeaIcon;
    public Sprite BlueSeaIconOp;
    public Sprite RedSeaIconOp;
    public Sprite GreenSeaIconOp;
    public Sprite BlueRocketIcon;
    public Sprite RedRocketIcon;
    public Sprite GreenRocketIcon;
    public Sprite BlueRocketIconOp;
    public Sprite RedRocketIconOp;
    public Sprite GreenRocketIconOp;

    public const int TechCount = 11;

    [SerializeField]
    MilTechnology[] airTechs = new MilTechnology[TechCount];
    [SerializeField]
    MilTechnology[] gndTechs = new MilTechnology[TechCount];
    [SerializeField]
    MilTechnology[] seaTechs = new MilTechnology[TechCount];
    [SerializeField]
    MilTechnology[] rocketTechs = new MilTechnology[TechCount];

    public void OnEnable()
    {
        GameManagerScript.GM.SubscribeMonth(new dlgtMonthTick(UpdateView));
        UpdateView();

        transform.Find("Panel/Raw1/ToggleGroup/Toggle0").GetComponent<Toggle>().isOn = true;
        transform.Find("Panel/Raw2/ToggleGroup/Toggle0").GetComponent<Toggle>().isOn = true;
        transform.Find("Panel/Raw3/ToggleGroup/Toggle0").GetComponent<Toggle>().isOn = true;
        if (SettingsScript.Settings)
        {
            transform.Find("Panel/Raw4/ToggleGroup/Toggle0").GetComponent<Toggle>().isOn = true;
        }
    }

    public void OnDisable()
    {
        GameManagerScript.GM.UnsubscribeMonth(new dlgtMonthTick(UpdateView));
    }

    void UpdateView()
    {
        PaintTechButtons(OutlayField.air);
        PaintTechButtons(OutlayField.ground);
        PaintTechButtons(OutlayField.sea);
        if (SettingsScript.Settings)
            PaintTechButtons(OutlayField.rocket);
    }

    public int GetTechCost(OutlayField field, int indx, PlayerScript pl)
    {
        float res = 0;

        switch (field)
        {
            case OutlayField.air:
                res = airTechs[indx].mCost;
                break;
            case OutlayField.ground:
                res = gndTechs[indx].mCost;
                break;
            case OutlayField.sea:
                res = seaTechs[indx].mCost;
                break;
            case OutlayField.rocket:
                res = rocketTechs[indx].mCost;
                break;
            default:
                break;
        }

        //бонус типа лидера (пиджак)
        res -= res * pl.PlayerLeader.GetMilTechDiscount();

        return Mathf.RoundToInt(res);
    }

    public int GetTechPower(OutlayField field, int indx)
    {
        int res = 0;

        switch (field)
        {
            case OutlayField.air:
                res = airTechs[indx].Power;
                break;
            case OutlayField.ground:
                res = gndTechs[indx].Power;
                break;
            case OutlayField.sea:
                res = seaTechs[indx].Power;
                break;
            case OutlayField.rocket:
                res = rocketTechs[indx].Power;
                break;
            default:
                break;
        }

        return res;
    }

    //Установка соответствующих спрайтов на кнопки технологий
    private void PaintTechButtons(OutlayField field)
    {
        Image BackImage;
        GameManagerScript GM = GameManagerScript.GM;
        Sprite BlueIcon, BlueIconOp, RedIcon, RedIconOp, GreenIcon, GreenIconOp;
        string initPath;

        switch (field)
        {
            case OutlayField.air:
                initPath = "Panel/Raw1/ToggleGroup/Toggle";

                BlueIcon = BlueAirIcon;
                BlueIconOp = BlueAirIconOp;
                RedIcon = RedAirIcon;
                RedIconOp = RedAirIconOp;
                GreenIcon = GreenAirIcon;
                GreenIconOp = GreenAirIconOp;
                break;
            case OutlayField.ground:
                initPath = "Panel/Raw2/ToggleGroup/Toggle";

                BlueIcon = BlueGndIcon;
                BlueIconOp = BlueGndIconOp;
                RedIcon = RedGndIcon;
                RedIconOp = RedGndIconOp;
                GreenIcon = GreenGndIcon;
                GreenIconOp = GreenGndIconOp;
                break;
            case OutlayField.sea:
                initPath = "Panel/Raw3/ToggleGroup/Toggle";

                BlueIcon = BlueSeaIcon;
                BlueIconOp = BlueSeaIconOp;
                RedIcon = RedSeaIcon;
                RedIconOp = RedSeaIconOp;
                GreenIcon = GreenSeaIcon;
                GreenIconOp = GreenSeaIconOp;
                break;
            case OutlayField.rocket:
                initPath = "Panel/Raw4/ToggleGroup/Toggle";

                BlueIcon = BlueRocketIcon;
                BlueIconOp = BlueRocketIconOp;
                RedIcon = RedRocketIcon;
                RedIconOp = RedRocketIconOp;
                GreenIcon = GreenRocketIcon;
                GreenIconOp = GreenRocketIconOp;
                break;
            default:
                initPath = "";

                BlueIcon = null;
                BlueIconOp = null;
                RedIcon = null;
                RedIconOp = null;
                GreenIcon = null;
                GreenIconOp = null;
                break;
        }

        for (int i = 0; i < TechCount; i++)
        {
            BackImage = transform.Find(initPath + i.ToString() + "/Background").GetComponent<Image>();
            if (GM.Player.GetMilTechStatus(field, i))
            {//технология открыта
                if (GM.GetOpponentTo(GM.Player).GetMilTechStatus(field, i))
                    BackImage.sprite = BlueIconOp;  //технология открыта оппонентом
                else
                    BackImage.sprite = BlueIcon;
            }
            else
            {
                if (GM.Player.GetMilTechStatus(field, GetPrevTechNumber(i)))
                {//технология доступна
                    if (GM.GetOpponentTo(GM.Player).GetMilTechStatus(field, i))
                        BackImage.sprite = GreenIconOp;  //технология открыта оппонентом
                    else
                        BackImage.sprite = GreenIcon;
                }
                else
                {//технология недоступна
                    if (GM.GetOpponentTo(GM.Player).GetMilTechStatus(field, i))
                        BackImage.sprite = RedIconOp;  //технология открыта оппонентом
                    else
                        BackImage.sprite = RedIcon;
                }
            }
        }
    }

    //Заполнение отображаемых полей при нажатии кнопки технологии
    public void ShowAirTechInfo(int ind)
    {
        ShowTechInfo(OutlayField.air, ind);
    }

    public void ShowGndTechInfo(int ind)
    {
        ShowTechInfo(OutlayField.ground, ind);
    }

    public void ShowSeaTechInfo(int ind)
    {
        ShowTechInfo(OutlayField.sea, ind);
    }

    public void ShowRocketTechInfo(int ind)
    {
        ShowTechInfo(OutlayField.rocket, ind);
    }

    void ShowTechInfo(OutlayField fld, int ind)
    {
        MilTechnology[] milTech;
        GameManagerScript GM = GameManagerScript.GM;

        switch (fld)
        {
            case OutlayField.air:
                milTech = airTechs;
                break;
            case OutlayField.ground:
                milTech = gndTechs;
                break;
            case OutlayField.sea:
                milTech = seaTechs;
                break;
            case OutlayField.rocket:
                milTech = rocketTechs;
                break;
            default:
                return;
                break;
        }

        if (GM.Player.Authority == Authority.Amer)
        {
            Description.text = milTech[ind].mUsaDescr;
            Image.sprite = milTech[ind].mUsaSprite;
        }
        else
        {
            Description.text = milTech[ind].mRusDescr;
            Image.sprite = milTech[ind].mRusSprite;
        }

        Price.text = "$" + GetTechCost(fld, ind, GM.Player).ToString();
        if(fld == OutlayField.rocket)
            FirePower.text = milTech[ind].Power.ToString();
        else
            FirePower.text = GM.FirePowerPerTech.ToString();
    }

    //Возвращает номер предыдущей технологии
    public int GetPrevTechNumber(int ind)
    {
        int prevNum = ind - 1;

        if (prevNum < 0)
            prevNum = 0;

        return prevNum;
    }
}

[System.Serializable]
class MilTechnology
{
    public Sprite mUsaSprite = null, mRusSprite = null;
    [TextArea(2, 7)]
    public string mUsaDescr = "", mRusDescr = ""; // текстовое описание
    public int mCost = 0;   // стоимость технологии
    public int Power = 5;   //мощь иехнологии (используется только для ядерных технологий)
}

