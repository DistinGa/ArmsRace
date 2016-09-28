using UnityEngine;
using UnityEngine.UI;

public class StatePrefab : MonoBehaviour {
    public Sprite[] spriteAmer;
    public Sprite[] spriteSov;


    public void Init(int sprIndx, Authority authority)
    {
        if(authority == Authority.Amer)
            GetComponent<Image>().sprite = spriteAmer[sprIndx];
        if(authority == Authority.Soviet)
            GetComponent<Image>().sprite = spriteSov[sprIndx];
    }
}
