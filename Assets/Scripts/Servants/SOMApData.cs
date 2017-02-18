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
    public string name;
    [SerializeField]
    [HideInInspector]
    string objName;
    [Tooltip("Регион для видео")]
    public Region Region;
    [Tooltip("В чьём альянсе")]
    public Authority Authority;
    [Space(10)]
    public int Score;
    public float Support;
    [Space(10)]
    [Tooltip("Влияние СССР")]
    public int SovInf;
    [Tooltip("Влияние США")]
    public int AmInf;
    [Tooltip("Нейтралитет")]
    public int NInf;
    [Space(10)]
    [Tooltip("Правительственные войска")]
    public int GovForce;
    [Tooltip("Оппозиционные войска")]
    public int OppForce;
    [Space(10)]
    public int KGB;
    public int CIA;
    [Space(10)]
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

            md[i].Region = c.Region;
            md[i].Authority = c.Authority;
            md[i].Score = c.Score;
            md[i].Support = c.Support;
            md[i].SovInf = c.SovInf;
            md[i].AmInf = c.AmInf;
            md[i].NInf = c.NInf;
            md[i].GovForce = c.GovForce;
            md[i].OppForce = c.OppForce;
            md[i].KGB = c.KGB;
            md[i].CIA = c.CIA;

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

            c.Region = md[i].Region;
            c.Authority = md[i].Authority;
            c.Score = md[i].Score;
            c.Support = md[i].Support;
            c.SovInf = md[i].SovInf;
            c.AmInf = md[i].AmInf;
            c.NInf = md[i].NInf;
            c.GovForce = md[i].GovForce;
            c.OppForce = md[i].OppForce;
            c.KGB = md[i].KGB;
            c.CIA = md[i].CIA;

            i++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}