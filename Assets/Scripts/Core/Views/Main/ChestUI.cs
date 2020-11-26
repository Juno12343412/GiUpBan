using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.View;
using BackEnd;

// 상자
public partial class MainUI : BaseScreen<MainUI>
{
    [Header("Chest")]
    [SerializeField] private Text itemList = null;

    public void OpenChest()
    {
        chestOpenObject.SetActive(true);

        SendQueue.Enqueue(Backend.Probability.GetProbability, "634", callback =>
        {
            if (callback.IsSuccess())
            {
                var log = callback.GetReturnValuetoJSON()["element"]["item"]["S"].ToString();
                Debug.Log(log);
                itemList.text = log;
            }
            else
                Debug.Log("실패 !");
        });
    }

    public void CloseChest()
    {
        itemList.text = "";
        chestOpenObject.SetActive(false);
    }
}
