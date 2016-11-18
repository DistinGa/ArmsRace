using UnityEngine;
using System.Collections;

[System.Serializable]
public class ActData
{
  [SerializeField]
  string description;
  public string Description { get {return description; } set { description = value;} }
  
  [SerializeField]
  int actwarplus;
  public int ActWarPlus { get {return actwarplus; } set { actwarplus = value;} }
  
  [SerializeField]
  int actpeaceplus;
  public int ActPeacePlus { get {return actpeaceplus; } set { actpeaceplus = value;} }
  
  [SerializeField]
  int actwarminus;
  public int ActWarMinus { get {return actwarminus; } set { actwarminus = value;} }
  
  [SerializeField]
  int actpeaceminus;
  public int ActPeaceMinus { get {return actpeaceminus; } set { actpeaceminus = value;} }
  
}