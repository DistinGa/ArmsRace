using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "MapData", menuName = "Create MapData object", order = 0)]
public class SOMApData : ScriptableObject
{
    public MapData[] md = new MapData[59];
}

[System.Serializable]
public class MapData
{
    [SerializeField]
    [HideInInspector]
    string objName;
    public string name;
    public int air;
    public int ground;
    public int sea;

    public string ObjectName
    {
        get { return objName; }
        set { objName = value; }
    }
}

[CustomEditor(typeof(SOMApData))]
public class TestOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button(new GUIContent("Export", "Из игры в файл")))
            Export();
        if (GUILayout.Button(new GUIContent("Import", "Из файла в игру")))
            Import();

        DrawDefaultInspector();
    }

    void Export()
    {
        MapData[] md = (Selection.activeObject as SOMApData).md;

        int i = 0;
        foreach (var c in GameObject.Find("Countries").GetComponentsInChildren<CountryScript>())
        {
            md[i].ObjectName = c.gameObject.name;
            md[i].name = c.Name;
            md[i].air = c.Air;
            md[i].ground = c.Ground;
            md[i].sea = c.Sea;

            i++;
        }
    }

    void Import()
    {
        MapData[] md = (Selection.activeObject as SOMApData).md;
        CountryScript c;

        int i = 0;
        foreach (var item in md)
        {
            c = GameObject.Find(item.ObjectName).GetComponent<CountryScript>();
            c.Air = item.air;
            c.Ground = item.ground;
            c.Sea = item.sea;

            i++;
        }
    }
}