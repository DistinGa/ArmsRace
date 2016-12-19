using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;

public class VideoQueue : MonoBehaviour {
    Image VideoPanel;
    MovieTexture Video;
    [SerializeField]
    AudioSource Audio;

    public float Interval = 0.3f;  //интервал проверки очереди роликов
    float TimeToTick;   //время до следующей итерации
    float NewsStartTime;    //Время начала трансляции новости

    // 1-тип файла ( 1-локальный СССР, 2- локальный США, 3-глобальный  )
    public const int V_TYPE_USSR = 1;
    public const int V_TYPE_USA = 2;
    public const int V_TYPE_GLOB = 3;

    // 2-подтип файла ( 1-срочный, 2-стандартный, 3-фоновый 0-не проверять/взять из настроек)
    public const int V_PRIO_NULL = 0;
    public const int V_PRIO_PRESSING = 1;
    public const int V_PRIO_STANDARD = 2;
    public const int V_PRIO_FON = 3;

    // 3-информация видео ( 1-космическая гонка, 4- военные действия революционеров, 5-добавление шпиона, 6- убийство шпиона, 7- ввод правительственных войск, 8- помощь революционерам и тд )
    public const int V_PUPPER_PEACE = 2; // 2-смена правительства мирным путем, 
    public const int V_PUPPER_WAR = 3; // 3-смена правительства через революцию 
    public const int V_PUPPER_REVOLUTION = 4; // 4-военные действия революционеров
    // public const int V_PUPPER_SPY_ADDED  =  5; // 5-добавление шпиона
    public const int V_PUPPER_SPY_KILLED = 6; // 6-убийство шпиона
    public const int V_PUPPER_MIL_ADDED = 7; // 7-ввод правительственных войск
    public const int V_PUPPER_REV_ADDED = 8; // 8-помощь революционерам
    public const int V_PUPPER_SUPPORT = 9; // 9-митинг в подд. государства
    public const int V_PUPPER_RIOTS = 10; // 10-беспорядки на улицах
    public const int V_PUPPER_OPPO_INFLU = 11; // 11-увеличение влияния оппозиционного infl (?)
    // public const int V_PUPPER_INFLU      = 12; // 12-увеличение влияния оппозиции influ

    public const int V_PUPPER_EVENT = 199; // Конец случ. событий
    public const int V_PUPPER_EVENT_FLOOD = 199; // Наводнение
    public const int V_PUPPER_EVENT_INDUSTR = 198; // Индустриализация
    public const int V_PUPPER_EVENT_NOBEL = 197; // Нобелевский лауреат
    public const int V_PUPPER_EVENT_FINANCE = 196; // Финансовый кризис
    public const int V_PUPPER_EVENT_POLITIC = 195; // Полит. кризис
    public const int V_PUPPER_EVENT_NAZI = 194; // Национализм
    public const int V_PUPPER_EVENT_COMMI = 193; // Коммун. движение
    public const int V_PUPPER_EVENT_DEMOCR = 192; // Демокр. движение
    public const int V_PUPPER_EVENT_START = 191; //

    public const int V_PUPPER_TECHNOLOW_START = 200; // 200-214 -нижний ряд технологий
    public const int V_PUPPER_TECHNOHIGH_START = 215; // 215-249 -верхние пятерки, слева направо снизу вверх

    bool mHaltVideo = false; // немедленно прервать видео

    public VideoRealPlayRolex mCurrentPlayedRolex; // текущий играемый ролик

    public List<VideoRolexPattern> mVideos; // список всех видеошаблонов с тегами
    public List<VideoRealPlayRolex> mVideoQueue; // очередь, в хвосте новые, ближайший -- 0-й

    void Start()
    {
        mVideoQueue = new List<VideoRealPlayRolex>();
        TimeToTick = Interval;
        VideoPanel = GetComponent<Image>();
        VideoPanel.material.mainTexture = null;
    }

    void Update()
    {
        if (TimeToTick <= 0)
        {
            TickTest();
            TimeToTick = Interval;
        }
        else
            TimeToTick -= Time.deltaTime;
    }

