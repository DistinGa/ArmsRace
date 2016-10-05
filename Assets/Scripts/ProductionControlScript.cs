using UnityEngine;
using UnityEngine.UI;

public class ProductionControlScript : MonoBehaviour
{
    [SerializeField]
    OutlayField field;
    [SerializeField]
    Text production;
    [SerializeField]
    Text Outlay;
    [SerializeField]
    Text pool;

    public void Start()
    {
        UpdateProduction();
    }

    public void ChangeOutlet(int amount)
    {
        GameManagerScript.GM.Player.Outlays[field].ChangeOutlet(amount);
        UpdateProduction();
    }

    void UpdateProduction()
    {
        PlayerScript Player = GameManagerScript.GM.Player;
        Outlay.text = "$" + Player.Outlays[field].Outlay;
        production.text = "$" + Player.Outlays[field].Budget + "/" + Player.Outlays[field].Cost;
        if(pool != null)
            pool.text = Player.MilitaryPool.ToString();
    }
}
