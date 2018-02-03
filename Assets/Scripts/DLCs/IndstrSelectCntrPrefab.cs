using UnityEngine;
using UnityEngine.UI;

public class IndstrSelectCntrPrefab : MonoBehaviour {
    //[SerializeField]
    public Text TextComponent;
    public CountryScript cntr;

	public void DoAct()
    {
        GameManagerScript.GM.DLC_Industrialisation.IndustrMenu.SelectCountry(this);
	}

    public CountryScript Country
    {
        get { return cntr; }

        set
        {
            cntr = value;
            TextComponent.text = value.Name;
        }
    }
}
