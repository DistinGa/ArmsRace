﻿using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveManager
{
    public static void SaveGame(string fName = "save")
    {
        GameManagerScript GM = GameManagerScript.GM;
        SavedGame gameData = new SavedGame();

        CountryScript[] csArray = CountryScript.Countries().ToArray();
        gameData.countryData = new SavedCountryData[csArray.Length];
        for (int i = 0; i < csArray.Length; i++)
        {
            gameData.countryData[i] = csArray[i].GetSavedData();
        }

        gameData.playerData = new SavedPlayerData[2];
        gameData.playerData[0] = GM.GetPlayerByAuthority(Authority.Amer).GetSavedData();
        gameData.playerData[1] = GM.GetPlayerByAuthority(Authority.Soviet).GetSavedData();

        gameData.month = GM.CurrentMonth;
        gameData.SpeedIndx = GM.curSpeedIndex;

        //сериализация и запись
        string initPath = Application.streamingAssetsPath + "/Saves/";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream;
        if (!Directory.Exists(initPath))
            Directory.CreateDirectory(initPath);

        stream = File.Create(initPath + fName + ".sav");
        try
        {
            bf.Serialize(stream, gameData);
            stream.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка записи данных GameManager " + " " + e.Message);
            stream.Close();
        }
    }

    public static void LoadGame(string fName = "save")
    {
        string initPath = Application.streamingAssetsPath + "/Saves/";
        string filePath;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream;
        SavedGame gameData = new SavedGame();

        //загрузка данных, десериализация
        filePath = initPath + fName + ".sav";
        if (File.Exists(filePath))
        {
            stream = File.Open(filePath, FileMode.Open);
            gameData = (SavedGame)bf.Deserialize(stream);
            stream.Close();
        }

        //установка значений объектов
        GameManagerScript.GM.CurrentMonth = gameData.month;
        GameManagerScript.GM.curSpeedIndex = gameData.SpeedIndx;
        GameManagerScript.GM.Tick = GameManagerScript.GM.GameSpeedPrefs[gameData.SpeedIndx];

        foreach (SavedCountryData item in gameData.countryData)
        {
            CountryScript country = GameObject.Find(item.Name).GetComponent<CountryScript>();
            country.SetSavedData(item);
        }

        foreach (SavedPlayerData item in gameData.playerData)
        {
            PlayerScript player = GameManagerScript.GM.GetPlayerByAuthority(item.aut).GetComponent<PlayerScript>();
            player.SetSavedData(item);
        }
    }
}

[System.Serializable]
public struct SavedCountryData
{
    public string Name;

    public float support;
    public int SovInf;
    public int AmInf;
    public int NInf;
    public Authority LastAut;
    public int GovForce;
    public int OppForce;
    public int KGB;
    public int CIA;

    public List<CountryScript.StateSymbol> Symbols;

    public int DiscounterUsaMeeting, DiscounterRusMeeting;
    public int DiscounterUsaParade, DiscounterRusParade;
    public int DiscounterUsaSpy, DiscounterRusSpy, DiscounterUsaInfl, DiscounterRusInfl;
}

[System.Serializable]
public struct SavedPlayerData
{
    public Authority aut;

    public double Budget;
    public bool[] TechStatus;
    public bool[] MilAirTechStatus;
    public bool[] MilGndTechStatus;
    public bool[] MilSeaTechStatus;
    public bool[] MilRocketTechStatus;

    public List<int> History;
    public List<int> History2;
    public int militaryAmount;
    public int spyAmount;
    public int diplomatAmount;

    public Dictionary<OutlayField, UniOutlay> outlays;
    public int outlayChangeDiscounter;
}

[System.Serializable]
public class SavedGame
{
    public int month;
    public int SpeedIndx;
    public SavedCountryData[] countryData;
    public SavedPlayerData[] playerData;
}