using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneManagerScript {

    static public void ChangeScenes(string from, string to)
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(to));
        SceneManager.UnloadScene(from);
        Resources.UnloadUnusedAssets();
        //GameObject EvSys = Object.FindObjectOfType<EventSystem>().gameObject;
        //EvSys.SetActive(false);
        //EvSys.SetActive(true);
    }
}
