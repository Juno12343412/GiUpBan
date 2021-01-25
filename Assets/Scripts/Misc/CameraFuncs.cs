using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFuncs : MonoBehaviour
{
    private float shakeAmount = 0;
    private float shakeTime = 0;
    private Vector3 direction = Vector3.zero;

    Vector3 initPos;

    public void SetShakeTime(float time, float scale, Vector3 direction)
    {
        shakeTime = time;
        shakeAmount = scale;
        this.direction = direction;
    }

    void Start()
    {
        initPos = transform.position;
    }

    void Update()
    {
        if (shakeTime > 0)
        {
            //if (direction == Vector3.zero)
            //{
            //    Debug.Log("카메라 흔들리는 중 : " + Time.timeScale);
            //    transform.position = Random.insideUnitSphere * shakeAmount + initPos;
            //}
            //else
            //    transform.position = direction * Random.Range(2f, 4f) * shakeAmount + initPos;
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

    public IEnumerator ZoomCamera(float _Time, float _Scale)
    {
        Debug.Log("줌 시작");
        float progress = _Time;
        yield return null;

        while (progress >= 0f || Camera.main.fieldOfView > 50f)
        {
            GetComponent<Camera>().fieldOfView -= Time.deltaTime * _Scale;
            progress -= Time.deltaTime;
            yield return null;
        }

        progress = _Time;

        while (progress >= 0f || Camera.main.fieldOfView < 60f)
        {
            GetComponent<Camera>().fieldOfView += Time.deltaTime * _Scale;
            progress -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("줌 끝");
        Camera.main.fieldOfView = 60f;
    }
}