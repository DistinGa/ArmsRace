using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class IndustrMenu : MonoBehaviour {
    [SerializeField]
    Text State1, State2, Info, Score1, Score2;
    [SerializeField]
    GameObject SelectCountryPrefab;
    [SerializeField]
    RectTransform Scroll1, Scroll2, Content1, Content2;
    [SerializeField]
    RectTransform btnContract, progress, indstrTypeToggles;
    [SerializeField]
    Button btnSelectCountry1, btnSelectCountry2;
    [SerializeField]
    IndustrDoneContractsPrefab IndustrDoneContractsPrefab;
    [SerializeField]
    RectTransform DoneContractsList;
    [SerializeField]
    Sprite[] TypeImages;

    CountryScript cntr1, cntr2;
    IndustryType indstrType = IndustryType.Diplomat;

    void OnEnable() {
        Scroll1.gameObject.SetActive(false);
        Scroll2.gameObject.SetActive(false);
        
        UpdateView();
    }

    public void UpdateView()
    {
        IndustryElement curIndustr = GameManagerScript.GM.DLC_Industrialisation.GetCurIndustr(GameManagerScript.GM.Player);
        if (curIndustr != null)
        {
            State1.text = curIndustr.Cntr1.Name;
            Score1.text = curIndustr.Cntr1.Score.ToString();
            State2.text = curIndustr.Cntr2.Name;
            Score2.text = curIndustr.Cntr2.Score.ToString();
            Info.text = string.Format("{0} + {1} = ${2} per month * {3} months = {4} in total",
                curIndustr.Cntr1.Score,
                curIndustr.Cntr2.Score,
                (curIndustr.Cntr1.Score + curIndustr.Cntr2.Score),
                GameManagerScript.GM.DLC_Industrialisation.IndustrCount,
                (curIndustr.Cntr1.Score + curIndustr.Cntr2.Score) * GameManagerScript.GM.DLC_Industrialisation.IndustrCount);

            btnContract.gameObject.SetActive(false);
            foreach (var item in indstrTypeToggles.GetComponentsInChildren<Toggle>())
            {
                item.isOn = (item.GetComponent<RectTransform>().GetSiblingIndex() == (int)curIndustr.IndustryType);
                item.interactable = false;
            }

            progress.GetComponent<Image>().fillAmount = (1 - (float)curIndustr.CountDown / GameManagerScript.GM.DLC_Industrialisation.IndustrCount);
            btnSelectCountry1.interactable = false;
            btnSelectCountry2.interactable = false;
        }
        else
        {
            State1.text = "";
            Score1.text = "";
            State2.text = "";
            Score2.text = "";
            Info.text = "";
            btnContract.gameObject.SetActive(true);
            foreach (var item in indstrTypeToggles.GetComponentsInChildren<Toggle>())
            {
                item.isOn = false;
                item.interactable = true;
            }
            btnSelectCountry1.interactable = true;
            btnSelectCountry2.interactable = true;

        }
    }

    public void SelectCountry(IndstrSelectCntrPrefab listString)
    {
        if (listString.GetComponent<RectTransform>().IsChildOf(Scroll1))
        {
            State1.text = listString.TextComponent.text;
            Score1.text = listString.Country.Score.ToString();
            cntr1 = listString.Country;
            Scroll1.gameObject.SetActive(false);
        }
        else
        {
            State2.text = listString.TextComponent.text;
            Score2.text = listString.Country.Score.ToString();
            cntr2 = listString.Country;
            Scroll2.gameObject.SetActive(false);
        }

        int score1 = cntr1 != null ? cntr1.Score : 0;
        int score2 = cntr2 != null ? cntr2.Score : 0;
        Info.text = string.Format("{0} + {1} = ${2} per month * {3} months = {4} in total",
            score1,
            score2,
            (score1 + score2),
            GameManagerScript.GM.DLC_Industrialisation.IndustrCount,
            (score1 + score2) * GameManagerScript.GM.DLC_Industrialisation.IndustrCount);
    }

    public void ShowScroll(int ScrollIndx)
    {
        if (ScrollIndx == 1)
        {
            Scroll2.gameObject.SetActive(false);

            while (Content1.childCount > 0)
            {
                DestroyImmediate(Content1.GetChild(0).gameObject);
            }

            Scroll1.gameObject.SetActive(true);
            List<CountryScript> CountryList = GameManagerScript.GM.DLC_Industrialisation.GetAvailableCountriesPrimary(GameManagerScript.GM.Player.Authority, cntr2);

            GameObject newString;
            foreach (var item in CountryList)
            {
                newString = Instantiate(SelectCountryPrefab);
                newString.transform.SetParent(Content1);
                newString.GetComponent<IndstrSelectCntrPrefab>().Country = item;
            }
        }
        else
        {
            Scroll1.gameObject.SetActive(false);

            while (Content2.childCount > 0)
            {
                DestroyImmediate(Content2.GetChild(0).gameObject);
            }

            Scroll2.gameObject.SetActive(true);
            List<CountryScript> CountryList = GameManagerScript.GM.DLC_Industrialisation.GetAvailableCountriesSecondary(GameManagerScript.GM.Player.Authority, cntr1);

            GameObject newString;
            foreach (var item in CountryList)
            {
                newString = Instantiate(SelectCountryPrefab);
                newString.transform.SetParent(Content2);
                newString.GetComponent<IndstrSelectCntrPrefab>().Country = item;
            }
        }
    }

    public void SetDiplomatType(bool val)
    {
        if(val)
            indstrType = IndustryType.Diplomat;
    }

    public void SetMilitaryType(bool val)
    {
        if (val)
            indstrType = IndustryType.Military;
    }

    public void SetSpyType(bool val)
    {
        if (val)
            indstrType = IndustryType.Spy;
    }

    public void StartContract()
    {
        if (GameManagerScript.GM.DLC_Industrialisation.StartIndustrialization(cntr1, cntr2, indstrType, GameManagerScript.GM.Player))
        {
            UpdateView();
        }
        else
        {
            SoundManager.SM.PlaySound("sound/ots4et");
        }
    }

    public IndustrDoneContractsPrefab AddDoneContract(IndustryElement item)
    {
        IndustrDoneContractsPrefab newDoneContract = Instantiate(IndustrDoneContractsPrefab);
        newDoneContract.transform.SetParent(DoneContractsList);
        newDoneContract.Cntr1.text = item.Cntr1.Name;
        newDoneContract.Cntr2.text = item.Cntr2.Name;
        newDoneContract.CntrScore1.text = item.Cntr1.Score.ToString();
        newDoneContract.CntrScore2.text = item.Cntr2.Score.ToString();
        newDoneContract.TypeImage.sprite = TypeImages[(int)item.IndustryType];

        return newDoneContract;
    }
}
