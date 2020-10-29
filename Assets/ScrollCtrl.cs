using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ScrollCtrl : MonoBehaviour , IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Slider tabSlider;
    public Transform contentTr;
    public Scrollbar scrollbar;
    public RectTransform[] BtnRect , BtnImageRect;
    const int SIZE = 3;
    float[] pos = new float[SIZE];
    float distance;
    bool isDrag;
    float targetPos = 0.5f;
    float curPos;
    public float ChangeSpeed = 0.1f;
    public float MoveSpeed = 18f;
    int targetIndex = 1;
    void Awake()
    {
    }
    void Start()
    {
        distance = 1f / (SIZE - 1);
        for (int i = 0; i < SIZE; i++) pos[i] = distance * i;
    }
    float SetPos() // 절반 거리를 기준으로 가까운 위치를 반환
    {
        for (int i = 0; i < SIZE; i++)
        {
            if (scrollbar.value < pos[i] + distance * 0.5f && scrollbar.value > pos[i] - distance * 0.5f)
            {
                targetIndex = i;
                return pos[i];
            }
        }
        return 0;
    }
    public void OnBeginDrag(PointerEventData eventData) => curPos = SetPos();

    public void OnDrag(PointerEventData eventData) => isDrag = true;

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;

        targetPos = SetPos();

        if(curPos == targetPos)
        {
            if(eventData.delta.x > MoveSpeed && curPos - distance >= 0)
            {
                --targetIndex;
                targetPos = curPos - distance;
            }
            else if (eventData.delta.x < -MoveSpeed && curPos + distance <= 1.01f)
            {
                ++targetIndex;
                targetPos = curPos + distance;
            }
        }

        for(int i = 0; i < SIZE; i++)
        {
            if(contentTr.GetChild(i).GetComponent<ScrollScript>() && curPos != pos[i] && targetPos == pos[i])
            {
                contentTr.GetChild(i).GetChild(1).GetComponent<Scrollbar>().value = 1;
            }
        }
    }

    void Update()
    {
        if (scrollbar.value <= 1)
            tabSlider.value = scrollbar.value - (0.16666f * targetIndex);
        else if(scrollbar.value > 1)
            tabSlider.value = 0.666f;
        else
            tabSlider.value = 0;

        if (!isDrag)
        {
            if (scrollbar.value <= 1)
                scrollbar.value = Mathf.Lerp(scrollbar.value, targetPos, ChangeSpeed);

            for (int i = 0; i < SIZE; i++) BtnRect[i].sizeDelta = new Vector2(i == targetIndex ? 540 : 270, BtnRect[i].sizeDelta.y);
        }
        if (Time.time < 0.2f) return;
        for(int i = 0; i < SIZE; i++)
        {
            Vector3 BtnTargetPos = BtnRect[i].anchoredPosition3D;
            Vector3 BtnTargetScale = Vector3.one;
            bool textActive = false;

            if(i == targetIndex)
            {
                BtnTargetPos.y = -44f;
                BtnTargetScale = new Vector3(1.2f, 1.2f, 1);
                textActive = true;
            }

            BtnImageRect[i].anchoredPosition3D = Vector3.Lerp(BtnImageRect[i].anchoredPosition3D, BtnTargetPos, 0.25f);
            BtnImageRect[i].localScale = Vector3.Lerp(BtnImageRect[i].localScale, BtnTargetScale, 0.25f);
            BtnImageRect[i].GetChild(0).gameObject.SetActive(textActive);
        }
    }

    public void TabClick(int n)
    {
        targetIndex = n;
        targetPos = pos[n];
    }


}
