using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollScript : ScrollRect
{
    bool forParent;
    ScrollCtrl SC;
    ScrollRect parentScrollRect;

    protected override void Start()
    {
        SC = GameObject.FindWithTag("ScrollCtrl").GetComponent<ScrollCtrl>();
        parentScrollRect = GameObject.FindWithTag("ScrollCtrl").GetComponent<ScrollRect>();
    }
    public override void OnBeginDrag(PointerEventData eventData)
    {
        forParent = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);

        if (forParent)
        {
            SC.OnBeginDrag(eventData);
            parentScrollRect.OnBeginDrag(eventData);
        }
        else base.OnBeginDrag(eventData);
    }
    public override void OnDrag(PointerEventData eventData)
    {
        if (forParent)
        {
            SC.OnDrag(eventData);
            parentScrollRect.OnDrag(eventData);
        }
        else base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (forParent)
        {
            SC.OnEndDrag(eventData);
            parentScrollRect.OnEndDrag(eventData);
        }
        else base.OnEndDrag(eventData);
    }
}
