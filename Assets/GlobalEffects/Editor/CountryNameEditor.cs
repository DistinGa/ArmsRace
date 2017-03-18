using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GlobalEffects;

[CustomPropertyDrawer(typeof(CountryNameAtribute))]
public class CountryNameEditor : PropertyDrawer
{

    void OnEnable()
    {
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<string> cnames = new List<string>();
        cnames.Add("");

        foreach (var item in GameObject.Find("Countries").GetComponentsInChildren<CountryScript>())
        {
            cnames.Add(item.Name);
        }

        cnames.Sort();

        int curIndx = cnames.FindIndex(c => c == property.stringValue);

        //int indx = EditorGUILayout.Popup(curIndx, cnames.ToArray());
        int indx = EditorGUI.Popup(position, "Country", curIndx, cnames.ToArray());

        property.stringValue = cnames[indx];
    }
}
