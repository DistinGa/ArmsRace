using UnityEngine;
using UnityEngine.UI;

//Класс для пометки переключателей, как неактивных, при выборе режима загрузки.
public class MarkToggle : MonoBehaviour {

    public void SetToggleActive(bool b)
    {
        Toggle tg = GetComponent<Toggle>();
        tg.interactable = b;
        if(tg.isOn)
            transform.FindChild("Background").gameObject.SetActive(b);
        //tg.isOn = !tg.isOn;
        //tg.isOn = !tg.isOn;
    }
}
