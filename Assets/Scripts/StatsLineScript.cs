using UnityEngine;
using UnityEngine.UI;

public class StatsLineScript : MonoBehaviour {
    public RectTransform Influence;
    public RectTransform Opposition;
    public RectTransform Score;
    public RectTransform Button;

    public Sprite Blue;
    public Sprite Red;
    public Sprite Grey;

    public Authority ListAuthority;
    public CountryScript Country;

    static Color BlueText = new Color(0, 0.75f, 1);
    static Color RedText = new Color(1, 0.25f, 0);

    public void Init(CountryScript c)
    {
        Country = c;

        Image Image = GetComponent<Image>();

        Text InfText = Influence.GetComponent<Text>();
        InfText.text = (ListAuthority == Authority.Amer? c.AmInf: c.SovInf).ToString("f0");
        switch (c.LastAut)
        {
            case Authority.Neutral:
                InfText.color = Color.white;
                break;
            case Authority.Amer:
                InfText.color = BlueText;
                break;
            case Authority.Soviet:
                InfText.color = RedText;
                break;
        }

        Opposition.GetComponent<Text>().text = (100f - c.Support).ToString("f0");
        //Очки стран учитываем, только для своих стран
        if(ListAuthority == c.Authority)
            Score.GetComponent<Text>().text = (c.Score).ToString("f0");
        else
            Score.GetComponent<Text>().text = "";

        Button.GetComponent<Text>().text = c.Name;

        switch (c.Authority)
        {
            case Authority.Neutral:
                Image.sprite = Grey;
                break;
            case Authority.Amer:
                Image.sprite = Blue;
                break;
            case Authority.Soviet:
                Image.sprite = Red;
                break;
        }

        //gameObject.SetActive(true);
    }

    //Позиционирование на страну, выбранную в списке
    public void ToCountry()
    {
        GameManagerScript.GM.SnapToCountry(Country);
    }
}