    // добавить ролик в очередь
    // type- глоб/лок V_TYPE_*
    public void AddRolex(int type, int tempo, int info, CountryScript c)
    {
        GameManagerScript GM = GameManagerScript.GM;

        VideoRealPlayRolex vrr = SearchRolex(type, tempo, info, c.Region, GM.GetCurrentEpoch(), c, GM.CurrentMonth);
        if (vrr == null) return;

        // звуковое сопровождение по группе ролика: всегда голосом фактически
        //string who = ""; // ролик дополнительно с привязкой к стороне ссср/сша
        vrr.mIsVoice = SettingsScript.Settings.mVoiceOn;

        if (info < V_PUPPER_EVENT_START) // обычные события
        {
            switch (info)
            {
                case V_PUPPER_MIL_ADDED:
                    if (type == V_TYPE_USA) vrr.mWavFile = "03 DislAmTroops";
                    else
                    if (type == V_TYPE_USSR) vrr.mWavFile = "04 DislSovTroops"; break;

                case V_PUPPER_PEACE:
                    if (type == V_TYPE_USA) vrr.mWavFile = "05 NewDemGov";
                    else
                    if (type == V_TYPE_USSR) vrr.mWavFile = "06 NewCommGov"; break;

                case V_PUPPER_WAR:
                    if (type == V_TYPE_USA) vrr.mWavFile = "07 NewDemGovInstalled";
                    else
                    if (type == V_TYPE_USSR) vrr.mWavFile = "08 NewCommGovInstalled"; break;

                case V_PUPPER_REVOLUTION: vrr.mWavFile = "09 CivilWarStrikes"; break;

                case V_PUPPER_SPY_KILLED:
                    if (type == V_TYPE_USA) vrr.mWavFile = "10 AmSpy";
                    else
                    if (type == V_TYPE_USSR) vrr.mWavFile = "11 SovSpy"; break;

                case V_PUPPER_REV_ADDED:
                    if (type == V_TYPE_USA) vrr.mWavFile = "13 US MilHelp";
                    else
                    if (type == V_TYPE_USSR) vrr.mWavFile = "12 SovMilHelp"; break;

                case V_PUPPER_SUPPORT: vrr.mWavFile = "14 PeopleOnStreets"; break;

                case V_PUPPER_RIOTS: vrr.mWavFile = "15 RiotOnTheStreets"; break;

                case V_PUPPER_OPPO_INFLU:
                    if (type == V_TYPE_USA) vrr.mWavFile = "17 AmInfluence";
                    else
                    if (type == V_TYPE_USSR) vrr.mWavFile = "16 SovInfluence"; break;

            }

        }
        else

        if (info < V_PUPPER_TECHNOLOW_START) // случайные события
        {
            switch (info)
            {
                case V_PUPPER_EVENT_FLOOD: vrr.mWavFile = "18 GreatFlood"; break;
                case V_PUPPER_EVENT_INDUSTR: vrr.mWavFile = "19 Industrialization"; break;
                case V_PUPPER_EVENT_NOBEL: vrr.mWavFile = "20 Great scientist"; break;
                case V_PUPPER_EVENT_FINANCE: vrr.mWavFile = "21 Financial crisis"; break;
                case V_PUPPER_EVENT_POLITIC: vrr.mWavFile = "25 DistabOfGov"; break;
                case V_PUPPER_EVENT_NAZI: vrr.mWavFile = "22 BigWaveOfNation"; break;
                case V_PUPPER_EVENT_COMMI: vrr.mWavFile = "23 CommunismMovement"; break;
                case V_PUPPER_EVENT_DEMOCR: vrr.mWavFile = "24 DemocrMovmnt"; break;
            }
        }

        else // для космотехнологий -- сказать речь за сторону
        {
            if (c.Authority == Authority.Amer) vrr.mWavFile = "01 AmSpcPrgr";
            else vrr.mWavFile = "02 SovSpcPrgr";
        }

        vrr.mWavFile = "Voices/" + vrr.mWavFile;
        PutRolexToQueue(vrr);
    }

    // поиск нужного ролика
    private VideoRealPlayRolex SearchRolex(int type, int tempo, int info, Region reg, int epoch, CountryScript c, int Month)
    {
        List<VideoRealPlayRolex> rlist = new List<VideoRealPlayRolex>();
        foreach (VideoRolexPattern vr in mVideos)
            if (vr.mType == type &&

                (vr.mSubtype == tempo || tempo == VideoQueue.V_PRIO_NULL) &&

                // совпадает вид ролика?
                vr.mInfoId == info &&

                // ... а если убийство шпиона, проверить еще, за чью сторону
                // (vr.mInfoId != VideoQueue.V_PUPPER_SPY_KILLED || IsSpyKillForSide(vr, info, kgbSpy)) &&

                (vr.mRegion == (int)reg || IsTechnoRolex(vr, info, c) /*|| IsEventRolex(vr, info, c)*/ )

                && (vr.mEpoch == epoch || vr.mEpoch == 0))

            {
                VideoRealPlayRolex vrr = new VideoRealPlayRolex(vr, c);
                
                // для технологий -- подходит самый первый:
                if (IsTechnoRolex(vr, info, c)) return vrr;

                // запоминаем одинаковые:
                rlist.Add(vrr);
            }

        if (rlist.Count == 0) return null;
        if (rlist.Count == 1) return rlist[0];
        return rlist[UnityEngine.Random.Range(0, rlist.Count)];
    }

