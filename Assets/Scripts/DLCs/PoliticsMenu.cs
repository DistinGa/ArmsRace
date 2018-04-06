using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DLC_Politics
{
    public class PoliticsMenu : MonoBehaviour
    {
        GameManagerScript GM_;
        bool activeMode = false;    //флаг, показывающий, что меню открыто в активном режиме для выборов (неактивное открывается при нажатии на флаги верхнего меню)
        bool slotIsOpend; //флаг, показывающий, что во время этих выборов слот уже был открыт.
        bool initPause;
        PolMinisterForMenu[] selMinisters = new PolMinisterForMenu[3]; //3 выбранных министра
        PolMinisterForMenu[] milMinisters = new PolMinisterForMenu[6], dipMinisters = new PolMinisterForMenu[6], ecMinisters = new PolMinisterForMenu[6];
        PolMinisterForMenu[] actMinisters; //министры линии, открытой в данный момент в нижней панели.
        int actLineIndx;    //Индекс линии, из которой выбирается министр (0 - дипломаты, 1 - экономисты, 2 - военные)

        [SerializeField]
        GameObject bottomMenu, btnVote, cnvSlotOpening, goVotingsCoverPanel, goStagnation;
        [SerializeField]
        Sprite USA_BG, USSR_BG, USA_Bottom_BG, USSR_Bottom_BG, USA_cardBack, USSR_cardBack, USA_cabinet, USSR_cabinet, ConservatorIcon, LiberalIcon, USA_SlotOpeningBack, USSR_SlotOpeningBack;
        [SerializeField]
        List<Selectable> activeElements;    //кнопки, которые становятся недоступными при открытии меню между выборами.
        [SerializeField]
        GameObject[] covers = new GameObject[3];
        [SerializeField]
        Sprite[] coverSpritesUS, coverSpritesSov, headsUS, headsSov, Suits, minHeadsUS, minHeadsSov, lineIcons;
        [SerializeField]
        Image[] selMinisterCards;  //Карточки выбранных министров.
        [SerializeField]
        Image[] ministerCards;  //Карточки в нижнем меню.
        [SerializeField]
        Toggle[] tggLines;
        [SerializeField]
        Image imgHead, imgSuit, cabinet, bottomIcon1, bottomIcon2;
        [SerializeField]
        Text txtBonuses;

        [Space(10)]
        [SerializeField]
        SOLP soLeaderProperties;    //для отображения текста бонусов лидера

        Authority MenuAuthority;

        #region Properties
        GameManagerScript GM
        {
            get
            {
                if (GM_ == null)
                    GM_ = GameManagerScript.GM;

                return GM_;
            }
        }

        public bool ActiveMode
        {
            get { return activeMode; }
        }
        #endregion

        public void UpdateView()
        {
            if (MenuAuthority == Authority.Amer)
            {
                GetComponent<Image>().sprite = USA_BG;
                cabinet.sprite = USA_cabinet;
            }
            else
            {
                GetComponent<Image>().sprite = USSR_BG;
                cabinet.sprite = USSR_cabinet;
            }

            foreach (var item in activeElements)
            {
                item.gameObject.SetActive(activeMode);
            }

            UpdateSelMinisters();
        }

        void UpdateSelMinisters()
        {
            txtBonuses.text = "";
            goStagnation.SetActive(false);

            if (selMinisters[0] != null && selMinisters[1] != null && selMinisters[2] != null &&
                selMinisters[0].LeaderInd == selMinisters[1].LeaderInd && selMinisters[1].LeaderInd == selMinisters[2].LeaderInd)
            {
                //Выбранные министры поддерживают одного кандидата.
                if (activeMode)
                {
                    btnVote.SetActive(true);
                    goStagnation.SetActive(GM.CurrentMonth > 0 && selMinisters[0].LeaderInd == GM.Player.PlayerLeader.LeaderID);
                }

                imgHead.sprite = MenuAuthority == Authority.Amer ? headsUS[selMinisters[0].LeaderInd - 1] : headsSov[selMinisters[0].LeaderInd - 1];

                if (selMinisters[0].Liberal && selMinisters[1].Liberal && selMinisters[2].Liberal)
                    imgSuit.sprite = Suits[0];  //лидер - экономист
                else if(!selMinisters[0].Liberal && !selMinisters[1].Liberal && !selMinisters[2].Liberal)
                    imgSuit.sprite = Suits[1];  //лидер - милитарист
                else
                    imgSuit.sprite = Suits[2];  //лидер - дипломат

                imgHead.color = new Color(1, 1, 1, 1);
                imgSuit.color = new Color(1, 1, 1, 1);

                //Бонусы лидера
                LeaderType tmpLT = GetleaderTypeByMinisters();
                txtBonuses.text = LeaderScript.GetBonusesStatic(1, soLeaderProperties, selMinisters[0].LeaderInd, tmpLT);
                txtBonuses.text += "\n" + LeaderScript.GetBonusesStatic(2, soLeaderProperties, selMinisters[0].LeaderInd, tmpLT);
            }
            else
            {
                btnVote.SetActive(false);
                imgHead.color = new Color(1, 1, 1, 0);
                imgSuit.color = new Color(1, 1, 1, 0);
            }

            //Отрисовка иконок лидера и "либеральности".
            GameObject tmpheadImage, tmpLiberalImage; 
            for (int i = 0; i < 3; i++)
            {
                tmpheadImage = selMinisterCards[i].transform.FindChild("head").gameObject;
                tmpLiberalImage = selMinisterCards[i].transform.FindChild("liberal").gameObject;

                if (selMinisters[i] == null)
                {
                    selMinisterCards[i].sprite = MenuAuthority == Authority.Amer ? USA_cardBack : USSR_cardBack;
                    selMinisterCards[i].GetComponentInChildren<Text>().text = "";
                    tmpheadImage.SetActive(false);
                    tmpLiberalImage.SetActive(false);
                    continue;
                }

                selMinisterCards[i].sprite = selMinisters[i].Photo;
                tmpheadImage.SetActive(true);
                tmpLiberalImage.SetActive(true);
                tmpheadImage.GetComponent<Image>().sprite = MenuAuthority == Authority.Amer ? minHeadsUS[selMinisters[i].LeaderInd - 1]: minHeadsSov[selMinisters[i].LeaderInd - 1];
                tmpLiberalImage.GetComponent<Image>().sprite = selMinisters[i].Liberal ? LiberalIcon : ConservatorIcon;

                //Бонусы министров
                if (selMinisters[i] != null)
                {
                    selMinisterCards[i].GetComponentInChildren<Text>().text = selMinisters[i].GetBonusDescr();
                    //Окно общих бонусов.
                    txtBonuses.text += "\n" + selMinisters[i].GetBonusDescr();
                }
            }
        }

        void OnEnable()
        {
            initPause = GM.IsPaused;
            GM.IsPaused = true;

            UpdateView();
        }

        void OnDisable()
        {
            GM.IsPaused = initPause;
            activeMode = false;
        }

        public void ShowMenu(Authority auth, bool isVotings)
        {
            if (activeMode)
                return; //Идут выборы переоткрытие или закрытие меню не допустимо.

            if (MenuAuthority != auth && this.isActiveAndEnabled)
                gameObject.SetActive(false);    //костыль для переоткрытия с новым Authority

            activeMode = isVotings;
            MenuAuthority = auth;
            goVotingsCoverPanel.SetActive(isVotings);

            //Если это выборы, заполняем министров.
            if (isVotings)
            {
                slotIsOpend = false;

                int LeaderInd = Random.Range(1, 5); //лидер, который может быть выбран в любом случае (лидеры нумеруются с 1).

                selMinisters = new PolMinisterForMenu[3];  //Сброс выбранных министров.

                List<PolMinister> tmpMinisters, tmpSpecMinisters;
                if (auth == Authority.Amer)
                    tmpMinisters = GM.DLC_Polit.USMinisters;
                else
                    tmpMinisters = GM.DLC_Polit.SovMinisters;

                int dec = 1950 + GM.CurrentMonth / 120 * 10;
                int listIndx;

                //Условие для определения пиджака лидера выбранного перед началом игры.
                //System.Func<PolMinister, bool> cond = (m) => m.Liberal == m.Liberal;
                System.Func<PolMinister, bool> cond = (m) => true;
                if (GM.CurrentMonth == 0)
                {
                    switch (GM.Player.PlayerLeader.ActualLeaderType)
                    {
                        case LeaderType.Economic:
                            cond = (m) => m.Liberal == true;
                            break;
                        case LeaderType.Militaristic:
                            cond = (m) => m.Liberal == false;
                            break;
                        default:
                            break;
                    }
                }

                //Дипломаты
                tmpSpecMinisters = tmpMinisters.Where(m => m.LeaderType == LeaderType.Diplomatic && m.InDecade(dec) && cond(m)).ToList();
                for (int i = 0; i < 6; i++)
                {
                    if (tmpSpecMinisters.Count == 0)
                        break;

                    listIndx = Random.Range(0, tmpSpecMinisters.Count);
                    dipMinisters[i] = new PolMinisterForMenu(tmpSpecMinisters[listIndx]);
                    tmpSpecMinisters.RemoveAt(listIndx);
                    if (GM.CurrentMonth == 0)
                    {
                        //Если это выборы сразу после начала игры, можем выбирать только лидера, которого выбрали перед началом игры.
                        dipMinisters[i].LeaderInd = GM.Player.PlayerLeader.LeaderID;
                    }
                    else
                    {
                        if (i == 0)
                            dipMinisters[i].LeaderInd = LeaderInd;
                        else
                            dipMinisters[i].LeaderInd = Random.Range(1, 5);
                    }
                }

                //Экономисты
                tmpSpecMinisters = tmpMinisters.Where(m => m.LeaderType == LeaderType.Economic && m.InDecade(dec) && cond(m)).ToList();
                for (int i = 0; i < 6; i++)
                {
                    if (tmpSpecMinisters.Count == 0)
                        break;

                    listIndx = Random.Range(0, tmpSpecMinisters.Count);
                    ecMinisters[i] = new PolMinisterForMenu(tmpSpecMinisters[listIndx]);
                    tmpSpecMinisters.RemoveAt(listIndx);
                    if (GM.CurrentMonth == 0)
                    {
                        //Если это выборы сразу после начала игры, можем выбирать только лидера, которого выбрали перед началом игры.
                        ecMinisters[i].LeaderInd = GM.Player.PlayerLeader.LeaderID;
                    }
                    else
                    {
                        if (i == 0)
                            ecMinisters[i].LeaderInd = LeaderInd;
                        else
                            ecMinisters[i].LeaderInd = Random.Range(1, 5);
                    }
                }

                //Милитаристы
                tmpSpecMinisters = tmpMinisters.Where(m => m.LeaderType == LeaderType.Militaristic && m.InDecade(dec) && cond(m)).ToList();
                for (int i = 0; i < 6; i++)
                {
                    if (tmpSpecMinisters.Count == 0)
                        break;

                    listIndx = Random.Range(0, tmpSpecMinisters.Count);
                    milMinisters[i] = new PolMinisterForMenu(tmpSpecMinisters[listIndx]);
                    tmpSpecMinisters.RemoveAt(listIndx);
                    if (GM.CurrentMonth == 0)
                    {
                        //Если это выборы сразу после начала игры, можем выбирать только лидера, которого выбрали перед началом игры.
                        milMinisters[i].LeaderInd = GM.Player.PlayerLeader.LeaderID;
                    }
                    else
                    {
                        if (i == 0)
                            milMinisters[i].LeaderInd = LeaderInd;
                        else
                            milMinisters[i].LeaderInd = Random.Range(1, 5);
                    }
                }

                tggLines[0].isOn = true;
                tggLines[1].isOn = false;
                tggLines[2].isOn = false;
            }
            else
            {
                //не выборы
                PlayerScript tmpPlayer = GM.GetPlayerByAuthority(MenuAuthority);
                for (int i = 0; i < 3; i++)
                {
                    selMinisters[i] = new PolMinisterForMenu(GM.DLC_Polit.curMinisters(MenuAuthority)[i]);
                    selMinisters[i].LeaderInd = tmpPlayer.PlayerLeader.LeaderID;
                }
                
            }


            GM.ToggleTechMenu(gameObject);
            //UpdateView();
        }

        public void SelectMinister(int indx)
        {
            SoundManager.SM.PlaySound("sound/buttons");
            selMinisters[actLineIndx] = actMinisters[indx];
            //tggLines[actLineIndx].isOn = false;
            //bottomMenu.SetActive(false);
            UpdateSelMinisters();
        }

        //Нажатие на кнопку под министром для открытия нижней панели.
        public void SelectLine(int l)
        {
            if (tggLines[l].isOn)
            {
                SoundManager.SM.PlaySound("sound/buttons");
                ShowBottomMenu(l);
            }
            else
            {
                //Если кнопка "отжимается", скрываем нижнюю панель.
                bool present = false;
                foreach (var item in tggLines)
                {
                    if (item.isOn)
                    {
                        present = true;
                        break;
                    }
                }

                if (!present)
                    //bottomMenu.SetActive(false);
                    tggLines[l].isOn = true;    //не даём кнопке "отжиматься"
            }
        }

        void ShowBottomMenu(int lineIndx)
        {
            if (lineIndx < 0 || lineIndx > 2)
                return;

            actLineIndx = lineIndx;

            bottomIcon1.sprite = lineIcons[lineIndx];
            bottomIcon2.sprite = lineIcons[lineIndx];

            if (MenuAuthority == Authority.Amer)
                bottomMenu.GetComponent<Image>().sprite = USA_Bottom_BG;
            else
                bottomMenu.GetComponent<Image>().sprite = USSR_Bottom_BG;

            //Отображение министров
            switch (actLineIndx)
            {
                case 0:
                    actMinisters = dipMinisters;
                    break;
                case 1:
                    actMinisters = ecMinisters;
                    break;
                case 2:
                    actMinisters = milMinisters;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < actMinisters.Length; i++)
            {
                if (actMinisters[i] ==  null)
                    continue;   //Если не все министры линии заполнились при условии от первых выборов (из-за необходимости соответствия одному типу лидера)
                ministerCards[i].sprite = actMinisters[i].Photo;
                ministerCards[i].transform.FindChild("head").GetComponent<Image>().sprite = MenuAuthority == Authority.Amer ? minHeadsUS[actMinisters[i].LeaderInd - 1] : minHeadsSov[actMinisters[i].LeaderInd - 1];
                ministerCards[i].transform.FindChild("liberal").GetComponent<Image>().sprite = actMinisters[i].Liberal ? LiberalIcon : ConservatorIcon;
                ministerCards[i].GetComponentInChildren<Text>().text = actMinisters[i].GetBonusDescr();
            }

            //Отображение исторических "карточек".
            for (int i = 0; i < 3; i++)
            {
                covers[i].SetActive(!(i < GM.DLC_Polit.OpenedSlots));
                covers[i].GetComponent<Image>().sprite = MenuAuthority == Authority.Amer? coverSpritesUS[i]: coverSpritesSov[i];

                covers[i].GetComponentInChildren<Button>().interactable = !(i > GM.DLC_Polit.OpenedSlots) && !slotIsOpend && GM.CurrentMonth > 0;
            }

            bottomMenu.SetActive(true);
        }

        public void OpenMenuForSlotOpening()
        {
            if (slotIsOpend)
                return;

            SoundManager.SM.PlaySound("sound/buttons");
            cnvSlotOpening.transform.FindChild("Panel/BackGround").GetComponent<Image>().sprite = MenuAuthority == Authority.Amer ? USA_SlotOpeningBack : USSR_SlotOpeningBack;
            cnvSlotOpening.SetActive(true);
        }

        public void OpenSlot()
        {
            GM.DLC_Polit.OpenSlot(MenuAuthority);
            covers[GM.DLC_Polit.OpenedSlots - 1].SetActive(false);
            slotIsOpend = true;
            cnvSlotOpening.SetActive(false);
            SoundManager.SM.PlaySound(MenuAuthority == Authority.Amer? "sound/usa_lowdiscover" : "sound/ussr_lowdiscover");
        }

        public void ButtonVoteClick()
        {
            SoundManager.SM.PlaySound("sound/un-news");
            GM.DLC_Polit.ApplyVotingsResult(GM.GetPlayerByAuthority(MenuAuthority), selMinisters[0].LeaderInd, GetleaderTypeByMinisters(), selMinisters);
            FindObjectOfType<CameraScriptXZ>().setOverMenu = false;
            gameObject.SetActive(false);
            bottomMenu.SetActive(false);
            goVotingsCoverPanel.SetActive(false);
        }

        //Определение типа выбираемого лидера по министрам.
        LeaderType GetleaderTypeByMinisters()
        {
            LeaderType lt;
            if (selMinisters[0].Liberal && selMinisters[1].Liberal && selMinisters[2].Liberal)
                lt = LeaderType.Economic;  //лидер - экономист
            else if (!selMinisters[0].Liberal && !selMinisters[1].Liberal && !selMinisters[2].Liberal)
                lt = LeaderType.Militaristic;  //лидер - милитарист
            else
                lt = LeaderType.Diplomatic;  //лидер - дипломат

            return lt;
        }
        ///////////////////////////////
        class PolMinisterForMenu : PolMinister
        {
            public int LeaderInd;

            public PolMinisterForMenu(PolMinister m)
            {
                this.Desc = m.Desc;
                this.Photo = m.Photo;
                this.Liberal = m.Liberal;
                this.BonusType = m.BonusType;
                this.LeaderType = m.LeaderType;
                this.Decades = m.Decades;
            }
        }
    }
}