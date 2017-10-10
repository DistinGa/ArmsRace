using UnityEngine;

public class ExitMenuScript : MonoBehaviour {
    public GameObject SaveButton;

	void OnEnable () {
        SaveButton.SetActive(!SettingsScript.Settings.Ironmode);
    }
}