    // является ли ролик технологией для стороны страны?
    private bool IsTechnoRolex(VideoRolexPattern vr, int info, CountryScript c)
    {
        if (info < VideoQueue.V_PUPPER_TECHNOLOW_START ||
            info >= VideoQueue.V_PUPPER_TECHNOLOW_START && vr.mId[vr.mId.Length - 1] != 's')
            return false; // для технологий не проверяем регион

        // сторона ролика технологии должна соответствовать стороне страны:
        return (vr.mId[vr.mId.Length - 1] == 's' && c.Authority == Authority.Soviet ||
                vr.mId[vr.mId.Length - 2] == 'n' && c.Authority == Authority.Amer);
    }

    // является ли ролик универсальным глобальным событием?
    private bool IsEventRolex(VideoRolexPattern vr, int info, CountryScript c)
    {
        return (info >= VideoQueue.V_PUPPER_EVENT_START && info <= VideoQueue.V_PUPPER_EVENT);
    }

    // проверяем, играется ли. если нет, показываем фоновый ролик 
    public void TickTest()
    {
        try
        {
            if (mHaltVideo) // немедленно прервать (фоновый) ролик
            {
                mHaltVideo = false;
                if (Video != null)
                {
                    Video.Stop();
                    Audio.Stop();
                }
            }


        A: if (mVideoQueue.Count <= 1
                                      // || GameEngine.rnd.Next(2)==0 // для отладки
                                      )
            { // в очереди пусто или единственный ролик уже играется -- поставить следом фоновый ролик
                List<int> fini = new List<int>(); // массив индексов фон-роликов
                int i;
                for (i = 0; i < mVideos.Count; i++)
                    if (mVideos[i].mSubtype == VideoRolexPattern.SUB_FON &&
                        (GameManagerScript.GM.Player.Authority == Authority.Amer && mVideos[i].mType == VideoQueue.V_TYPE_USA ||
                         GameManagerScript.GM.Player.Authority == Authority.Soviet && mVideos[i].mType == VideoQueue.V_TYPE_USSR) &&
                        GameManagerScript.GM.IsCurrentEpoch(mVideos[i].mEpoch))
                        fini.Add(i);

                //
                if (fini.Count <= 0) return;
                i = UnityEngine.Random.Range(0, fini.Count);

                VideoRealPlayRolex vrr = new VideoRealPlayRolex(mVideos[fini[i]], null);
                PutRolexToQueue(vrr); // фоновый...
            }


            else // запустить следующий из очереди
            {
                // еще играется ли текущий? 0-й -- всегда тот, который уже играется
                if (IsRunning()) return;


                // удалить рол, если он старый:
                if (mVideoQueue[1].mSetMonth < GameManagerScript.GM.CurrentMonth - 12 * 2)
                {
                    mVideoQueue.RemoveAt(1);
                    goto A;
                }

                //
                string videoFileName = mVideoQueue[1].getFileName();
                string videoCountry = (mVideoQueue[1].mCountry != null) ? mVideoQueue[1].mCountry.Name : "";
                string videoInfo = mVideoQueue[1].mVideoRolexPattern.mText;

                if (Video != null)
                {
                    Video.Stop();
                    Audio.Stop();
                }

                GameManagerScript.GM.SetInfo(videoInfo, videoCountry);  //Вывод текста новости

                //Начало трансляции новости
                if (SettingsScript.Settings.mVideo)
                {
                    VideoPanel.sprite = null;
                    Video = Resources.Load<MovieTexture>("Video/" + videoFileName);
                    VideoPanel.material.mainTexture = Video;
                    VideoPanel.SetMaterialDirty();
                    Video.Play();
                }
                else
                {
                    Sprite Spr = Resources.Load<Sprite>("news/" + videoFileName);
                    VideoPanel.sprite = Spr;
                    NewsStartTime = Time.time;
                }

                // и звук если был
                if (mVideoQueue[1].mWavFile != "")
                    if (SettingsScript.Settings.mVideo) // для AVI берем звук ролика
                    {
                        if (mVideoQueue[1].mIsVoice)
                            Audio.PlayOneShot(Resources.Load<AudioClip>(mVideoQueue[1].mWavFile), SettingsScript.Settings.mSoundVol);
                    }
                    else
                        Audio.PlayOneShot(Resources.Load<AudioClip>("sound/newspaper"), SettingsScript.Settings.mSoundVol); // для изображения просто звук

                mCurrentPlayedRolex = mVideoQueue[1];
                mVideoQueue.RemoveAt(0);
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    // поместить ролик в очередь с учетом приоритета
    private bool PutRolexToQueue(VideoRealPlayRolex vrr)
    {
        // если не фоновый, может уже такой есть?
        if (vrr.mVideoRolexPattern.mSubtype == V_PRIO_PRESSING || vrr.mVideoRolexPattern.mSubtype == V_PRIO_STANDARD)
        {
            for (int i = mVideoQueue.Count - 1; i >= 0; i--)
                if (mVideoQueue[i].mVideoRolexPattern.mInfoId == vrr.mVideoRolexPattern.mInfoId &&

                    // любые революционеры -- только один раз!
                    (mVideoQueue[i].mVideoRolexPattern.mInfoId == V_PUPPER_REVOLUTION ||

                      // нереволюционеры -- проверять еще и тип/регион: 
                      (mVideoQueue[i].mVideoRolexPattern.mType == vrr.mVideoRolexPattern.mType &&
                       mVideoQueue[i].mVideoRolexPattern.mRegion == vrr.mVideoRolexPattern.mRegion)
                    )
                  ) return false;

            // если только фоновые и играется фоновый, а поступил нефоновый, прервать!
            if (mCurrentPlayedRolex != null && mCurrentPlayedRolex.mVideoRolexPattern.mSubtype == V_PRIO_FON)
            {
                foreach (VideoRealPlayRolex virr in mVideoQueue)
                    if (virr.mVideoRolexPattern.mSubtype != V_PRIO_FON) goto L;

                mHaltVideo = true;

                while (mVideoQueue.Count > 1) mVideoQueue.RemoveAt(0);
            }
        }

    L: // добавляем рол:
       // GameEngine.lg( mVideoQueue.Count + ": " + vrr.mVideoRolexPattern.ToStr() );

        // удалить все вхождения в очередь riot-parade для региона, если вставляется смена правительства:
        if (vrr.mVideoRolexPattern.mInfoId == V_PUPPER_PEACE || vrr.mVideoRolexPattern.mInfoId == V_PUPPER_WAR)
        {
            //GameEngine.lg("MIL:");
            //for(int j=1; j<mVideoQueue.Count; j++) GameEngine.lg( mVideoQueue[j].mVideoRolexPattern.ToStr() );
            RemoveAllRolex(VideoQueue.V_PUPPER_MIL_ADDED, vrr.mVideoRolexPattern.mRegion);
            //GameEngine.lg("REV:");for(int j=1; j<mVideoQueue.Count; j++) GameEngine.lg( mVideoQueue[j].mVideoRolexPattern.ToStr() );
            RemoveAllRolex(VideoQueue.V_PUPPER_REV_ADDED, vrr.mVideoRolexPattern.mRegion);
            //GameEngine.lg("FIN:");for(int j=1; j<mVideoQueue.Count; j++) GameEngine.lg( mVideoQueue[j].mVideoRolexPattern.ToStr() );
            RemoveAllRolex(VideoQueue.V_PUPPER_REVOLUTION, vrr.mVideoRolexPattern.mRegion);
            //GameEngine.lg( ":"+mVideoQueue.Count.ToString() );
            RemoveAllRolex(VideoQueue.V_PUPPER_RIOTS, vrr.mVideoRolexPattern.mRegion);
            //GameEngine.lg( ":"+mVideoQueue.Count.ToString() );
        }

        //
        switch (vrr.mVideoRolexPattern.mSubtype)
        {
            case V_PRIO_PRESSING: // срочный
                                  // ищем последний срочный:
                for (int i = mVideoQueue.Count - 1; i >= 0; i--)
                    if (mVideoQueue[i].mVideoRolexPattern.mSubtype == V_PRIO_PRESSING)
                    { mVideoQueue.Insert(i + 1, vrr); return true; }
                if (mVideoQueue.Count > 0) mVideoQueue.Insert(1, vrr); // следом за играющимся
                else mVideoQueue.Insert(0, vrr); break; // первым

            case V_PRIO_STANDARD: // стандартный
                                  // ищем последний стандартный или срочный:
                for (int i = mVideoQueue.Count - 1; i >= 0; i--)
                    if (mVideoQueue[i].mVideoRolexPattern.mSubtype == V_PRIO_PRESSING || mVideoQueue[i].mVideoRolexPattern.mSubtype == V_PRIO_STANDARD)
                    { mVideoQueue.Insert(i + 1, vrr); return true; }
                if (mVideoQueue.Count > 0) mVideoQueue.Insert(1, vrr); // следом за играющимся
                else mVideoQueue.Insert(0, vrr); break; // первым

            case V_PRIO_NULL:
            case V_PRIO_FON: mVideoQueue.Add(vrr); break; // фоновый в хвост
        }

        return true;
    }

    // удалить все ролики из очереди
    private void RemoveAllRolex(int info, int region)
    {
        if (mVideoQueue.Count <= 1) return; // пусто или играется только один

        L: //
        for (int i = 1; i < mVideoQueue.Count; i++)
            if (mVideoQueue[i].mVideoRolexPattern.mInfoId == info &&
                mVideoQueue[i].mVideoRolexPattern.mRegion == region)
            {
                mVideoQueue.RemoveAt(i);
                goto L;
            }
    }

    // найти страну, по которой играется текущий ролик
    internal CountryScript GetVideoCountry()
    {
        if (mVideoQueue.Count == 0) return null;
        return mVideoQueue[0].mCountry;
    }

    // почистить очередь -- удалить ролики про военные действия в стране как сменилась власть
    internal void ClearVideoQueue(CountryScript c, int videoEvent)
    {
        for (int n = 1; n < mVideoQueue.Count; n++)
        {
            if (mVideoQueue[n].mCountry == c && mVideoQueue[n].mVideoRolexPattern.mInfoId == videoEvent)
            {
                mVideoQueue.RemoveAt(n);
            }
        }
    }

    //Переключение на карту, чей ролик транслируется.
    public void SnapToCountryFromNews()
    {
        if (mVideoQueue.Count == 0)
            return;

        CountryScript c = mVideoQueue[0].mCountry;
        if(c != null)
            GameManagerScript.GM.SnapToCountry(c);
    }

    //Проверка того, что ролик проигрывается или картинка новости показывается меньше пяти секунд.
    public bool IsRunning()
    {
        if (SettingsScript.Settings.mVideo)
            return (Video != null && Video.isPlaying);
        else
            return (Time.time - NewsStartTime <= 5f);
    }

    //Преобразование к локальной кодировке типа ролика
    public int LocalType(Authority Aut)
    {
        int res;

        switch (Aut)
        {
            case Authority.Amer:
                res = V_TYPE_USA;
                break;
            case Authority.Soviet:
                res = V_TYPE_USSR;
                break;
            default:
                res = V_TYPE_GLOB;
                break;
        }

        return res;
    }
}

[Serializable]
public class VideoRolexPattern // один видеоролик
{
    public const int SUB_FON = 3;

    public string mId;    // "номер" ролика, с 1 -- на самом деле, название avi-видеофайла; для технологий заканчивается на s ns
    public int mType;    // тип, ( 1-локальный СССР, 2- локальный США, 3-глобальный  )
    public int mSubtype; // 2-	подтип файла ( 1-срочный, 2-стандартный, 3-фоновый )
    public int mInfoId;  // 3-	информация видео ( 1-космическая гонка, 2-смена правительства мирным путем, 3- ...
    public int mRegion;  // (1-СССР, 2-Америка, 3-Европа, 4-Азия, 5- Страны третьего мира )
    public int mEpoch;   // 5-	временная эпоха ( 1- 1950-1970, 2- 1970-2000 )
    public string mText;  // сопроводительная инфа

    public string ToStr()
    {
        return mId + " " + mType + " " + mSubtype + " " + mInfoId + " " + mRegion + " " + mEpoch + " " + mText;
    }

}

[Serializable]
public class VideoRealPlayRolex // реальный видео-"ролик" для очереди
{
    public VideoRolexPattern mVideoRolexPattern;
    //public string mCountryName;
    //public int mCountryId;
    public CountryScript mCountry;
    public string mWavFile;  // звуковой файл для сопровождения
    public bool mIsVoice;  // wav -- голос?
    public int mSetMonth; // месяц, когда разместили ролик

    // создать ролик на основании шаблона ролика и страны в которой произошло событие
    public VideoRealPlayRolex(VideoRolexPattern videoRolexPattern, CountryScript Country)
    {
        mCountry = Country;
        //mCountryName = name;
        mVideoRolexPattern = videoRolexPattern;
        mWavFile = "";
        mIsVoice = false;
        mSetMonth = GameManagerScript.GM.CurrentMonth;
    }

    // имя файла ролика без расширения и без пути
    internal string getFileName()
    {
        return mVideoRolexPattern.mId.ToString();
    }
}
