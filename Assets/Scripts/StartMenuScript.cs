using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Steamworks;

public class StartMenuScript : MonoBehaviour
{
    public Animator Animator;
    [Space(10)]
    public Toggle VideoNews;
    public Toggle Voice;
    public Toggle Music;
    public Slider MusicVolume;
    public Toggle Sound;
    public Slider SoundVolume;
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
    [SerializeField]
    Text CurSentence;
    [TextArea(2, 5)]
    [SerializeField]
    string[] Sentences;

    private AudioSource AS;

    public void Start()
    {
        AS = GetComponent<AudioSource>();

        if (SteamApps.BIsDlcInstalled((AppId_t)508430))
        {
            VideoNews.interactable = true;
            Voice.interactable = true;
            VideoNews.isOn = SettingsScript.Settings.mVideo;
            Voice.isOn = SettingsScript.Settings.mVoiceOn;
        }
        else
        {
            VideoNews.interactable = false;
            Voice.interactable = false;
            VideoNews.isOn = false;
            Voice.isOn = false;
        }

        Music.isOn = SettingsScript.Settings.mMusicOn;
        Sound.isOn = SettingsScript.Settings.mSoundOn;
        MusicVolume.value = SettingsScript.Settings.mMusicVol;
        SoundVolume.value = SettingsScript.Settings.mSoundVol;

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
        Invoke(("StartGame"), 5f);
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

    public void PlaySound(AudioClip ac)
    {
        if(SettingsScript.Settings.mSoundOn)
            AS.PlayOneShot(ac, SettingsScript.Settings.mSoundVol);
    }
}
