using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GlobalEffects;

public class GEPanel : MonoBehaviour {
    public GlobalEffectObject GEobj;

    public Image background;
    public Image usabar;
    public Image sovbar;
    public Image Image;
    public Image Title;
    public Text description;
    public Text usaGPP;
    public Text sovGPP;
    public Text usaGPPcounter;
    public Text sovGPPcounter;

    public Sprite offHighlight;
    public Sprite usaHighlight;
    public Sprite sovHighlight;
    public Sprite usaDonePicture;
    public Sprite sovDonePicture;
    public Sprite notActiveBackGround;
    public Sprite activeBackGround;

    //Полная прорисовка панели
    public void DisplayEvent (GlobalEffectObject GlobalEffectObj, bool active, bool updateDescription = false)
    {
        GEobj = GlobalEffectObj;

        Image.raycastTarget = active;

        //background
        if (active)
        {
            if (GEobj.GEDone)
            {
                if (GEobj.sovGPP > GEobj.usaGPP)
                    background.sprite = sovDonePicture;
                else
                    background.sprite = usaDonePicture;
            }
            else
            {
                background.sprite = activeBackGround;
            }

            Title.sprite = GEobj.picture;
        }
        else
        {
            background.sprite = notActiveBackGround;
            Title.sprite = GEobj.offPicture;
        }

        //отображение описания
        if (updateDescription)
            SetDescription(GEobj.counter);

        usabar.fillAmount = (float)GEobj.usaGPP / GEobj.usaGPPLimit;
        sovbar.fillAmount = (float)GEobj.sovGPP / GEobj.sovGPPLimit;

        usaGPP.text = GEobj.usaGPP.ToString() + "/" + GEobj.usaGPPLimit.ToString();
        sovGPP.text = GEobj.sovGPP.ToString() + "/" + GEobj.sovGPPLimit.ToString();

        if (GEobj.counter > 0)
        {
            usaGPPcounter.text = "0";
            sovGPPcounter.text = "+" + GEobj.counter.ToString();
        }
        else if (GEobj.counter < 0)
        {
            usaGPPcounter.text = "+" + (-GEobj.counter).ToString();
            sovGPPcounter.text = "0";
        }
        else
        {
            usaGPPcounter.text = "0";
            sovGPPcounter.text = "0";
        }

    }

    public void SetDescription(int side)
    {
        if (side > 0)
        {
            //Image.sprite = sovHighlight;
            description.text = GEobj.sovDescription;
        }
        else if(side < 0)
        {
            //Image.sprite = usaHighlight;
            description.text = GEobj.usaDescription;
        }
        else
        {
            //Image.sprite = offHighlight;
            description.text = "";
        }
    }

    public void HighLightTitle(bool on)
    {
        PlayerScript player = GameManagerScript.GM.Player;

        if (on && player.TYGEDiscounter > 0)
        {
            if (player.Authority == Authority.Amer)
                Image.sprite = usaHighlight;
            else
                Image.sprite = sovHighlight;
        }
        else
        {
            Image.sprite = offHighlight;
        }
    }

    public void ApplyGPPLeaderBonus()
    {
        GEobj.ChangeGPPCounter();
        GlobalEffectsManager.GeM.Menu.UpdateView();
    }
}
