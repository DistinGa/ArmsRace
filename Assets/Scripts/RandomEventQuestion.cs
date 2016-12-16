using UnityEngine;
using System.Collections;

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
