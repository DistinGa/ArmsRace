using UnityEngine;
using System.Collections.Generic;

namespace GlobalEffects
{
    [CreateAssetMenu(fileName = "GlobalEffectsPrefs", menuName = "Create GlobalEffectsPrefs object", order = 0)]
    public class SOGlobalEffectsPrefs : ScriptableObject
    {
        public GEPref[] GlobalEffectsPreferences = new GEPref[30];
    }

    [System.Serializable]
    public class GEPref
    {
        public string eventName;
        public string sovDescription;
        public string usaDescription;
        public Sprite picture;  //картинка на панели в меню глобальныхх последствий
        public Sprite icon;     //картинка на кнопке в верхнем меню
        public int sovGPPLimit, usaGPPLimit;
        public List<GEEvent> sovEvents, usaEvents;

        public void InvokeSovEvent()
        {
            foreach (var item in sovEvents)
            {
                item.ApplyEffect();
            }
        }

        public void InvokeUsaEvent()
        {
            foreach (var item in usaEvents)
            {
                item.ApplyEffect();
            }
        }
    }

    [System.Serializable]
    public class GEEvent
    {
        public GETypes GEType;
        public int Amount;
        public Authority AllianceAction;
        public CountryScript Country;
        public Region Region;
        public GEFields Field;
        public bool IfInAllianceOnly;
        public Authority AllianceCondition;

