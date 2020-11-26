using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager.Pooling;
using Manager.Scene;

// 처음 시작하는 곳

public class Main : MonoBehaviour
{
    void Start()
    {
        MainUI.instance.SetShopItems();    
    }
}
