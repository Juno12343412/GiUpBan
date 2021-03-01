using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager.Scene;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Button LoadingBar = null;

    void Awake()
    {
        CameraResolution.SetCamera();

        Loader.LoaderCallback();
    }
    void Update()
    {
        LoadingBar.GetComponent<Image>().fillAmount = Mathf.Lerp(LoadingBar.GetComponent<Image>().fillAmount, Loader.GetLoadingProgress(), 0.7f);
    }
}
