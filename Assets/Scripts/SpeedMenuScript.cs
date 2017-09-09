using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SpeedMenuScript : MonoBehaviour
{
    RectTransform ToggleGroup;
    GameManagerScript GM;
    Toggle[] ToggleArray = new Toggle[5];

    void Awake()
    {
        GM = GameManagerScript.GM;
        ToggleGroup = GetComponent<RectTransform>();

        int i = 0;
        foreach (RectTransform item in ToggleGroup)
        {
            ToggleArray[i] = item.GetComponent<Toggle>();
            i++;
        }
    }

    void OnEnable()
    {
        for (int i = 0; i < ToggleArray.Length; i++)
            ToggleArray[i].isOn = false;

        if (GM.IsPaused)
            ToggleArray[4].isOn = true;
        else if (GM.IsTurnBased)
            ToggleArray[0].isOn = true;
        else
            switch (GM.CurrentSpeed)
            {
                case 0:
                    ToggleArray[1].isOn = true;
                    break;
                case 1:
                    ToggleArray[2].isOn = true;
                    break;
                case 2:
                    ToggleArray[3].isOn = true;
                    break;
            }
    }

    public void ChangeSpeed(int toggleIndx)
    {
        if (!ToggleArray[toggleIndx].isOn)
            return;

        switch (toggleIndx)
        {
            case 0:
                GM.IsTurnBased = true;
                GM.SetPause(0);
                break;
            case 1:
                GM.IsTurnBased = false;
                GM.SetPause(0);
                GM.CurrentSpeed = 0;
                break;
            case 2:
                GM.IsTurnBased = false;
                GM.SetPause(0);
                GM.CurrentSpeed = 1;
                break;
            case 3:
                GM.IsTurnBased = false;
                GM.SetPause(0);
                GM.CurrentSpeed = 2;
                break;
            case 4:
                GM.SetPause(1);
                break;
        }
    }
}
