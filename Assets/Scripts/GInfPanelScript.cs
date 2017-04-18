using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GInfPanelScript : MonoBehaviour {
    public Sprite[] Suits;
    public Sprite[] Heads;

    public Image Suit;
    public Image Head;
    public Text LeaderName;
    public Text InfPercent;
    public Text FPBonus;
    public Text InfValue;

    public void SetSuit(int i)
    {
        Suit.sprite = Suits[i];
    }

    public void SetHead(int i)
    {
        Head.sprite = Heads[i];
    }

    public void SetName(string name)
    {
        LeaderName.text = name;
    }

    public void SetInfPercent(int percent)
    {
        InfPercent.text = percent.ToString() + "%";
    }

    public void SetFPBonus(int value)
    {
        FPBonus.text = "+" + value.ToString() + " Firepower";
    }

    public void SetInfValue(int value)
    {
        InfValue.text = value.ToString();
    }

}
