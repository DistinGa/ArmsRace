using UnityEngine;
using System;
using System.Collections;

    [ExecuteInEditMode]
public class LoadTechInfo : MonoBehaviour {
    public bool Load;
    public SpaceRace SR;
    string[] lines;
    int mCurrLineNum = 0;

    // Update is called once per frame
    void Update () {
        if (!Load)
            return;

        Load = false;
        LoadInfo();
    }

    public void LoadInfo()
    {
        print("Start");

        mCurrLineNum = 0;
        lines = System.IO.File.ReadAllLines(System.IO.Path.Combine(Application.streamingAssetsPath, "techs.txt"));

        for (int techNumber = 0; techNumber < 40; techNumber++)
            AddSpaceRaceTechno(techNumber);

        print("Finish");
    }

    // загрузка текущей технологии
    private void AddSpaceRaceTechno(int techNumber)
    {
        SpaceRaceTechno srt = new SpaceRaceTechno(techNumber);

        srt.mImageFileName = lines[mCurrLineNum++];

        string descr = lines[mCurrLineNum++];
        while (lines[mCurrLineNum] != "*") descr += lines[mCurrLineNum++];
        srt.mRusDescr = descr;

        descr = "";
        for (mCurrLineNum++; lines[mCurrLineNum] != "*"; mCurrLineNum++) descr += lines[mCurrLineNum];
        srt.mUsaDescr = descr;

        mCurrLineNum++;
        srt.mCost = Int32.Parse(lines[mCurrLineNum++]);

        //if (!GameEngine.mIamUSA) mCurrLineNum++;

        string[] s = lines[mCurrLineNum++].Split(' ');
        srt.mLocalInfl = Int32.Parse(s[0]);
        srt.mGlobalInfl = Int32.Parse(s[1]);
        srt.mLocalInfl_1 = Int32.Parse(s[2]);
        srt.mGlobalInfl_1 = Int32.Parse(s[3]);

        //if (GameEngine.mIamUSA)
            mCurrLineNum++;

        SR.SetTechno(srt.mTechNumber+1, srt.mUsaDescr, srt.mRusDescr, srt.mCost, srt.mLocalInfl, srt.mGlobalInfl, srt.mLocalInfl_1, srt.mGlobalInfl_1);
    }
}

public class SpaceRaceTechno // технология
    {
        public int mTechNumber; // с 1, для привязки к картинке
        public string mImageFileName; // имя файла с картинкой
        public string mUsaDescr, mRusDescr; // текстовое описание
        public int mCost; // стоимость технологии
        public int mLocalInfl, mGlobalInfl, mLocalInfl_1, mGlobalInfl_1; // % прироста лок-глоб infl, и если открыли первыми

        public SpaceRaceTechno(int techNumber)
        {
            mTechNumber = techNumber;
        }
    }
