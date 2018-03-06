using UnityEngine;
using System.Collections;

public class LoadableObject : MonoBehaviour {
    [SerializeField]
    GameObject news3DPrefab;
    GameObject loadedObject;

    public bool IsLoaded
    {
        get { return transform.childCount > 0; }
    }

    public void LoadObject()
    {
        if (news3DPrefab != null && !IsLoaded)
        {
            loadedObject = Instantiate(news3DPrefab);
            loadedObject.transform.parent = transform;
            loadedObject.transform.name = "LoadedObject";
        }
        else
        {
            print(gameObject.name + ": Не указан префаб для загрузки.");
        }
    }

    public GameObject FindNewsObject(string ObjName)
    {
        return transform.FindChild("LoadedObject/" + ObjName).gameObject;
    }

    public GameObject LoadedObject
    {
        get { return loadedObject; }
    }
}
