using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager.Pooling;

public class Effect : PoolingObject
{
    public override string objectName => "Effect";

    public override void Init()
    {
        base.Init();
    }

    public override void Release()
    {
        base.Release();
    }
}
