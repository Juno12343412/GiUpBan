using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFuncs : MonoBehaviour
{
    private float shakeAmount = 0;
    private float shakeTime = 0;
    Vector3 initPos;


    public void SetShakeTime(float time, float scale)
    {
        shakeTime = time;
        shakeAmount = scale;
    }

    void Start()
    {
        initPos = transform.position;
    }
    
    void Update()
    {
        if(shakeTime > 0)
        {
            transform.position = Random.insideUnitSphere * shakeAmount + initPos;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            shakeTime = 0.0f;
            transform.position = initPos;
        }
    }
}
