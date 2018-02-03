﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Steamworks;

public class StartMenuScript : MonoBehaviour
{
    [SerializeField]
    Image LoadingBar;
    public Animator Animator;
    [Space(10)]
    public Toggle VideoNews;
    public Toggle Music;
    public Slider MusicVolume;
    public Toggle Sound;
    public Slider SoundVolume;
    public Toggle RTS;
    public Toggle TB;
    [Space(10)]
    public Toggle Easy;
    public Toggle Medium;
    public Toggle Hard;
    [Space(10)]
    public RectTransform UsaEasy;
    public RectTransform UsaMed;
    public RectTransform UsaHard;
    public RectTransform UssrEasy;
    public RectTransform UssrMed;
    public RectTransform UssrHard;
    [Space(10)]
    public Text Games;
    public Text Wins;
    public Text Efficiency;
    [Space(10)]
    [SerializeField]
    Text CurSentence;
    [TextArea(2, 5)]
    [SerializeField]
    string[] Sentences;
    [SerializeField]
    Sprite[] sprSuiteArray = new Sprite[3];
    [SerializeField]
    SOLP soLeaderProperties;

    [Space(10)]
    public Toggle Armageddon;
    public Toggle Industrialization;

    private AudioSource AS;

    public void Start()
    {
        AS = GetComponent<AudioSource>();

        //if (SteamApps.BIsDlcInstalled((AppId_t)508430))
        //{
        //    VideoNews.interactable = true;
        //    Voice.interactable = true;
        //    VideoNews.isOn = SettingsScript.Settings.mVideo;
        //    Voice.isOn = SettingsScript.Settings.mVoiceOn;
        //}
        //else
        //{
        //    VideoNews.interactable = false;
        //    Voice.interactable = false;
        //    VideoNews.isOn = false;
        //    Voice.isOn = false;
        //}


        GameObject.Find("ToggleUSSR").GetComponent<Toggle>().isOn = true;
        GameObject.Find("Historic").GetComponent<Toggle>().isOn = true;
        GameObject.Find("Leader1").GetComponent<Toggle>().isOn = true;

        RTS.isOn = !SettingsScript.Settings.mTurnBaseOn;
        TB.isOn = SettingsScript.Settings.mTurnBaseOn;

        Music.isOn = SettingsScript.Settings.mMusicOn;
        Sound.isOn = SettingsScript.Settings.mSoundOn;
        MusicVolume.value = SettingsScript.Settings.mMusicVol;
        SoundVolume.value = SettingsScript.Settings.mSoundVol;

        if (SettingsScript.Settings.ArmageddonPurchased)
        {
            Armageddon.gameObject.SetActive(true);
            Armageddon.isOn = SettingsScript.Settings.ArmageddonAvailable;
        }
        else
            Armageddon.gameObject.SetActive(false);

        if (SettingsScript.Settings.IndustrPurchased)
        {
            Industrialization.gameObject.SetActive(true);
            Industrialization.isOn = SettingsScript.Settings.IndustrAvailable;
        }
        else
            Industrialization.gameObject.SetActive(false);

        //экран кампаний
        UsaEasy.gameObject.SetActive(!SavedSettings.Mission1USA);
        UsaMed.gameObject.SetActive(!SavedSettings.Mission2USA);
        UsaHard.gameObject.SetActive(!SavedSettings.Mission3USA);
        UssrEasy.gameObject.SetActive(!SavedSettings.Mission1SU);
        UssrMed.gameObject.SetActive(!SavedSettings.Mission2SU);
        UssrHard.gameObject.SetActive(!SavedSettings.Mission3SU);

        AudioSource MusicSource = GameObject.Find("StartScreen").GetComponent<AudioSource>();
        MusicSource.volume = SettingsScript.Settings.mMusicVol;
        MusicSource.enabled = SettingsScript.Settings.mMusicOn;

        CurSentence.text = Sentences[Random.Range(0, Sentences.Length)];

        Games.text = SavedSettings.Games.ToString();
        Wins.text = SavedSettings.Wins.ToString();
        if (SavedSettings.Games >= 5)
            Efficiency.text = (100f * SavedSettings.Wins / SavedSettings.Games).ToString() + "%";
        else
        {
            GameObject.Find("Label:Eff").SetActive(false);
            Efficiency.text = "";
        }
    }

    public void Exit()
    {
        SettingsScript.Settings.SaveSettings();
        Application.Quit();
    }

    public void LoadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void InvokeStart()
    {
        SettingsScript.Settings.SaveSettings();
        Animator.Play(0);
        //Invoke(("StartGame"), 5f);
        StartCoroutine(DelayedStartGame());
    }

