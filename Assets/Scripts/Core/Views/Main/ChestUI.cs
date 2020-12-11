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

[System.Serializable]
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

    public System.DateTime startTime = System.DateTime.Now;
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
    [SerializeField] private Text disKindText = null;
    [SerializeField] private Text disGoldText = null, disCardText = null;
    [SerializeField] private Text disTimeText = null;
    [SerializeField] private Button disButton = null, openButton = null;
    // Dismissing

    // ChestOpen
    [SerializeField] private Image chestImg = null;                // 상자 종류 이미지
    [SerializeField] private Image cardGradeImg = null;                // 카드 등급 이미지
    [SerializeField] private Image cardImg = null, etcImg = null; // 카드 종류 이미지
    [SerializeField] private Text cardText = null;                // 카드 이름
    [SerializeField] private Text cardCountText = null;                // 카드 개수
    [SerializeField] private Text chestItemCountText = null;                // 상자 남은 아이템 개수 
    // ChestOpen

    // ETC
    int count = 0;

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
            myChests[index].startTime = System.DateTime.Now;
        else
            myChests[index].startTime = System.DateTime.Parse(startTime);

        myChests[index].disTime = 1;
        myChests[index].diamondPrice = ((int)kind + 1) * 10;

        myChests[index].idleTimeText.text = myChests[index].disTime + "분";
        myChests[index].idleArenaText.text = myChests[index].chestKind.ToString();

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
        System.DateTime endTime = System.DateTime.Now;
        System.TimeSpan timeCal = endTime - myChests[index].startTime;

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
            myChests[curSelectMyChest].startTime = System.DateTime.Now;

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

            // ...
            disGoldText.text = (index + 1) * 100 + "C" + "-" + (index + 1) * 999 + "C";
            disCardText.text = "x" + (index + 1) * 5;
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

    // 다이아몬드 체스트
    public void OpenDiamondChestUI(int index)
    {
        count = index + 2;
        chestItemCountText.text = count.ToString();

        chestDisObject.SetActive(false);
        chestOpenObject.SetActive(true);

        chestImg.gameObject.SetActive(true);

        Invoke("ContinueOpenChest", 1.5f);
    }

    // 메인에서 잠금해제가 완료된 상자 터치할 때
    public void OpenChestUI()
    {
        count = curSelectMyChest + 2;
        chestItemCountText.text = count.ToString();

        chestDisObject.SetActive(false);
        chestOpenObject.SetActive(true);

        chestImg.gameObject.SetActive(true);

        Invoke("ContinueOpenChest", 1.5f);
    }

    // 그 다음에 상자를 열고 있을 때 남은 아이템들을 열어볼 때
    public void ContinueOpenChest()
    {
        OpenChest(curSelectMyChest);
    }

    public void OpenChest(int index)
    {
        chestItemCountText.text = (count - 1).ToString();

        if (count <= 0)
        {
            chestItemCountText.text = "0";

            chestImg.gameObject.SetActive(false);
            cardGradeImg.gameObject.SetActive(false);

            curHaveChests--;

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

            CloseChest();
            return;
        }
        else if (count == index + 2)
        {
            // 맨 처음 골드 부분
            int gold = Random.Range((index + 1) * 100, (index + 1) * 999);
            BackEndServerManager.instance.myInfo.gold += gold;

            cardText.text = "클로버";
            ShowResultCard(characterImgs.Length - 2, gold);
        }
        else if (count == 1)
        {
            // 확률적으로 다이아몬드
            if (Random.Range(0f, 101f) <= 30f)
            {
                // 다이아몬드
                int diamond = Random.Range(index + 1, (index + 1) * 5);
                BackEndServerManager.instance.myInfo.diamond += diamond;

                cardText.text = "다이아";
                ShowResultCard(characterImgs.Length - 1, diamond);
            }
            else
            {
                // 카드
                GiveCard(index);
            }
        }
        else
        {
            // 중간 부분 카드 줌
            GiveCard(index);
        }
        count--;
        //OpenChest(index, count);
    }

    public void OpenDiamondChest(ChestKind kind)
    {
        diamondChestDisObject.SetActive(false);
        OpenDiamondChestUI((int)kind);
        SetGoldUI();
    }

    public void CloseChest()
    {
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

    void GiveCard(int index)
    {
        SendQueue.Enqueue(Backend.Probability.GetProbability, "767", callback =>
        {
            // 그 다음 카드 부분
            if (callback.IsSuccess())
            {
                int card = -1;
                int count = Random.Range((index + 1) * 2, (index + 1) * 5);

                var log = callback.GetReturnValuetoJSON()["element"]["item"]["S"].ToString();
                Debug.Log(log);

                switch (log)
                {
                    case "나이트":
                        card = 0;
                        break;
                    case "벤전스":
                        card = 1;
                        break;
                    case "듀얼":
                        card = 2;
                        break;
                    case "도끼":
                        card = 3;
                        break;
                    default:
                        break;
                }

                if (CheckHaveCard(card))
                {
                    Debug.Log("캐릭터 있음 : " + card);
                    BackEndServerManager.instance.myInfo.levelExp[card] += count;
                }
                else
                {
                    Debug.Log("캐릭터 없음");
                    BackEndServerManager.instance.myInfo.haveCharacters.Add(card);
                    BackEndServerManager.instance.myInfo.charactersLevel.Add(1);
                    BackEndServerManager.instance.myInfo.levelExp.Add(-1);

                    var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == card);
                    BackEndServerManager.instance.myInfo.levelExp[value] += count;
                }

                cardText.text = log;
                ShowResultCard(card, count);

                SetInventory();
            }
            else
                Debug.Log("실패 !");
        });
    }

    // 상자 열때 무슨 아이템 나왔는지 보여주는 함수
    void ShowResultCard(int index, int count)
    {
        Debug.Log("아이템 개수 : " + count);

        // 애니메이션 재생을 위함
        cardGradeImg.gameObject.SetActive(false);
        cardGradeImg.gameObject.SetActive(true);
        // 애니메이션 재생을 위함

        cardImg.gameObject.SetActive(false);
        etcImg.gameObject.SetActive(false);
        if (index == characterImgs.Length - 2) // 골드 일경우
        {
            cardCountText.text = count + "C";
            etcImg.sprite = characterImgs[index];
            etcImg.gameObject.SetActive(true);
        }
        else if (index == characterImgs.Length - 1)
        {
            cardCountText.text = count + "D";
            etcImg.sprite = characterImgs[index];
            etcImg.gameObject.SetActive(true);
        }
        else
        {
            cardCountText.text = "x" + count;
            cardImg.sprite = characterImgs[index];
            cardImg.gameObject.SetActive(true);
        }

        // 상자랑 희귀도 차이도 나중에 나눠야됨
    }
}