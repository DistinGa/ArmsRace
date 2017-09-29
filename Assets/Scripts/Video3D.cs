using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;

namespace video3D
{
    public class Video3D : MonoBehaviour
    {
        public static Video3D V3Dinstance;

        float curVideoPeriod;
        Image VideoPanel;
        List<NewsBlock> CommonList = new List<NewsBlock>();
        List<NewsBlock> PriorList = new List<NewsBlock>();
        NewsBlock CurrentNewsBlock = null;
        Dictionary<int, GameObject> NonCitysAnimObjects = new Dictionary<int, GameObject>();    //Словарь анимационных объектов не связанных с городами.

        public AudioSource AS;
        public Image Fader;
        [Tooltip("Время проигрывания (сек.)")]
        public float Period;
        [Tooltip("Сколько месяцев анимация будет находиться в режиме ожидания")]
        public int WaitTime;

        [Space(10)]
        [SerializeField]
        GameObject[] animObjArray;
        [SerializeField]
        GameObject[] amerGroundFacilities;
        [SerializeField]
        GameObject[] sovGroundFacilities;
        [SerializeField]
        GameObject SovRocket, AmRocket;

        public bool NewsListIsEmpty
        {
            get { return PriorList.Count == 0 && CurrentNewsBlock != null && !CurrentNewsBlock.Prior; }
        }

        void Awake()
        {
            VideoPanel = GetComponent<Image>();
            V3Dinstance = this;

            NonCitysAnimObjects.Add((int)NonCitysAnim.SpaceUSA, animObjArray[(int)NonCitysAnim.SpaceUSA]);
            NonCitysAnimObjects.Add((int)NonCitysAnim.SpaceUSSR, animObjArray[(int)NonCitysAnim.SpaceUSSR]);
            NonCitysAnimObjects.Add((int)NonCitysAnim.UNOUSA, animObjArray[(int)NonCitysAnim.UNOUSA]);
            NonCitysAnimObjects.Add((int)NonCitysAnim.UNOUSSR, animObjArray[(int)NonCitysAnim.UNOUSSR]);
            NonCitysAnimObjects.Add((int)NonCitysAnim.NukeUSA, animObjArray[(int)NonCitysAnim.NukeUSA]);
            NonCitysAnimObjects.Add((int)NonCitysAnim.NukeUSSR, animObjArray[(int)NonCitysAnim.NukeUSSR]);
            NonCitysAnimObjects.Add((int)NonCitysAnim.MilitaryUSA, animObjArray[(int)NonCitysAnim.MilitaryUSA]);
            NonCitysAnimObjects.Add((int)NonCitysAnim.MilitaryUSSR, animObjArray[(int)NonCitysAnim.MilitaryUSSR]);
        }

        void Start()
        {
            AS.volume = SettingsScript.Settings.mSoundVol;
        }

        void Update()
        {
            if (curVideoPeriod < 0 && (CurrentNewsBlock == null || CurrentNewsBlock.SetMonth > 0 || PriorList.Count > 0 || CommonList.Count > 0))
            {
                curVideoPeriod = 0.5f;
                FadeOut();
            }
            else
                curVideoPeriod -= Time.deltaTime;
        }

        //Городские новости
        public void AddNews(GameObject VideoObject, GameObject AnimObject, int StartMonth, CountryScript Country, string NewsText, string AudioName = "", bool Prior = false)
        {
            NewsBlock newBlk = new NewsBlock(VideoObject, AnimObject, StartMonth, Country, NewsText, Prior, AudioName);
            if (Prior)
            {
                if (PriorList.Find(x => x.Equals(newBlk)) == null && !CurrentNewsBlock.Equals(newBlk))
                {
                    PriorList.Add(newBlk);
                }
            }
            else
            {
                if (CommonList.Find(x => x.Equals(newBlk)) == null && !CurrentNewsBlock.Equals(newBlk))
                {
                    CommonList.Add(newBlk);
                }
            }

            ////Если сейчас проигрывается "фоновая" анимация, стартуем только что вставленную.
            //if (CurrentNewsBlock.SetMonth == 0)
            //    FadeOut();
        }

        //Видео для негородских новостей
        public void AddTechNews(NonCitysAnim VideoObjectIndex, int SpaceIndex, int StartMonth, CountryScript Country, string NewsText, string AudioName = "", bool Prior = false)
        {
            GameObject AnimObj = null;

            if (SpaceIndex > -1 && SpaceIndex < amerGroundFacilities.Length)
            {
                if (Country.Authority == Authority.Amer)
                    for (int i = 0; i < amerGroundFacilities.Length; i++)
                    {
                        if (i <= SpaceIndex)
                            amerGroundFacilities[i].SetActive(true);
                        else
                            amerGroundFacilities[i].SetActive(false);
                    }
                else
                    for (int i = 0; i < sovGroundFacilities.Length; i++)
                    {
                        if (i <= SpaceIndex)
                            sovGroundFacilities[i].SetActive(true);
                        else
                            sovGroundFacilities[i].SetActive(false);
                    }
            }
            else if (SpaceIndex == 16)  //технология запусков, нужно активировать ракету
                AnimObj = Country.Authority == Authority.Amer? AmRocket: SovRocket;

            AddNews(NonCitysAnimObjects[(int)VideoObjectIndex], AnimObj, StartMonth, Country, NewsText, AudioName, Prior);
        }

        public void ShowDefaultAnim(CountryScript c)
        {
            //фоновую анимацию запускаем только, если сейчас проигрывается другая фоновая. В противном случае ждём окончания текущей.
            if (CurrentNewsBlock == null || (CurrentNewsBlock.SetMonth == 0 && CurrentNewsBlock.Country != c))
                FadeOut();
            //ChangeNews(new NewsBlock(c.CapitalScene, null, 0, c, ""));
        }

