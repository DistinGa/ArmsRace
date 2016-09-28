using UnityEngine;
using System.Collections.Generic;

public class AI : MonoBehaviour {
    public PlayerScript AIPlayer;

    void Start ()
    {
        if (GameManagerScript.GM.AI == null)
            Destroy(gameObject);    //Играем против другого игрока, АИ не нужен

        //Назначаем сторону AI в зависимости от выбранной стороны игрока
        if (SettingsScript.Settings.playerSelected == Authority.Amer)
            AIPlayer = GameObject.Find("SovPlayer").GetComponent<PlayerScript>();
        else
            AIPlayer = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>();
    }

    // ход AI
    public void AIturn()
    {
    }

}
