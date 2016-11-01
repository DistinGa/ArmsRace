using UnityEngine;
using System.Collections;

[System.Serializable]
public class LeaderScript
{
    public LeaderType LeaderType;
    public int LeaderID; //1-4

    public string GetBonuses(int var)
    {
        return "";
    }
}

public enum LeaderType
{
    Historic=0,
    Economic,
    Militaristic,
    Diplomatic
}