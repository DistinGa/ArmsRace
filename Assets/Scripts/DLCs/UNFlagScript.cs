using UnityEngine.UI;
using System.Collections;
using LeftFlagEvents;

public class UNFlagScript : BaseEventFlagScript
{
    GameManagerScript GM;

    void Start()
    {
        GM = GameManagerScript.GM;
    }

    public override void OnClickEvent()
    {
        GM.ToggleTechMenu(GM.DLC_UN.UNMenu.gameObject);
    }

    //Проверка условия для отображения флага
    public override bool TestSelfCondition()
    {
        return GM.DLC_UN.CheckLFConditions();
    }
}
