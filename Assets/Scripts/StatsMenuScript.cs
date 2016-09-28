using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class StatsMenuScript : MonoBehaviour
{
    public RectTransform StatLists;
    public RectTransform ListUSA;
    public RectTransform ListSU;

    List<CountryScript> cntUSA = new List<CountryScript>(); //страны с американским влиянием
    List<CountryScript> cntSU = new List<CountryScript>();  //страны с советским влиянием

    public void OnEnable()
    {
        SoundManager.SM.PlaySound("sound/stat");

        PlayerScript AmericanPlaer = GameObject.FindGameObjectWithTag("American").GetComponent<PlayerScript>();
        PlayerScript SovietPlaer = GameObject.FindGameObjectWithTag("Soviet").GetComponent<PlayerScript>();

        transform.Find("USGNP").GetComponent<Text>().text = "GNP " + AmericanPlaer.Budget + "+" + AmericanPlaer.Score + " scores";
        transform.Find("SUGNP").GetComponent<Text>().text = "GNP " + SovietPlaer.Budget + "+" + SovietPlaer.Score + " scores";

        cntUSA.Clear();
        cntSU.Clear();

        GameObject Countries = GameObject.Find("Countries");
        for (int i = 0; i < Countries.transform.childCount; i++)
        {
            CountryScript Country = Countries.transform.GetChild(i).GetComponent<CountryScript>();
            if (Country.AmInf > 0)
                cntUSA.Add(Country);
            if (Country.SovInf > 0)
                cntSU.Add(Country);
        }

        //Сортировка списков по очкам страны
        SortByScore("left");
        SortByScore("right");
    }

    void FillList(RectTransform ListOnScreen, List<CountryScript> cntList)
    {
        //Заполняем списки
        int n = 0;
        foreach (CountryScript c in cntList)
        {
            if (ListOnScreen.childCount < n+1)
                break;

            Transform p = ListOnScreen.GetChild(n);
            p.gameObject.SetActive(true);
            p.GetComponent<StatsLineScript>().Init(c);

            n++;
        }

        //Скрываем нижние оставшиеся панельки
        for (int i = n; i < ListOnScreen.childCount; i++)
        {
            ListOnScreen.GetChild(i).gameObject.SetActive(false);
        }

        //Перемещение в верх списка
        ListOnScreen.parent.localPosition = Vector3.zero;
    }

    public void SortByInf(string Side)
    {
        if (Side == "left")
        {
            cntUSA.Sort((a, b) => a.AmInf.CompareTo(b.AmInf));
            cntUSA.Reverse();
            FillList(ListUSA, cntUSA);
        }

        if (Side == "right")
        {
            cntSU.Sort((a, b) => a.SovInf.CompareTo(b.SovInf));
            cntSU.Reverse();
            FillList(ListSU, cntSU);
        }
    }

    public void SortByOpp(string Side)
    {
        if (Side == "left")
        {
            cntUSA.Sort((a, b) => a.Support.CompareTo(b.Support));
            FillList(ListUSA, cntUSA);
        }

        if (Side == "right")
        {
            cntSU.Sort((a, b) => a.Support.CompareTo(b.Support));
            FillList(ListSU, cntSU);
        }
    }

    public void SortByScore(string Side)
    {
        if (Side == "left")
        {
            cntUSA.Sort((a, b) => (a.Authority == Authority.Amer ? a.Score : 0).CompareTo(b.Authority == Authority.Amer?b.Score:0));
            cntUSA.Reverse();
            FillList(ListUSA, cntUSA);
        }

        if (Side == "right")
        {
            cntSU.Sort((a, b) => (a.Authority == Authority.Soviet ? a.Score : 0).CompareTo(b.Authority == Authority.Soviet ? b.Score : 0));
            cntSU.Reverse();
            FillList(ListSU, cntSU);
        }
    }

    public void ScrollStatMenu(int dt)
    {
        SoundManager.SM.PlaySound("sound/click2");

        Vector3 NewPos = StatLists.localPosition + Vector3.up * dt;

        if (NewPos.y < 0)
            NewPos -= Vector3.up * NewPos.y;

        float maxY = StatLists.rect.height - ((RectTransform)StatLists.parent).rect.height;
        if (NewPos.y > maxY)
            NewPos -= Vector3.up * (NewPos.y - maxY);

        StatLists.localPosition = NewPos;
    }
}
