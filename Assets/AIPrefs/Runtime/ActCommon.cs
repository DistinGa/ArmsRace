using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

///
/// !!! Machine generated code !!!
///
/// A class which deriveds ScritableObject class so all its data 
/// can be serialized onto an asset data file.
/// 
[System.Serializable]
public class ActCommon : ScriptableObject 
{	
    [HideInInspector] [SerializeField] 
    public string SheetName = "";
    
    [HideInInspector] [SerializeField] 
    public string WorksheetName = "";
    
    // Note: initialize in OnEnable() not here.
    public ActData[] dataArray;
    
    void OnEnable()
    {		
//#if UNITY_EDITOR
        //hideFlags = HideFlags.DontSave;
//#endif
        // Important:
        //    It should be checked an initialization of any collection data before it is initialized.
        //    Without this check, the array collection which already has its data get to be null 
        //    because OnEnable is called whenever Unity builds.
        // 		
        if (dataArray == null)
            dataArray = new ActData[0];

    }

    //
    // Highly recommand to use LINQ to query the data sources.
    //

    public int GetValue(int row, int col)
    {
        int res = 0;
        switch (col)
        {
            case 0:
                res = dataArray[row].ActWarPlus;
                break;
            case 1:
                res = dataArray[row].ActPeacePlus;
                break;
            case 2:
                res = dataArray[row].ActWarMinus;
                break;
            case 3:
                res = dataArray[row].ActPeaceMinus;
                break;
        }
        return res;
    }
}
