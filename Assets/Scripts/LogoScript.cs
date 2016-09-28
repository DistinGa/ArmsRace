using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoScript : MonoBehaviour {

	void Start () {
        Invoke("StartAnimation", 1);
        Invoke("LoadStartMenu", 5);
	}

    void StartAnimation()
    {
        GetComponent<Animator>().SetTrigger("Start");
    }

    void LoadStartMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
