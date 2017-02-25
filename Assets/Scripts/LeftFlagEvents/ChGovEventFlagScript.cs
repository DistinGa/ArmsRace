using UnityEngine;
using System.Collections;
using System;
using LeftFlagEvents;

public class ChGovEventFlagScript : BaseEventFlagScript
{
    CountryScript country;

    public CountryScript Country
    {
        set
        {
            country = value;
            GetComponent<UnityEngine.UI.Image>().sprite = (country.Authority == Authority.Soviet ? country.FlagS : country.FlagNs);
        }

        get
        {
            return country;
        }
    }

    public override void OnClickEvent()
    {
        GameManagerScript.GM.SnapToCountry(country);
    }

    //Проверка условия для отображения флага
    public override bool TestSelfCondition()
    {
        return (country.CanChangeGov(GameManagerScript.GM.Player.Authority));
    }
}
