using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager.Scene;

public class GameStartsc : MonoBehaviour
{
    void Start()
    {
        CameraResolution.SetCamera();
        Invoke("GoGame", 2);
    }

    void GoGame()
    {
        Loader.Load(Scene.Login);
    }
}
