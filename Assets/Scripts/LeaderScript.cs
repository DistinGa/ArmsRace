using UnityEngine;
using System.Collections;

[System.Serializable]
public class LeaderScript
{
    public LeaderType LeaderType;
    public int LeaderID; //1-4

    //var = 1 - левый список, 2 - правый
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