using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MilDepMenuScript : MonoBehaviour {
    public Image Image;
    public Text Description;
    public Text FirePower;

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
        PaintTechButtons(OutlayField.air);
        PaintTechButtons(OutlayField.ground);
        PaintTechButtons(OutlayField.sea);
        //PaintTechButtons(OutlayField.rocket);

        transform.Find("Panel/Raw1/ToggleGroup/Toggle1").GetComponent<Toggle>().isOn = true;
        transform.Find("Panel/Raw2/ToggleGroup/Toggle1").GetComponent<Toggle>().isOn = true;
        transform.Find("Panel/Raw3/ToggleGroup/Toggle1").GetComponent<Toggle>().isOn = true;
        //transform.Find("Panel/Raw4/ToggleGroup/Toggle1").GetComponent<Toggle>().isOn = true;

        //ShowAirTechInfo(0);
        //ShowGndTechInfo(0);
        //ShowSeaTechInfo(0);
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
            BackImage = transform.Find(initPath + (i+1).ToString() + "/Background").GetComponent<Image>();
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
        --ind;  //индексы с нуля
        GameManagerScript GM = GameManagerScript.GM;

        if (GM.Player.Authority == Authority.Amer)
        {
            Description.text = airTechs[ind].mUsaDescr;
            Image.sprite = airTechs[ind].mUsaSprite;
        }
        else
        {
            Description.text = airTechs[ind].mRusDescr;
            Image.sprite = airTechs[ind].mRusSprite;
        }
    }

    public void ShowGndTechInfo(int ind)
    {
        --ind;  //индексы с нуля
        GameManagerScript GM = GameManagerScript.GM;

        if (GM.Player.Authority == Authority.Amer)
        {
            Description.text = gndTechs[ind].mUsaDescr;
            Image.sprite = gndTechs[ind].mUsaSprite;
        }
        else
        {
            Description.text = gndTechs[ind].mRusDescr;
            Image.sprite = gndTechs[ind].mRusSprite;
        }
    }

    public void ShowSeaTechInfo(int ind)
    {
        --ind;  //индексы с нуля
        GameManagerScript GM = GameManagerScript.GM;

        if (GM.Player.Authority == Authority.Amer)
        {
            Description.text = seaTechs[ind].mUsaDescr;
            Image.sprite = seaTechs[ind].mUsaSprite;
        }
        else
        {
            Description.text = seaTechs[ind].mRusDescr;
            Image.sprite = seaTechs[ind].mRusSprite;
        }
    }

    public void ShowRocketTechInfo(int ind)
    {
        --ind;  //индексы с нуля
        GameManagerScript GM = GameManagerScript.GM;

        if (GM.Player.Authority == Authority.Amer)
        {
            Description.text = rocketTechs[ind].mUsaDescr;
            Image.sprite = rocketTechs[ind].mUsaSprite;
        }
        else
        {
            Description.text = rocketTechs[ind].mRusDescr;
            Image.sprite = rocketTechs[ind].mRusSprite;
        }
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
    [TextArea(2, 5)]
    public string mUsaDescr, mRusDescr; // текстовое описание
    public int mCost; // стоимость технологии
    public int mFirePower = 10; // множитель огневой мощи
}

