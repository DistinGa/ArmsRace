using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UNMenu : MonoBehaviour {
    GameManagerScript GM_ = null;
    bool initPause;
    CountryScript selectedCountry;
    UNActionType selectedType;
    List<CountryScript> cntrs;

    [SerializeField]
    Button btnSubmit;
    [SerializeField]
    ToggleGroup tgActions;
    [SerializeField]
    Sprite sprGreenPlate, sprRedPlate;  //плашки для цены резолюции
    [SerializeField]
    GameObject iconScore, iconArm, iconInf;
    [SerializeField]
    Transform prestGainList;
    [SerializeField]
    RectTransform scrolCountry;
    [SerializeField]
    Image[] pnlsPrice;
    [SerializeField]
    Text txtPrestige, txtResDescription;
    [SerializeField]
    GameObject prestiggeGainStringPrefab, UNCountrySelectionPrefab;
    [SerializeField]
    Image Herb;
    [SerializeField]
    Sprite iconExpansion, iconMil, iconSpy, HerbUSA, HerbUSSR;

    GameManagerScript GM
    {
        get
        {
            if (GM_ == null)
                GM_ = GameManagerScript.GM;

            return GM_;
        }
    }

    void Start()
    {
        if (GM.Player.Authority == Authority.Amer)
            Herb.sprite = HerbUSA;
        else
            Herb.sprite = HerbUSSR;

        pnlsPrice[(int)UNActionType.CondemnAggressor].GetComponentInChildren<Text>().text = GM.DLC_UN.GetCondemnAggressorCost.ToString();
        pnlsPrice[(int)UNActionType.Peacekeeping].GetComponentInChildren<Text>().text = GM.DLC_UN.GetPeacekeepingCost.ToString();
        pnlsPrice[(int)UNActionType.Intervention].GetComponentInChildren<Text>().text = GM.DLC_UN.GetInterventionCost.ToString();
        pnlsPrice[(int)UNActionType.Speech].GetComponentInChildren<Text>().text = GM.DLC_UN.GetSpeechCost.ToString();
    }

    void OnEnable()
    {
        initPause = GM.IsPaused;
        GM.IsPaused = true;

        foreach (var item in tgActions.GetComponentsInChildren<Toggle>())
        {
            item.isOn = false;
        }

        selectedType = UNActionType.None;
        txtResDescription.text = "";
        UpdateView();
    }

    void OnDisable()
    {
        GM.IsPaused = initPause;
    }

    public void UpdateView()
    {
        txtPrestige.text = GM.DLC_UN.GetPrestige(GM.Player.Authority).ToString();

        //Отметка доступных резолюций
        pnlsPrice[(int)UNActionType.CondemnAggressor].sprite = GM.DLC_UN.CheckCondemnCondition(GM.Player.Authority) ? sprGreenPlate: sprRedPlate;
        pnlsPrice[(int)UNActionType.Peacekeeping].sprite = GM.DLC_UN.CheckPeacemakingCondition(GM.Player.Authority) ? sprGreenPlate : sprRedPlate;
        pnlsPrice[(int)UNActionType.Intervention].sprite = GM.DLC_UN.CheckInterventionCondition(GM.Player.Authority) ? sprGreenPlate : sprRedPlate;
        pnlsPrice[(int)UNActionType.Speech].sprite = GM.DLC_UN.CheckSpeechCondition(GM.Player.Authority) ? sprGreenPlate : sprRedPlate;

        //Выделение кнопки активной резолюции
        switch (selectedType)
        {
            case UNActionType.CondemnAggressor:
                SetCondemnAction(true);
                break;
            case UNActionType.Peacekeeping:
                SetPeecekeepingAction(true);
                break;
            case UNActionType.Intervention:
                SetInterventionAction(true);
                break;
            case UNActionType.Speech:
                SetSpeechAction(true);
                break;
            default:
                btnSubmit.interactable = false;
                foreach (var item in tgActions.GetComponentsInChildren<Toggle>())
                    item.isOn = false;
                break;
        }

        //Иконки месячных повышений престижа
        int suprScore, suprInf, suprArma;
        GM.DLC_UN.CheckMonSupremacy(out suprScore, out suprInf, out suprArma);
        iconScore.SetActive(suprScore > 0);
        iconArm.SetActive(suprArma > 0);
        iconInf.SetActive(suprInf > 0);
    }

    public void SetCondemnAction(bool vl)
    {
        if (vl)
        {
            selectedType = UNActionType.CondemnAggressor;
            //btnSubmit.interactable = GM.DLC_UN.CheckCondemnCondition(GM.Player.Authority);
            bool interact = GM.DLC_UN.CheckCondemnCondition(GM.Player.Authority);
            if (interact)
            {
                interact = false;
                foreach (CountryScript c in CountryScript.Countries())
                {
                    if (c.AggressorAuth != GM.Player.Authority && c.AggressorAuth != Authority.Neutral)
                    {
                        interact = true;
                        break;
                    }
                }
            }

            btnSubmit.interactable = interact;
            txtResDescription.text = string.Format("-1 global influence for {0}", GM.AI.AIPlayer.MyCountry.Name);
            List<CountryScript> tmpLst = new List<CountryScript>();
            tmpLst.Add(GM.Player.OppCountry);
            FillFlagList(tmpLst);
        }
    }

    public void SetPeecekeepingAction(bool vl)
    {
        if (vl)
        {
            selectedType = UNActionType.Peacekeeping;
            btnSubmit.interactable = (FillFlagList(GM.DLC_UN.GetPeacemakingCountries()) && GM.DLC_UN.CheckPeacemakingCondition(GM.Player.Authority));
            txtResDescription.text = string.Format("50/50 for opposition and support in <country>");
        }
    }

    public void SetInterventionAction(bool vl)
    {
        if (vl)
        {
            selectedType = UNActionType.Intervention;
            btnSubmit.interactable = (FillFlagList(GM.DLC_UN.GetInterventionCountries()) && GM.DLC_UN.CheckInterventionCondition(GM.Player.Authority));
            txtResDescription.text = string.Format("+3 military and opposition =90 in <country>");
        }
    }

    public void SetSpeechAction(bool vl)
    {
        if (vl)
        {
            selectedType = UNActionType.Speech;
            btnSubmit.interactable = GM.DLC_UN.CheckSpeechCondition(GM.Player.Authority);
            txtResDescription.text = string.Format("+1 global influence for {0}", GM.Player.MyCountry.Name);
            List<CountryScript> tmpLst = new List<CountryScript>();
            tmpLst.Add(GM.Player.MyCountry);
            FillFlagList(tmpLst);
        }
    }

    bool FillFlagList(List<CountryScript> lstCountries)
    {
        cntrs = lstCountries;

        for (int i = scrolCountry.childCount - 1; i >= 0; i--)
        {
            Destroy(scrolCountry.GetChild(i).gameObject);
        }

        foreach (var c in cntrs)
        {
            GameObject newCntr = Instantiate(UNCountrySelectionPrefab);
            newCntr.transform.SetParent(scrolCountry);
            newCntr.transform.GetComponentInChildren<Image>().sprite = c.Flag;
        }

        //Позиционирование на выбранную страну (или на первую, если такой страны нет в списке).
        //selectedCountry = cntrs.Find(x => x.Name == selectedCountry.Name);
        //selectedCountry = cntrs.Find(x => x == selectedCountry);
        //if (selectedCountry == null && cntrs.Count > 0)
        //    selectedCountry = cntrs[0];
        int indx = cntrs.FindIndex(x => x == selectedCountry);
        if (indx < 0)   //не нашлось
            indx = 0;

        FlagPositioning(indx);

        return cntrs.Count > 0;
    }

    public void AddPrestGainString(CountryScript c, int month, UN.SingleGainType tp)
    {
        if (prestGainList.childCount == GM.DLC_UN.PrestGainsCount)
        {
            DestroyImmediate(prestGainList.GetChild(GM.DLC_UN.PrestGainsCount - 1).gameObject);
        }

        GameObject newString = Instantiate(prestiggeGainStringPrefab);
        newString.transform.SetParent(prestGainList);
        newString.transform.SetAsFirstSibling();
        newString.transform.FindChild("Flag").GetComponent<Image>().sprite = c.Flag;
        newString.transform.FindChild("Date").GetComponent<Text>().text = GM.GetDateString(month);
        switch (tp)
        {
            case UN.SingleGainType.AlliExpansion:
                newString.transform.FindChild("Icon").GetComponent<Image>().sprite = iconExpansion;
                newString.transform.FindChild("Gain").GetComponent<Text>().text = "+" + GM.DLC_UN.AlliPrestGain.ToString();
                break;
            case UN.SingleGainType.MilLiquidation:
                newString.transform.FindChild("Icon").GetComponent<Image>().sprite = iconMil;
                newString.transform.FindChild("Gain").GetComponent<Text>().text = "+" + GM.DLC_UN.MilPrestGain.ToString();
                break;
            case UN.SingleGainType.SpyLiquidation:
                newString.transform.FindChild("Icon").GetComponent<Image>().sprite = iconSpy;
                newString.transform.FindChild("Gain").GetComponent<Text>().text = "+" + GM.DLC_UN.SpyPrestGain.ToString();
                break;
        }
    }

    void FlagPositioning(int indx)
    {
        const int flagSize = 75;

        if (cntrs == null || cntrs.Count == 0)
            return;

        selectedCountry = cntrs[indx];
        scrolCountry.anchoredPosition = new Vector2(-indx * flagSize, scrolCountry.anchoredPosition.y);
        GM.SnapToCountry(selectedCountry);
    }

    //Перемещение линии флагов, если right - вправо, иначе -влево.
    public void MoveFlags(bool right)
    {
        int indx = cntrs.FindIndex(x => x == selectedCountry);
        indx += right ? 1 : -1;
        if(indx >= 0 && indx < cntrs.Count)
            FlagPositioning(indx);
    }

    public void SubmitAction()
    {
        switch (selectedType)
        {
            case UNActionType.CondemnAggressor:
                GM.DLC_UN.CondemnAggressor(GM.Player.Authority, GM.AI.AIPlayer.Authority);
                break;
            case UNActionType.Peacekeeping:
                GM.DLC_UN.Peacekeeping(GM.Player.Authority, selectedCountry);
                break;
            case UNActionType.Intervention:
                GM.DLC_UN.Intervention(GM.Player.Authority, selectedCountry);
                break;
            case UNActionType.Speech:
                GM.DLC_UN.Speech(GM.Player.Authority);
                break;
        }

        selectedType = UNActionType.None;
        selectedCountry = null;
        //UpdateView();
        SoundManager.SM.PlaySound("sound/UN-submit");
        GM.ToggleTechMenu(gameObject);
        FindObjectOfType<CameraScriptXZ>().setOverMenu = false;
    }

    enum UNActionType
    {
        CondemnAggressor,
        Peacekeeping,
        Intervention,
        Speech,
        None
    }
}
