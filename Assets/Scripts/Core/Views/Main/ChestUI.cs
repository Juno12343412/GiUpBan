using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.View;
using BackEnd;

public enum ChestState : byte
{
    Idle,
    Dismissing,
    Open,
    NONE = 99
}

[Serializable]
public class Chest
{
    public GameObject idleChest = null;
    public GameObject disChest = null;
    public GameObject openChest = null;

    public Image idleImage = null, disImage = null, openImage = null;
    public Text idleTimeText = null, disTimeText = null;
    public Text idleArenaText = null;
    public Text disDiamondText = null;

    public ChestKind chestKind = ChestKind.브론즈;
    public ChestState chestState = ChestState.Idle;

    public int disTime = 0;
    public int diamondPrice = 0;

    public DateTime startTime = DateTime.Now;
}

// 메인상자
public partial class MainUI : BaseScreen<MainUI>
{
    // Main
    [Header("Chest")]
    public Chest[] myChests = null;
    public int curHaveChests = 0;
    public int curSelectMyChest = 0;
    public int curDisChest = -1;
    // Main

    // Dismissing
    [Header("Dismissing")]
    [SerializeField] private Image disChestImg = null;
    [SerializeField] private Text disArenaText = null, disKindText = null;
    [SerializeField] private Text disGoldText = null, disCardText = null;
    [SerializeField] private Text disTimeText = null;
    [SerializeField] private Button disButton = null, openButton = null;
    // Dismissing

