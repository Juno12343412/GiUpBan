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
        if (shakeTime > 0)
        {
            Debug.Log("카메라 흔들리는 중 : " + Time.timeScale);
            transform.position = Random.insideUnitSphere * shakeAmount + initPos;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            shakeTime = 0.0f;
            transform.position = initPos;
        }
    }
    public void CameraAnimReset()
    {
        GetComponent<Animator>().SetBool("isMove", false);
    }
}