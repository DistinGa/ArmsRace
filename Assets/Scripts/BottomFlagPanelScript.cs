using UnityEngine;
using UnityEngine.UI;

public class BottomFlagPanelScript : MonoBehaviour {
    [SerializeField]
    CountryScript country;
    [SerializeField]
    Image fImage;
    [SerializeField]
    Sprite USAFlag, USSRFlag;

    void Update () {
        if (fImage != null)
        {
            if (country.Authority == Authority.Soviet)
                fImage.sprite = USSRFlag;
            else
                fImage.sprite = USAFlag;
        }
    }
}
