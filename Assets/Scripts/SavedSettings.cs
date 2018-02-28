using UnityEngine;
using System;

public static class SavedSettings {
    static float musicVolume;
    static float soundVolume;
    static bool musicEnable;
    static bool soundEnable;
    static bool videoNews;
    static bool voice;
    static bool mission1USA, mission2USA, mission3USA;
    static bool mission1SU, mission2SU, mission3SU;
    static bool turnBaseEnable;

    //вызывается из актуального геттера
    static bool commonBoolGet(string strPar)
    {
        if (PlayerPrefs.HasKey(strPar))
            return PlayerPrefs.GetInt(strPar) == 1 ? true : false;
        else
        {
            return false;
        }
    }

    //вызывается из актуального сеттера
    static void commonBoolSet(string strPar, bool value)
    {
        PlayerPrefs.SetInt(strPar, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static float MusicVolume
    {
        get
        {
            if (PlayerPrefs.HasKey("MusicVolume"))
                return PlayerPrefs.GetFloat("MusicVolume");
            else
            {
                MusicVolume = 0.5f;
                return MusicVolume;
            }
        }

        set
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            PlayerPrefs.Save();
        }
    }

    public static float SoundVolume
    {
        get
        {
            if (PlayerPrefs.HasKey("SoundVolume"))
                return PlayerPrefs.GetFloat("SoundVolume");
            else
            {
                SoundVolume = 0.5f;
                return SoundVolume;
            }
        }

        set
        {
            PlayerPrefs.SetFloat("SoundVolume", value);
            PlayerPrefs.Save();
        }
    }

    public static bool TurnBaseEnable
    {
        get
        {
            if (PlayerPrefs.HasKey("TurnBaseEnable"))
                return PlayerPrefs.GetInt("TurnBaseEnable") == 1 ? true : false;
            else
            {
                turnBaseEnable = false;
                return turnBaseEnable;
            }
        }

        set
        {
            PlayerPrefs.SetInt("TurnBaseEnable", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool MusicEnable
    {
        get
        {
            if (PlayerPrefs.HasKey("MusicEnable"))
                return PlayerPrefs.GetInt("MusicEnable") == 1 ? true: false;
            else
            {
                MusicEnable = true;
                return MusicEnable;
            }
        }

        set
        {
            PlayerPrefs.SetInt("MusicEnable", value?1:0);
            PlayerPrefs.Save();
        }
    }

    public static bool SoundEnable
    {
        get
        {
            if (PlayerPrefs.HasKey("SoundEnable"))
                return PlayerPrefs.GetInt("SoundEnable") == 1 ? true : false;
            else
            {
                SoundEnable = true;
                return SoundEnable;
            }
        }

        set
        {
            PlayerPrefs.SetInt("SoundEnable", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool VideoNews
    {
        get
        {
            if (PlayerPrefs.HasKey("VideoNews"))
                return PlayerPrefs.GetInt("VideoNews") == 1 ? true : false;
            else
            {
                VideoNews = false;
                return VideoNews;
            }
        }

        set
        {
            PlayerPrefs.SetInt("VideoNews", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool Voice
    {
        get
        {
            if (PlayerPrefs.HasKey("Voice"))
                return PlayerPrefs.GetInt("Voice") == 1 ? true : false;
            else
            {
                Voice = false;
                return Voice;
            }
        }

        set
        {
            PlayerPrefs.SetInt("Voice", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool Mission1USA
    {
        get
        {
            if (PlayerPrefs.HasKey("Mission1USA"))
                return PlayerPrefs.GetInt("Mission1USA") == 1 ? true : false;
            else
            {
                Mission1USA = false;
                return Mission1USA;
            }
        }

        set
        {
            PlayerPrefs.SetInt("Mission1USA", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool Mission2USA
    {
        get
        {
            if (PlayerPrefs.HasKey("Mission2USA"))
                return PlayerPrefs.GetInt("Mission2USA") == 1 ? true : false;
            else
            {
                Mission2USA = false;
                return Mission2USA;
            }
        }

        set
        {
            PlayerPrefs.SetInt("Mission2USA", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool Mission3USA
    {
        get
        {
            if (PlayerPrefs.HasKey("Mission3USA"))
                return PlayerPrefs.GetInt("Mission3USA") == 1 ? true : false;
            else
            {
                Mission3USA = false;
                return Mission3USA;
            }
        }

        set
        {
            PlayerPrefs.SetInt("Mission3USA", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool Mission1SU
    {
        get
        {
            if (PlayerPrefs.HasKey("Mission1SU"))
                return PlayerPrefs.GetInt("Mission1SU") == 1 ? true : false;
            else
            {
                Mission1SU = false;
                return Mission1SU;
            }
        }

        set
        {
            PlayerPrefs.SetInt("Mission1SU", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool Mission2SU
    {
        get
        {
            if (PlayerPrefs.HasKey("Mission2SU"))
                return PlayerPrefs.GetInt("Mission2SU") == 1 ? true : false;
            else
            {
                Mission2SU = false;
                return Mission2SU;
            }
        }

        set
        {
            PlayerPrefs.SetInt("Mission2SU", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool Mission3SU
    {
        get
        {
            if (PlayerPrefs.HasKey("Mission3SU"))
                return PlayerPrefs.GetInt("Mission3SU") == 1 ? true : false;
            else
            {
                Mission3SU = false;
                return Mission3SU;
            }
        }

        set
        {
            PlayerPrefs.SetInt("Mission3SU", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool ArmageddonEnable
    {
        get
        {
            if (PlayerPrefs.HasKey("ArmageddonEnable"))
                return PlayerPrefs.GetInt("ArmageddonEnable") == 1 ? true : false;
            else
            {
                return false;
            }
        }

        set
        {
            PlayerPrefs.SetInt("ArmageddonEnable", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool IndustrializationEnable
    {
        get
        {
            if (PlayerPrefs.HasKey("IndustrializationEnable"))
                return PlayerPrefs.GetInt("IndustrializationEnable") == 1 ? true : false;
            else
            {
                return false;
            }
        }

        set
        {
            PlayerPrefs.SetInt("IndustrializationEnable", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static int Games
    {
        get
        {
            if (PlayerPrefs.HasKey("Games"))
                return PlayerPrefs.GetInt("Games");
            else
            {
                return 0;
            }
        }

        set
        {
            PlayerPrefs.SetInt("Games", value);
            PlayerPrefs.Save();
        }
    }

    public static int Wins
    {
        get
        {
            if (PlayerPrefs.HasKey("Wins"))
                return PlayerPrefs.GetInt("Wins");
            else
            {
                return 0;
            }
        }

        set
        {
            PlayerPrefs.SetInt("Wins", value);
            PlayerPrefs.Save();

            //Steam achievments
            if (Games >= 5)
            {
                if ((100f * Wins / Games) >= 50)
                    SteamManager.UnLockAchievment("50% of Ironmode efficiency");

                if ((100f * Wins / Games) >= 70)
                    SteamManager.UnLockAchievment("70% of Ironmode efficiency");

                if ((100f * Wins / Games) >= 90)
                    SteamManager.UnLockAchievment("90% of Ironmode efficiency");
            }
        }
    }


    public static bool ArmageddonAchieved
    {
        get
        {
            return commonBoolGet("Arma");
        }

        set
        {
            commonBoolSet("Arma", value);
            SteamManager.UnLockAchievment("Nuclear strike");
        }
    }

    public static bool IndustrializationAchieved
    {
        get
        {
            return commonBoolGet("Industr");
        }

        set
        {
            commonBoolSet("Industr", value);
            SteamManager.UnLockAchievment("Industrialisation");
        }
    }

}