    public IEnumerator DelayedStartGame()
    {
        FindObjectOfType<UnityEngine.EventSystems.EventSystem>().gameObject.SetActive(false);
        AsyncOperation Progress = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
        while (!Progress.isDone)
        {
            LoadingBar.fillAmount = Progress.progress;
            yield return new WaitForSeconds(0.5f);
        }
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("GameScene"));
        //SceneManager.UnloadScene("StartMenu");        
        SceneManagerScript.ChangeScenes("StartMenu", "GameScene");
    }

    public void StartGame()
    {
        LoadScene("GameScene");
    }

    public void SelectPlayer(bool Amer)
    {
        if (Amer)
            SettingsScript.Settings.playerSelected = Authority.Amer;
        else
            SettingsScript.Settings.playerSelected = Authority.Soviet;

        SetAILevel(Amer);
    }

    public void SetAILevel(bool Amer)
    {
        Easy.interactable = true;

        if (Amer)
        {
            Easy.isOn = !SavedSettings.Mission1USA;
            Medium.interactable = SavedSettings.Mission1USA;
            Medium.isOn = (SavedSettings.Mission1USA && !SavedSettings.Mission2USA);
            Hard.interactable = SavedSettings.Mission2USA;
            Hard.isOn = SavedSettings.Mission2USA;
        }
        else
        {
            Easy.isOn = !SavedSettings.Mission1SU;
            Medium.interactable = SavedSettings.Mission1SU;
            Medium.isOn = (SavedSettings.Mission1SU && !SavedSettings.Mission2SU);
            Hard.interactable = SavedSettings.Mission2SU;
            Hard.isOn = SavedSettings.Mission2SU;
        }
    }

    public void OpenManual()
    {
        System.Diagnostics.Process.Start(Application.streamingAssetsPath + "/Quick guide.pdf");
    }

    public void OpenTutorial()
    {
        try
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/watch?v=5T90YJiGODY");
        }
        catch (System.Exception)
        {
            //throw;
        }
    }

    public void PlaySound(AudioClip ac)
    {
        if (SettingsScript.Settings.mSoundOn)
        {
            AS.PlayOneShot(ac, SettingsScript.Settings.mSoundVol);
        }
    }

    public void SetGameMode(int lt)
    {
        SettingsScript.Settings.PlayerLeader.LeaderType = (LeaderType)lt;
        SetSuite(SettingsScript.Settings.PlayerLeader.LeaderID);
    }

    public void SetSuite(int LID)
    {
        Sprite sprSuite = null;

        SettingsScript.Settings.PlayerLeader.LeaderID = LID;

        //if (SettingsScript.Settings.PlayerLeader.LeaderType == LeaderType.Historic)
        //{
        //    if (SettingsScript.Settings.playerSelected == Authority.Amer)
        //    {
        //        if (LID == 1 || LID == 3)
        //            sprSuite = sprSuiteArray[1];    //светлый
        //        else
        //            sprSuite = sprSuiteArray[0];    //тёмный
        //    }
        //    else //СССР
        //    {
        //        if (LID == 1)
        //            sprSuite = sprSuiteArray[2];    //китель
        //        else if (LID == 2)
        //            sprSuite = sprSuiteArray[1];    //светлый
        //        else
        //            sprSuite = sprSuiteArray[0];    //тёмный
        //    }
        //}
        //else if (SettingsScript.Settings.PlayerLeader.LeaderType == LeaderType.Economic)
        //    sprSuite = sprSuiteArray[1];    //светлый
        //else if (SettingsScript.Settings.PlayerLeader.LeaderType == LeaderType.Diplomatic)
        //    sprSuite = sprSuiteArray[0];    //тёмный
        //else if (SettingsScript.Settings.PlayerLeader.LeaderType == LeaderType.Militaristic)
        //    sprSuite = sprSuiteArray[2];    //китель

        switch (SettingsScript.Settings.PlayerLeader.ActualLeaderType)
        {
            case LeaderType.Economic:
                sprSuite = sprSuiteArray[1];    //светлый
                break;
            case LeaderType.Militaristic:
                sprSuite = sprSuiteArray[2];    //китель
                break;
            case LeaderType.Diplomatic:
                sprSuite = sprSuiteArray[0];    //тёмный
                break;
        }

        GameObject.Find("Suite").GetComponent<Image>().sprite = sprSuite;
        GameObject.Find("BonusLeft").GetComponent<Text>().text = SettingsScript.Settings.PlayerLeader.GetBonuses(1, soLeaderProperties);
        GameObject.Find("BonusRight").GetComponent<Text>().text = SettingsScript.Settings.PlayerLeader.GetBonuses(2, soLeaderProperties);
    }

    public void SetTogglesActive(bool b)
    {
        foreach (var item in FindObjectsOfType<MarkToggle>())
        {
            item.SetToggleActive(b);
        }

        GameObject.Find("IronmodeToggle").GetComponent<Toggle>().interactable = (b && SettingsScript.Settings.AIPower > 0);
    }
}