    // 아이템 리스트
    [SerializeField] private Text itemList = null;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            AddChest(ChestKind.예아, ChestState.Idle, "", curHaveChests);
    }

    public void ChestInit()
    {
        foreach (var chest in myChests)
        {
            chest.idleImage = chest.idleChest.transform.GetChild(0).GetComponent<Image>();
            chest.disImage = chest.disChest.transform.GetChild(0).GetComponent<Image>();
            chest.openImage = chest.openChest.transform.GetChild(0).GetComponent<Image>();

            chest.idleTimeText = chest.idleChest.transform.GetChild(1).GetComponent<Text>();
            chest.idleArenaText = chest.idleChest.transform.GetChild(2).GetComponent<Text>();

            chest.disTimeText = chest.disChest.transform.GetChild(0).GetComponent<Text>();
            chest.disDiamondText = chest.disChest.transform.GetChild(2).GetComponent<Text>();
        }
    }

    // 자신의 상자 설정
    public void AddChest(ChestKind kind, ChestState state = ChestState.Idle, string startTime = "", int index = 0)
    {
        if (curHaveChests == 3 || kind == ChestKind.NONE)
            return;

        myChests[index].chestKind = kind;
        myChests[index].chestState = state;

        if (startTime == "")
            myChests[index].startTime = DateTime.Now;
        else
            myChests[index].startTime = DateTime.Parse(startTime);

        myChests[index].disTime = 1;
        myChests[index].diamondPrice = ((int)kind + 1) * 10;

        myChests[index].idleTimeText.text = myChests[index].disTime + "분";

        myChests[index].disTimeText.text = myChests[index].disTime + "분";
        myChests[index].disDiamondText.text = myChests[index].diamondPrice + "D";

        // 아레나 설정 등등 해주기 ...

        switch (myChests[index].chestState)
        {
            case ChestState.Idle:
                myChests[index].idleChest.SetActive(true);
                myChests[index].disChest.SetActive(false);
                myChests[index].openChest.SetActive(false);
                break;
            case ChestState.Dismissing:
                myChests[index].idleChest.SetActive(false);
                myChests[index].disChest.SetActive(true);
                myChests[index].openChest.SetActive(false);
                StartCoroutine(CheckingChest(index));
                break;
            case ChestState.Open:
                myChests[index].idleChest.SetActive(false);
                myChests[index].disChest.SetActive(false);
                myChests[index].openChest.SetActive(true);
                break;
            default:
                break;
        }

        BackEndServerManager.instance.myInfo.haveChestKind[index] = (int)kind;
        BackEndServerManager.instance.myInfo.haveChestState[index] = (int)state;

        curHaveChests++;
        BackEndServerManager.instance.myInfo.haveChests = curHaveChests;
        BackEndServerManager.instance.myInfo.disStartTime = startTime;
    }

    // 해당 인덱스의 상자 체킹 (열 시간이 지났는가 안지났는가)
    public void CheckMyChest(int index)
    {
        DateTime endTime = DateTime.Now;
        TimeSpan timeCal = endTime - myChests[index].startTime;

        Debug.Log("시작 시간 : " + myChests[index].startTime + " / 현재 시간 : " + endTime + " / 남은 시간 : " + timeCal.Minutes + "분");

        myChests[index].disTimeText.text = ((myChests[index].disTime - 1) - timeCal.Minutes) + "분" + (60 - timeCal.Seconds) + "초"; 

        if (myChests[index].disTime <= timeCal.Minutes && myChests[index].chestState == ChestState.Dismissing)
        {
            myChests[index].chestState = ChestState.Open;
            myChests[index].idleChest.SetActive(false);
            myChests[index].disChest.SetActive(false);
            myChests[index].openChest.SetActive(true);

            BackEndServerManager.instance.myInfo.haveChestState[index] = (int)ChestState.Open;
            CheckDis();
        }

        BackEndServerManager.instance.myInfo.disChest = curDisChest;
    }

    public void OnDismissing()
    {
        if (myChests[curSelectMyChest].chestState == ChestState.Idle && curDisChest == -1)
        {
            chestDisObject.SetActive(false);

            curDisChest = curSelectMyChest;

            myChests[curSelectMyChest].chestState = ChestState.Dismissing;
            myChests[curSelectMyChest].startTime = DateTime.Now;

            myChests[curSelectMyChest].idleChest.SetActive(false);
            myChests[curSelectMyChest].disChest.SetActive(true);
            myChests[curSelectMyChest].openChest.SetActive(false);

            StartCoroutine(CheckingChest(curSelectMyChest));

            BackEndServerManager.instance.myInfo.haveChestState[curSelectMyChest] = (int)ChestState.Dismissing;
            BackEndServerManager.instance.myInfo.disStartTime = myChests[curSelectMyChest].startTime.ToString("yyyy/MM/dd hh:mm:ss");
            BackEndServerManager.instance.myInfo.disChest = curDisChest;
        }
    }

    public void OpenDismissing(int index)
    {
        curSelectMyChest = index;

        disKindText.text = myChests[index].chestKind.ToString();

        if (myChests[index].chestState == ChestState.Idle)
        {
            disButton.gameObject.SetActive(true);
            openButton.gameObject.SetActive(false);
            disTimeText.text = myChests[index].disTime.ToString() + "분";
        }
        else
        {
            disButton.gameObject.SetActive(false);
            openButton.gameObject.SetActive(true);
            disTimeText.text = "해제중";
        }

        chestDisObject.SetActive(true);
    }

    public void CloseDismissing()
    {
        chestDisObject.SetActive(false);
    }

    public void OpenChestUI()
    {
        OpenChest(curSelectMyChest);
    }

    public void OpenChest(int index)
    {
        curHaveChests--;

        chestDisObject.SetActive(false);
        chestOpenObject.SetActive(true);

        myChests[index].idleChest.SetActive(false);
        myChests[index].disChest.SetActive(false);
        myChests[index].openChest.SetActive(false);

        myChests[index].chestState = ChestState.NONE;

        BackEndServerManager.instance.myInfo.haveChestKind[index] = 99;
        BackEndServerManager.instance.myInfo.haveChestState[index] = 99;
        BackEndServerManager.instance.myInfo.disChest = curDisChest;
        BackEndServerManager.instance.myInfo.haveChests = curHaveChests;
        BackEndServerManager.instance.myInfo.disStartTime = "";

        CheckDis();

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

    public void OpenDiamondChest(ChestKind kind)
    {
        diamondChestDisObject.SetActive(false);
        chestOpenObject.SetActive(true);

        BackEndServerManager.instance.myInfo.diamond -= chests[curSelectChest].chestPrice;

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
        SetGoldUI();
    }

    public void CloseChest()
    {
        itemList.text = "";
        chestOpenObject.SetActive(false);
    }

    public IEnumerator CheckingChest(int index)
    {
        Debug.Log("체크 시작 : " + (GameManager.instance.gameState == GameManager.GameState.MatchLobby || myChests[index].chestState == ChestState.Open));

        while (GameManager.instance.gameState == GameManager.GameState.MatchLobby && myChests[index].chestState == ChestState.Dismissing)
        {
            CheckMyChest(index);
            yield return new WaitForSeconds(1f);
        }
    }

    public void CheckDis()
    {
        Debug.Log("체크 시작");
        foreach (var chest in myChests)
        {
            if (chest.chestState == ChestState.Dismissing)
            {
                return;
            }
        }
        Debug.Log("해제중인 상자 없음");
        curDisChest = -1;
        BackEndServerManager.instance.myInfo.disChest = curDisChest;
    }

    public void SettingMyChest()
    {
        if (BackEndServerManager.instance.myInfo.haveChests <= 0)
            return;

        Debug.Log("상자 셋팅 시작 : " + BackEndServerManager.instance.myInfo.haveChests);

        curDisChest = BackEndServerManager.instance.myInfo.disChest;

        for (int i = 0; i < 3; i++)
        {
            Debug.Log("상자 셋팅 중... " + i);
            if (BackEndServerManager.instance.myInfo.haveChestKind[i] != 99)
                AddChest((ChestKind)BackEndServerManager.instance.myInfo.haveChestKind[i], (ChestState)BackEndServerManager.instance.myInfo.haveChestState[i], BackEndServerManager.instance.myInfo.disStartTime, i);
        }
    }
}
