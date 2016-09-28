using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScript : MonoBehaviour {
    void Start()
    {
        AudioSource AS = GetComponent<AudioSource>();
        if (SavedSettings.MusicEnable)
        {
            AS.volume = SavedSettings.MusicVolume;
            AS.Play();
        }
    }

    public void LoadStartMenu() {
        Destroy(SettingsScript.Settings.gameObject);
        SceneManager.LoadScene("StartMenu");
    }
}
