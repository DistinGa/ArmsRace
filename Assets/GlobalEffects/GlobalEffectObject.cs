using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace GlobalEffects
{
    [System.Serializable]
    public class GlobalEffectObject : GEPref
    {
        //текущие значения (сохраняются)
        public int counter = 0; //если >0 - sov, если <0 - usa
        public int sovGPP = 0, usaGPP = 0;

        public GlobalEffectObject(GEPref GEPref)
        {
            eventName = GEPref.eventName;
            sovDescription = GEPref.sovDescription;
            usaDescription = GEPref.usaDescription;
            picture = GEPref.picture;
            offPicture = GEPref.offPicture;
            icon = GEPref.icon;
            sovGPPLimit = GEPref.sovGPPLimit;
            usaGPPLimit = GEPref.usaGPPLimit;
            sovEvents = GEPref.sovEvents;
            usaEvents = GEPref.usaEvents;

            counter = 0;
            sovGPP = 0;
            usaGPP = 0;
        }

        public bool GEDone
        {
            get { return (sovGPP >= sovGPPLimit || usaGPP >= usaGPPLimit); }
        }

        //возвращает 0 - если последствие уже сработало раньше,
        //           1 - если последствие сработало за СССР,
        //          -1 - если за США
        public int DoStep()
        {
            int res = 0;

            if (GEDone)
                return 0; //последствие уже сработало

            if (counter > 0)
                sovGPP += Mathf.Abs(counter);
            else if(counter < 0)
                usaGPP += Mathf.Abs(counter);

            if (sovGPP >= sovGPPLimit)
            {
                InvokeSovEvent();
                res = 1;
            }

            if (usaGPP >= usaGPPLimit)
            {
                InvokeUsaEvent();
                res = -1;
            }

            return res;
        }

        //Изменение счётчика через бонус лидера-дипломата
        public void ChangeGPPCounter()
        {
            int amount;
            PlayerScript player = GameManagerScript.GM.Player;

            if (player.TYGEDiscounter > 0)
            {
                if (player.Authority == Authority.Soviet)
                    amount = 1;
                else
                    amount = -1;

                counter += amount;
                player.TYGEDiscounter--;
            }
        }
    }
}