        private void FadeOut()
        {
            if (Fader.color.a == 1 && CurrentNewsBlock.SetMonth == 0)
                TestNewsLists();
            else
                StartCoroutine(FadeAnimation(0.25f, true));
        }

        private void ChangeNews(NewsBlock nb)
        {
            GameManagerScript.GM.SetInfo(nb.NewsText, nb.Country.Name);

            if (CurrentNewsBlock == null || CurrentNewsBlock.VideoObject != nb.VideoObject)
            {
                if (CurrentNewsBlock != null && CurrentNewsBlock.VideoObject != null)
                    CurrentNewsBlock.VideoObject.SetActive(false);

                if (nb.VideoObject != null)
                    nb.VideoObject.SetActive(true);
            }

            if (nb.VideoObject != null)
                StartCoroutine(FadeAnimation(0.25f, false));

            if (CurrentNewsBlock != null && CurrentNewsBlock.AnimObject != null)
                CurrentNewsBlock.AnimObject.SetActive(false);

            if (nb.AnimObject != null)
                nb.AnimObject.SetActive(true);

            CurrentNewsBlock = nb;

            if (CurrentNewsBlock.SetMonth > 0)
            {
                //Invoke("FadeOut", Period);
                curVideoPeriod = Period;
            }

            if (SettingsScript.Settings.mSoundOn && nb.AudioName != "")
            {
                AS.clip = Resources.Load<AudioClip>(nb.AudioName);
                AS.Play();
            }
        }

        //Вызывается в конце анимации fadeout.
        public void TestNewsLists()
        {
            GameManagerScript GM = GameManagerScript.GM;
            int cm = GM.CurrentMonth;

            //сначала удаляем просроченные новости
            if (PriorList.Count > 0)
            {
                while (PriorList[0].SetMonth > 0 && PriorList[0].SetMonth + WaitTime < cm)
                {
                    PriorList.RemoveAt(0);
                }
            }

            if (CommonList.Count > 0)
            {
                while (CommonList[0].SetMonth > 0 && CommonList[0].SetMonth + WaitTime < cm)
                {
                    CommonList.RemoveAt(0);
                }
            }

            ////
            if (PriorList.Count > 0)
            {
                ChangeNews(PriorList[0]);
                //PriorList.RemoveAt(0);
            }
            else if (CommonList.Count > 0)
            {
                ChangeNews(CommonList[0]);
                //CommonList.RemoveAt(0);
            }
            ////фоновую анимацию запускаем только, если сейчас проигрывается другая фоновая. В противном случае ждём окончания текущей.
            //else if (CurrentNewsBlock == null || CurrentNewsBlock.SetMonth == 0)
            else
            {
                CountryScript c = GM.CurrentCountry;
                ChangeNews(new NewsBlock(c.CapitalScene, null, 0, c, "", false));
                //ShowDefaultAnim(GM.CurrentCountry);
            }
        }

        //fadeout: true - fadeout, false - fadein
        private IEnumerator FadeAnimation(float period, bool fadeout)
        {
            float rt = fadeout ? 1 / period : -1 / period;
            float alpha = fadeout ? 0 : 1;

            while (period > 0)
            {
                alpha += rt * Time.deltaTime;
                period -= Time.deltaTime;

                Fader.color = new Color(0, 0, 0, alpha);
                AS.volume = (1 - alpha) * SettingsScript.Settings.mSoundVol;
                yield return new WaitForEndOfFrame();
            }

            Fader.color = new Color(0, 0, 0, fadeout ? 1 : 0);
            AS.volume = (fadeout ? 0 : 1) * SettingsScript.Settings.mSoundVol;

            if (fadeout)
            {
                AS.clip = null;
                AS.Stop();
                if (CommonList.Count > 0 && CommonList[0] == CurrentNewsBlock)
                    CommonList.RemoveAt(0);
                else if(PriorList.Count > 0 && PriorList[0] == CurrentNewsBlock)
                    PriorList.RemoveAt(0);

                TestNewsLists();
            }
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        //Переключение на карту, чей ролик транслируется.
        public void SnapToCountryFromNews()
        {
            if (CurrentNewsBlock != null)
            {
                GameManagerScript.GM.SnapToCountry(CurrentNewsBlock.Country);
            }
        }
    }

    public class NewsBlock
    {
        public GameObject VideoObject;
        public GameObject AnimObject;
        public int SetMonth;
        public CountryScript Country;
        public string NewsText;
        public string AudioName;
        public bool Prior;

        public NewsBlock(GameObject VideoObject, GameObject AnimObject, int SetMonth, CountryScript Country, string NewsText, bool Prior, string AudioName = "")
        {
            this.VideoObject = VideoObject;
            this.AnimObject = AnimObject;
            this.SetMonth = SetMonth;
            this.Country = Country;
            this.NewsText = NewsText;
            this.AudioName = AudioName;
            this.Prior = Prior;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            NewsBlock nbObj = obj as NewsBlock;
            return (Country == nbObj.Country && NewsText == nbObj.NewsText);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum NonCitysAnim
    {
        SpaceUSA,
        SpaceUSSR,
        UNOUSA,
        UNOUSSR,
        NukeUSA,
        NukeUSSR,
        MilitaryUSA,
        MilitaryUSSR
    }

    public enum CitysAnim
    {
        War,
        InvasionUSA,
        InvasionUSSR,
        DemRed,
        DemBlue,
        ParadRed,
        ParadBlue,
        StrikeRed,
        StrikeBlue,
        Storm,
        Industry,
        Nobel,
        PoliticalCrisis,
        FinanceCrisis,
        MoveRed,
        MoveBlue,
        MoveNeutral,
        Nuke
    }
}