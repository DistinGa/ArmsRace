using UnityEngine;
using System.Collections;

public class FlagButton : MonoBehaviour
{
    public CountryScript Country;

    private CameraScriptXZ scCamera;

    void Start()
    {
        scCamera = FindObjectOfType<CameraScriptXZ>();
        GetComponent<UnityEngine.UI.Image>().sprite = (Country.Authority == Authority.Soviet ? Country.FlagS : Country.FlagNs);
    }

    public void ClickFlag()
    {
        GameManagerScript.GM.SnapToCountry(Country);
    }

    public void OnMouseEnter()
    {
        scCamera.setOverMenu = true;
    }

    public void OnMouseExit()
    {
        scCamera.setOverMenu = false;
    }
}
