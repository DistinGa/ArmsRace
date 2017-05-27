using UnityEngine;
using UnityEngine.UI;

public class CountryLabelScript : MonoBehaviour {
    public Text CountryName;

    public void SetName(string n)
    {
        CountryName.text = n;
    }
}
