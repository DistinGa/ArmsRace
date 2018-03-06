using UnityEngine;
using Steamworks;
using System.Collections;

public class SettingsScript : MonoBehaviour
{
    readonly AppId_t ArmageddonID = (AppId_t)759170;
    readonly AppId_t IndustrID = (AppId_t)792490;
    readonly AppId_t UNID = (AppId_t)794400;
    readonly AppId_t PoliticsID = (AppId_t)794401;

    public static SettingsScript Settings;
    public bool mVideo { get; set; } // tru-используем avi-видео, fals-используем картинки
    public bool mMusicOn { get; set; }   //вкл/выкл фоновой музыки
    public bool mSoundOn { get; set; }   //вкл/выкл остальных звуков
    public bool mVoiceOn { get; set; }   //вкл/выкл голосовых сообщений
    public float mMusicVol { get; set; }
    public float mSoundVol { get; set; }
    public bool mTurnBaseOn { get; set; }

    public bool Ironmode { get; set; }
    public int AIPower { get; set; }
    public Authority playerSelected { get; set; }
    public bool NeedLoad { get; set; }
    public LeaderScript PlayerLeader;
    [Space(10)]
    public bool news3D; //использование 3Д объектов для отображения новостей

    [Space(10)]
    [SerializeField]
    bool DLC_Armageddon;
    [SerializeField]
    bool DLC_Industr, DLC_UN, DLC_Politics;

    public bool ArmageddonPurchased
    {
        get
        {
#if DEBUG
            return true;
#endif
#if !DEBUG
            return SteamApps.BIsDlcInstalled(ArmageddonID);
#endif
        }
    }

    public bool ArmageddonAvailable
    {
        set { DLC_Armageddon = value; }
        get
        {
            if (ArmageddonPurchased)
                return DLC_Armageddon;
            else
                return false;
        }
    }

    public bool IndustrPurchased
    {
        get
        {
#if DEBUG
            return true;
#endif
#if !DEBUG
            return SteamApps.BIsDlcInstalled(IndustrID);
#endif
        }
    }

   public bool IndustrAvailable
    {
        set { DLC_Industr = value; }
        get
        {
            if (IndustrPurchased)
                return DLC_Industr;
            else
                return false;
        }
    }

    public bool UNPurchased
    {
        get
        {
#if DEBUG
            return true;
#endif
#if !DEBUG
            return SteamApps.BIsDlcInstalled(UNID);
#endif
        }
    }

    public bool UNAvailable
    {
        set { DLC_UN = value; }
        get
        {
            if (UNPurchased)
                return DLC_UN;
            else
                return false;
        }
    }

    public bool PoliticsPurchased
    {
        get
        {
#if DEBUG
            return true;
#endif
#if !DEBUG
            return SteamApps.BIsDlcInstalled(PoliticsID);
#endif
        }
    }

    public bool PoliticsAvailable
    {
        set { DLC_Politics = value; }
        get
        {
            if (PoliticsPurchased)
                return DLC_Politics;
            else
                return false;
        }
    }

    public void Awake()
    {
        //Singletone
        if (Settings != null)
        {
            Destroy(gameObject);
            return;
        }

        Settings = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    public void SaveSettings()
    {
        SavedSettings.VideoNews = mVideo;
        SavedSettings.MusicEnable = mMusicOn;
        SavedSettings.SoundEnable = mSoundOn;
        SavedSettings.Voice = mVoiceOn;
        SavedSettings.TurnBaseEnable = mTurnBaseOn;

        SavedSettings.SoundVolume = mMusicVol;
        SavedSettings.MusicVolume = mSoundVol;

        SavedSettings.ArmageddonEnable = DLC_Armageddon;
        SavedSettings.IndustrializationEnable = DLC_Industr;
    }

    public void LoadSettings()
    {
        mVideo = SavedSettings.VideoNews;
        mMusicOn = SavedSettings.MusicEnable;
        mSoundOn = SavedSettings.SoundEnable;
        mVoiceOn = SavedSettings.Voice;
        mTurnBaseOn = SavedSettings.TurnBaseEnable;

        mMusicVol = SavedSettings.SoundVolume;
        mSoundVol = SavedSettings.MusicVolume;

        DLC_Armageddon = SavedSettings.ArmageddonEnable;
        DLC_Industr = SavedSettings.IndustrializationEnable;
    }
}