        public void ApplyEffect()
        {
            switch (GEType)
            {
                //изменение оппозиции
                case GETypes.OppositionChange:
                    switch (Field)
                    {
                        case GEFields.SingleCountry:
                            if(!IfInAllianceOnly || (Country.Authority == AllianceCondition))
                                Country.Support -= Amount;
                            break;
                        case GEFields.AllianceCountries:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                                c.Support -= Amount;
                            break;
                        case GEFields.AllianceCountriesInRegion:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                            {
                                if(c.Region == Region)
                                    c.Support -= Amount;
                            }
                            break;
                        case GEFields.Global:
                            foreach (var c in CountryScript.Countries())
                            {
                                if (!IfInAllianceOnly || (c.Authority == AllianceCondition))
                                    c.Support -= Amount;
                            }
                            break;
                    }
                    break;
                //установка оппозиции
                case GETypes.OppositionSet:
                    switch (Field)
                    {
                        case GEFields.SingleCountry:
                            if (!IfInAllianceOnly || (Country.Authority == AllianceCondition))
                            {
                                if (Country.Support > (100 - Amount))
                                    Country.Support = (100 - Amount);
                            }
                            break;
                        case GEFields.AllianceCountries:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                            {
                                if (Country.Support > (100 - Amount))
                                    c.Support = (100 - Amount);
                            }
                            break;
                        case GEFields.AllianceCountriesInRegion:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                            {
                                if (c.Region == Region && Country.Support > (100 - Amount))
                                    c.Support = (100 - Amount);
                            }
                            break;
                        case GEFields.Global:
                            foreach (var c in CountryScript.Countries())
                            {
                                if (!IfInAllianceOnly || (c.Authority == AllianceCondition))
                                {
                                    if (Country.Support > (100 - Amount))
                                        c.Support = (100 - Amount);
                                }
                            }
                            break;
                    }
                    break;
                //изменение влияния
                case GETypes.InfluenceChange:
                    switch (Field)
                    {
                        case GEFields.SingleCountry:
                            if (!IfInAllianceOnly || (Country.Authority == AllianceCondition))
                                Country.AddInfluence(AllianceAction, Amount);
                            break;
                        case GEFields.AllianceCountries:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                                c.AddInfluence(AllianceAction, Amount);
                            break;
                        case GEFields.AllianceCountriesInRegion:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                            {
                                if (c.Region == Region)
                                    c.AddInfluence(AllianceAction, Amount);
                            }
                            break;
                        case GEFields.Global:
                            foreach (var c in CountryScript.Countries())
                                if (!IfInAllianceOnly || (c.Authority == AllianceCondition))
                                    c.AddInfluence(AllianceAction, Amount);
                            break;
                    }
                    break;
                //установка влияния
                case GETypes.InfluenceSet:
                    switch (Field)
                    {
                        case GEFields.SingleCountry:
                            if (!IfInAllianceOnly || (Country.Authority == AllianceCondition))
                                Country.AddInfluence(AllianceAction, Amount - Country.GetInfluense(AllianceAction));
                            break;
                        case GEFields.AllianceCountries:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                                c.AddInfluence(AllianceAction, Amount - c.GetInfluense(AllianceAction));
                            break;
                        case GEFields.AllianceCountriesInRegion:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                            {
                                if (c.Region == Region)
                                    c.AddInfluence(AllianceAction, Amount - c.GetInfluense(AllianceAction));
                            }
                            break;
                        case GEFields.Global:
                            foreach (var c in CountryScript.Countries())
                                if (!IfInAllianceOnly || (c.Authority == AllianceCondition))
                                    c.AddInfluence(AllianceAction, Amount - c.GetInfluense(AllianceAction));
                            break;
                    }
                    break;
                //добавление военных
                case GETypes.MilitaryIncrease:
                    switch (Field)
                    {
                        case GEFields.SingleCountry:
                            if (!IfInAllianceOnly || (Country.Authority == AllianceCondition))
                                Country.AddMilitary(AllianceAction, Amount, true);
                            break;
                        case GEFields.AllianceCountries:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                                c.AddMilitary(AllianceAction, Amount, true);
                            break;
                        case GEFields.AllianceCountriesInRegion:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                            {
                                if (c.Region == Region)
                                    c.AddMilitary(AllianceAction, Amount, true);
                            }
                            break;
                        case GEFields.Global:
                            foreach (var c in CountryScript.Countries())
                                if (!IfInAllianceOnly || (c.Authority == AllianceCondition))
                                    c.AddMilitary(AllianceAction, Amount, true);
                            break;
                    }
                    break;
                //добавление военных до определённого числа
                case GETypes.MilitarySet:
                    switch (Field)
                    {
                        case GEFields.SingleCountry:
                            if (!IfInAllianceOnly || (Country.Authority == AllianceCondition))
                            {
                                if (Country.GetMilitary(AllianceAction) < Amount)
                                    Country.AddMilitary(AllianceAction, Amount - Country.GetMilitary(AllianceAction), true);
                            }
                            break;
                        case GEFields.AllianceCountries:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                            {
                                if (c.GetMilitary(AllianceAction) < Amount)
                                    c.AddMilitary(AllianceAction, Amount - c.GetMilitary(AllianceAction), true);
                            }
                            break;
                        case GEFields.AllianceCountriesInRegion:
                            foreach (var c in CountryScript.Countries(AllianceCondition))
                            {
                                if (c.Region == Region)
                                {
                                    if (c.GetMilitary(AllianceAction) < Amount)
                                        c.AddMilitary(AllianceAction, Amount - c.GetMilitary(AllianceAction), true);
                                }
                            }
                            break;
                        case GEFields.Global:
                            foreach (var c in CountryScript.Countries())
                            {
                                if (!IfInAllianceOnly || (c.Authority == AllianceCondition))
                                {
                                    if (c.GetMilitary(AllianceAction) < Amount)
                                        c.AddMilitary(AllianceAction, Amount - c.GetMilitary(AllianceAction), true);
                                }
                            }
                            break;
                    }
                    break;
                //установка определённого количества шпионов в конкретной стране
                case GETypes.SpiesSet:
                    if (!IfInAllianceOnly || (Country.Authority == AllianceCondition))
                    {
                        if (Country.SpyCount(AllianceAction) < Amount)
                            Country.AddSpy(AllianceAction, Amount - Country.SpyCount(AllianceAction), true);
                    }
                    break;
                //добавление шпионов в пул
                case GETypes.SpiesAddToPool:
                    GameManagerScript.GM.GetPlayerByAuthority(AllianceAction).SpyPool += Amount;
                    break;
                //добавление дипломатов в пул
                case GETypes.DiplomatsAddToPool:
                    GameManagerScript.GM.GetPlayerByAuthority(AllianceAction).DiplomatPool += Amount;
                    break;
                //добавление военных в пул
                case GETypes.MilitaryAddToPool:
                    GameManagerScript.GM.GetPlayerByAuthority(AllianceAction).MilitaryPool += Amount;
                    break;
                case GETypes.ChangeGovernment:
                    if(Country.Authority != AllianceAction)
                        GameManagerScript.GM.ChangeGovernment(Country, AllianceAction, false, false);
                    break;
                case GETypes.FirePowerAddition:
                    GameManagerScript.GM.GetPlayerByAuthority(AllianceAction).fpBonus += Amount;
                    break;
                //изменение бюджета страны
                case GETypes.GNPchange:
                    GameManagerScript.GM.GetPlayerByAuthority(AllianceAction).Budget += Amount;
                    break;
                case GETypes.SpaceDiscount:
                    GameManagerScript.GM.GetPlayerByAuthority(AllianceAction).SpaceDiscount += Amount;
                    break;
                case GETypes.ScoreAddition:
                    GameManagerScript.GM.GetPlayerByAuthority(AllianceAction).ScoreBonus += Amount;
                    break;
                case GETypes.Crisis:
                    GameManagerScript.GM.GetPlayerByAuthority(AllianceAction).MakeCrisis();
                    break;
            }
        }

        public enum GEFields
        {
            SingleCountry,
            AllianceCountries,
            AllianceCountriesInRegion,
            Global
        }

        public enum GETypes
        {
            OppositionChange,
            OppositionSet,
            InfluenceChange,
            InfluenceSet,
            MilitaryIncrease,
            MilitarySet,
            SpiesSet,
            SpiesAddToPool,
            DiplomatsAddToPool,
            MilitaryAddToPool,
            ChangeGovernment,
            FirePowerAddition,
            GNPchange,
            SpaceDiscount,
            ScoreAddition,
            Crisis
        }
    }
}
