﻿using UnityEngine;
using UnityEngine.UI;

public class Armageddon : MonoBehaviour {
    public GameObject RocketPanel;
    public Text NuclearUSSR, NuclearUSA;
    [Space(10)]
    public int[] PlayerWinPercent = new int[3];
    public int[] AIWinPercent = new int[3];

    void Start()
    {
        if (SettingsScript.Settings.CheckDLC_Armageddon())
        {
            RocketPanel.SetActive(true);
            NuclearUSSR.gameObject.SetActive(true);
            NuclearUSA.gameObject.SetActive(true);
        }
    }

    public PlayerScript GetWinner(int AIpower)
    {
        if (!SettingsScript.Settings.CheckDLC_Armageddon())
            return null;

        if (AIpower > AIWinPercent.Length)
        {
            Debug.LogError("Неверный уровень сложности", gameObject);
            return null;
        }

        PlayerScript res = null;
        GameManagerScript GM = GameManagerScript.GM;
        PlayerScript opp = GM.AI.AIPlayer;

        if (GM.Player.RelativeNuclearPower() >= PlayerWinPercent[AIpower] && GM.Player.Score > opp.Score)
        {
            res = GM.Player;
        }
        else if(opp.RelativeNuclearPower() >= AIWinPercent[AIpower] && GM.Player.Score < opp.Score)
            res = opp;

        return res;
    }

    public void SetNuclearPercent(Authority aut, int percent)
    {
        if (aut == Authority.Soviet)
            NuclearUSSR.text = percent.ToString() + "%";
        if (aut == Authority.Amer)
            NuclearUSA.text = percent.ToString() + "%";
    }
}
