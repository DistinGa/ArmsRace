using UnityEngine;
using UnityEngine.UI;

public class ProductionControlScript : MonoBehaviour
{
    [SerializeField]
    OutlayField field;  //тип расходов
    [SerializeField]
    Text production;    //поле прогресса производства юнита или изучения технологии
    [SerializeField]
    Text Outlay;        //поле трат
    [SerializeField]
    Text pool;          //количество юнитов для air, ground и sea

    public void Start()
    {
        UpdateProduction();
    }

    public void OnEnable()
    {
        GameManagerScript.GM.SubscribeMonth(new dlgtMonthTick(UpdateProduction));
    }

    public void OnDisable()
    {
        GameManagerScript.GM.UnsubscribeMonth(new dlgtMonthTick(UpdateProduction));
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
        if (pool != null)
            pool.text = Player.MilitaryPool.ToString();
    }

}
