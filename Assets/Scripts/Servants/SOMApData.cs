using UnityEngine;

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

