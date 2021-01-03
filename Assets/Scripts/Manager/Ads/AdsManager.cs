using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour
{
    public static AdsManager instance = null;

    public void Awake()
    {
        instance = GetComponent<AdsManager>();
        Advertisement.Initialize("3382566", false);
    }

    public void ShowAd()
    {
        if (Advertisement.IsReady() && !BackEndServerManager.instance.myInfo.ads)
        {
            Advertisement.Show("video");
        }
    }
}
