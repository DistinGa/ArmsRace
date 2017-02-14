﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class LeaderScript
{
    public LeaderType LeaderType;
    public int LeaderID; //1-4 (в историческом порядке)

    public LeaderType ActualLeaderType
    {
        get
        {
            //Если исторический тип, то определяем действительный тип.
            LeaderType lt = LeaderType;
            if (lt == LeaderType.Historic)
            {
                switch (LeaderID)
                {
                    case 1:
                        lt = LeaderType.Militaristic;
                        break;
                    case 2:
                        lt = LeaderType.Economic;
                        break;
                    case 3:
                        lt = LeaderType.Diplomatic;
                        break;
                    case 4:
                        lt = LeaderType.Diplomatic;
                        break;
                }
            }

            return lt;
        }
    }   
    
    //var = 1 - левый список, 2 - правый
    public string GetBonuses(int var, SOLP soLeaderProperties)
    {
        string res = "";

        if (var == 1)
        {
            switch (LeaderID)
            {
                case 1:
                    res = "- " + soLeaderProperties.descriptionFPBonus;
                    break;
                case 2:
                    res = "- " + soLeaderProperties.descriptionSpace;
                    break;
                case 3:
                    res = "- " + soLeaderProperties.descriptionFDS;
                    break;
                case 4:
                    res = "- " + soLeaderProperties.descriptionIB;
                    break;
            }
        }
        else
        {
            switch (ActualLeaderType)
            {
                case LeaderType.Militaristic:
                    res = "- " + soLeaderProperties.descriptionMilDisc + "\n- " + soLeaderProperties.descriptionFMil;
                    break;
                case LeaderType.Diplomatic:
                    res = "- " + soLeaderProperties.descriptionDipDisc + "\n- " + soLeaderProperties.descriptionSpyDisc;
                    break;
                case LeaderType.Economic:
                    res = "- " + soLeaderProperties.descriptionMB + "\n- " + soLeaderProperties.descriptionGEC;
                    break;
            }
        }

        return res;
    }

    public int GetFPBonus()
    {
        int res = 0;
        if (LeaderID == 1)
            res = GameManagerScript.GM.LeaderPrefs.FPBonus;

        return res;
    }

    public float GetSpaceDiscount()
    {
        float res = 0;
        if (LeaderID == 2)
            res = GameManagerScript.GM.LeaderPrefs.SpaceDiscount;

        return res;
    }

    public int GetAnnualFreeDipSpy()
    {
        int res = 0;
        if (LeaderID == 3)
            res = GameManagerScript.GM.LeaderPrefs.AnnualFreeDipSpy;

        return res;
    }

    public int GetInfluenceBoost()
    {
        int res = 1;
        if (LeaderID == 4)
            res = GameManagerScript.GM.LeaderPrefs.InfluenceBoost;

        return res;
    }

    public float GetMilTechDiscount()
    {
        float res = 0;
        if (ActualLeaderType == LeaderType.Militaristic)
            res = GameManagerScript.GM.LeaderPrefs.MilTechDiscount;

        return res;
    }

    public int GetAnnualFreeMil()
    {
        int res = 0;
        if (ActualLeaderType == LeaderType.Militaristic)
            res = GameManagerScript.GM.LeaderPrefs.AnnualFreeMil;

        return res;
    }

    public float GetDipDiscount()
    {
        float res = 0;
        if (ActualLeaderType == LeaderType.Diplomatic)
            res = GameManagerScript.GM.LeaderPrefs.DipDiscount;

        return res;
    }

    public float GetSpyDiscount()
    {
        float res = 0;
        if (ActualLeaderType == LeaderType.Diplomatic)
            res = GameManagerScript.GM.LeaderPrefs.SpyDiscount;

        return res;
    }

    public int GetMeetingBoost()
    {
        int res = 1;
        if (ActualLeaderType == LeaderType.Economic)
            res = GameManagerScript.GM.LeaderPrefs.MeetingBoost;

        return res;
    }

    public int GetTenYearsGlobalEffectsChange()
    {
        int res = 0;
        if (ActualLeaderType == LeaderType.Economic)
            res = GameManagerScript.GM.LeaderPrefs.TenYearsGlobalEffectsChange;

        return res;
    }

}

public enum LeaderType
{
    Historic=0,
    Economic,
    Militaristic,
    Diplomatic
}

public enum LeaderName
{
    Truman,
    Kennedy,
    Nixon,
    Reagan,
    Stalin,
    Khrushchev,
    Brezhnev,
    Gorbachev
}