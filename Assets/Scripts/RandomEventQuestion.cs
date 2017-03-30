using UnityEngine;
using UnityEngine.UI;

public class RandomEventQuestion : MonoBehaviour
{
    RandomEvent rEvent;
    RandomEventManager EventManager;

    public void Start()
    {
        EventManager = FindObjectOfType<RandomEventManager>();

    }

    public void Init(RandomEvent re)
    {
        rEvent = re;

        //string _color = "";
        //switch (rEvent.Country.Authority)
        //{
        //    case Authority.Neutral:
        //        _color = "yellow";
        //        break;
        //    case Authority.Amer:
        //        _color = "blue";
        //        break;
        //    case Authority.Soviet:
        //        _color = "red";
        //        break;
        //}
        //transform.Find("CountryName").GetComponent<Text>().text = "<color=" + _color + ">" + rEvent.Country.Name + "</color>";
        transform.Find("CountryName").GetComponent<Text>().text = rEvent.Country.Name;
        transform.Find("DescriptionPanel/Description").GetComponent<Text>().text = rEvent.Description;
        transform.Find("DescriptionPanel/Description2").GetComponent<Text>().text = rEvent.Description2;
        transform.Find("CostPanel/Cost").GetComponent<Text>().text = "$" + rEvent.Cost.ToString();
        transform.Find("Flag").GetComponent<Image>().sprite = rEvent.Country.Authority == Authority.Soviet ? rEvent.Country.FlagS : rEvent.Country.FlagNs;
    }

    public void SelectYes()
    {
        if(GameManagerScript.GM.PayCost(GameManagerScript.GM.Player, rEvent.Cost))
            rEvent.Action();

        GameManagerScript.GM.SnapToCountry(rEvent.Country);

        SelectNo();
    }

    public void SelectNo()
    {
        EventManager.CloseEvent(rEvent);
        GameManagerScript.GM.ToggleTechMenu(gameObject);
        rEvent.eventDurationDiscounter = 0;
        rEvent = null;
        FindObjectOfType<CameraScriptXZ>().setOverMenu = false;
    }
}
