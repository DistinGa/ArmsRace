using UnityEngine;
using System.Collections;

//Класс для хранения бонусов лидеров
[CreateAssetMenu(fileName = "LeaderBonuses", menuName = "Create leader bonuses object", order = 0)]
public class SOLP : ScriptableObject
{
    //Leader bonuses
    [Tooltip("Бонус к fire power")]
    public int FPBonus = 5;
    public string descriptionFPBonus = "+5 to Air, Ground and NAVY firepower";

    [Space(10)]
    [Tooltip("Скидка на стоимость изучения космических технологий")]
    public float SpaceDiscount = 0.05f;
    public string descriptionSpace = "Space technology cost -5%";

    [Space(10)]
    [Tooltip("Ежегодная прибавка дипломатов и шпионов (количество)")]
    public int AnnualFreeDipSpy = 2;
    public string descriptionFDS = "+2 spies and +2 diplomats production yearly";

    [Space(10)]
    [Tooltip("Изменение влияния за одного дипломата")]
    public int InfluenceBoost = 2;
    public string descriptionIB = "One diplomat influence action increase by one ( two in total )";

    //LeaderType bonuses
    [Space(10)]
    [Tooltip("Скидка на стоимость изучения военных технологий")]
    public float MilTechDiscount = 0.05f;
    public string descriptionMilDisc = "Military technology cost -5%";

    [Space(10)]
    [Tooltip("Ежегодная прибавка military")]
    public int AnnualFreeMil = 1;
    public string descriptionFMil = "+1 to military production yearly";

    [Space(10)]
    [Tooltip("Скидка на стоимость дипломата")]
    public float DipDiscount = 0.2f;
    public string descriptionDipDisc = "Diplomats production cost -20%";

    [Space(10)]
    [Tooltip("Скидка на стоимость шпиона")]
    public float SpyDiscount = 0.2f;
    public string descriptionSpyDisc = "Spies production cost -20%";

    [Space(10)]
    [Tooltip("Изменение поддержки/оппозиции от одного шпиона")]
    public int MeetingBoost = 2;
    public string descriptionMB = "One spy riot and parade action efficiency increased by one ( two in total )";

    [Space(10)]
    [Tooltip("Изменения глобальных последствий в 10 лет")]
    public int TenYearsGlobalEffectsChange = 1;
    public string descriptionGEC = "+1 to global consequences changes per 10 years";

    //DLC
    [Header("===DLC===")]
    [Space(10)]
    [Tooltip("Скидка на миссии ООН")]
    public float UNMissionDiscount = 0;
    [Tooltip("Скидка на ядерное оружие")]
    public float NuclearDiscount = 0;
    [Tooltip("Ежемесячная прибавка престижа")]
    public int MonthlyPrestigePlus = 0;
    [Space(10)]
    [Tooltip("Скидка на индустриальные контракты")]
    public int IndustryDiscount = 0;  //на сколько уменьшаем время постройки по факту
    public string descriptionIndustr = "Industry construction -20%";
}
