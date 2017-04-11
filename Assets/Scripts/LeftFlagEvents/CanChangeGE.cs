using UnityEngine;
using System.Collections;
using System;
using LeftFlagEvents;

public class CanChangeGE : BaseEventFlagScript
{
    public override void OnClickEvent()
    {
        GameManagerScript.GM.ToggleTechMenu(RandomEventManager.REMInstance.GEMenu);
    }

    //Проверка условия для отображения флага
    public override bool TestSelfCondition()
    {
        return (GameManagerScript.GM.Player.TYGEDiscounter > 0);
    }
}
