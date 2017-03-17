using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using GlobalEffects;

public class GlobalEffectsMenu : MonoBehaviour {

    public GlobalEffectsManager GEM;
    public List<RectTransform> Toggles;

    int displayedDecade;

    void OnEnable()
    {
        displayedDecade = GameManagerScript.GM.CurrentMonth / 120;

        for (int i = 0; i < Toggles.Count; i++)
        {
            Toggles[i].GetComponent<Toggle>().isOn = false;
        }
        Toggles[displayedDecade].GetComponent<Toggle>().isOn = true;

        UpdateView(true);
    }

    public void UpdateView(bool updateDescription = false)
    {
        GameManagerScript GM = GameManagerScript.GM;

        GEPanel[] eventsPanels = GetComponentsInChildren<GEPanel>();
        GlobalEffectObject[] geO = GEM.GetDecadeGEs(displayedDecade);
        for(int i = 0; i < geO.Length; i++)
        {
            eventsPanels[i].DisplayEvent(geO[i], displayedDecade == (GM.CurrentMonth / 120), updateDescription);
        }

        //счётчик изменений GPP
        transform.Find("TimeLine/ActionCount/Text").GetComponent<Text>().text = GM.Player.TYGEDiscounter.ToString();

    }

    //отрабатывает при нажатии на нижние кнопки
    public void ChangeDecade(int dec)
    {
        if (Toggles[dec].GetComponent<Toggle>().isOn == false)
            return;

        displayedDecade = dec;

        for (int i = 0; i < Toggles.Count; i++)
        {
            Toggles[i].GetComponent<Image>().enabled = (i != dec);
        }

        UpdateView();
    }
}
