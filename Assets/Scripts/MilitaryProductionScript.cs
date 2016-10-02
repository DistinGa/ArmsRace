using UnityEngine;
using UnityEngine.UI;

public class MilitaryProductionScript : MonoBehaviour
{
    [SerializeField]
    OutlayField field;
    [SerializeField]
    Text production;
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
        GameManagerScript GM = GameManagerScript.GM;
        production.text = "$" + GM.Player.Outlays[field].Outlay + "/" + GM.MILITARY_COST;
        pool.text = GM.Player.MilitaryPool.ToString();
    }
}
