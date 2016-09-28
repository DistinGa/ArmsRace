using UnityEngine;
using System.Collections;

public class FlagButton : MonoBehaviour
{
    public CountryScript Country;

    private CameraScript scCamera;

    void Start()
    {
        scCamera = FindObjectOfType<Camera>().GetComponent<CameraScript>();
        GetComponent<UnityEngine.UI.Image>().sprite = (Country.Authority == Authority.Soviet ? Country.FlagS : Country.FlagNs);
    }

    public void ClickFlag()
    {
        scCamera.SetNewPosition(Country.Capital);
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
