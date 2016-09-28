using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

    [ExecuteInEditMode]
public class LoadVideoInfo : MonoBehaviour {

    public bool Load;
    public VideoQueue VQ;

	// Use this for initialization
	void Start () {
	
	}
	
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

        VQ.mVideos = new List<VideoRolexPattern>();
        int n = 0;
        try
        {
            // грузим видео из файла:
            string[] lines;
            lines = System.IO.File.ReadAllLines(System.IO.Path.Combine(Application.streamingAssetsPath, "video.txt"));

            while (n < lines.Length - 1)
            {
                string[] s = lines[n++].Split(' ');
                VideoRolexPattern vr = new VideoRolexPattern();
                vr.mId = s[0];
                vr.mType = Int32.Parse(s[1]);
                vr.mSubtype = Int32.Parse(s[2]);
                vr.mInfoId = Int32.Parse(s[3]);
                vr.mRegion = Int32.Parse(s[4]);
                vr.mEpoch = Int32.Parse(s[5]);
                vr.mText = lines[n++];

                VQ.mVideos.Add(vr);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Loading video error " + n + " " + e.Message);
        }

        print("Finish");
    }
}
