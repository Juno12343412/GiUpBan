using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackEnd.Tcp;
using BackEnd;
using Manager.View;
using Manager.Sound;

public partial class MainUI : BaseScreen<MainUI>
{
    [Header("Match")]
    [SerializeField] private GameObject matchlookingObject = null;
    [SerializeField] private GameObject matchFoundObject = null;
    [SerializeField] private GameObject matchReconnectObject = null;
    [SerializeField] private Text matchReconnectMMR = null;
    [SerializeField] private GameObject loadingObject = null;
    [SerializeField] private GameObject errorObject = null;
    [SerializeField] private Text errorText = null;

    [Header("Card")]
    [SerializeField] private GameObject cardUpgrade = null;
    [SerializeField] private GameObject cardPurchase = null;

    [Header("Chest")]
    [SerializeField] private GameObject chestOpenObject = null;
    [SerializeField] private GameObject chestDisObject = null;
    [SerializeField] private GameObject diamondChestDisObject = null;

    [Header("Main")]
    [SerializeField] private Text nameText = null;
    [SerializeField] private GameObject textObject = null;
    [SerializeField] private Image tearImage = null;
    [SerializeField] private Text tearText = null;
    [SerializeField] private Text broadcastText = null;

    [Header("Ranking")]
    [SerializeField] private GameObject rankObject = null;

    [Header("Ads")]
    [SerializeField] private GameObject noAds = null;

    [HideInInspector] public List<string> readyUserList = new List<string>();

    void Start()
    {
        Time.timeScale = 1;

        SoundPlayer.instance.PlayBGM("MainBgm", 0.5f);

        matchlookingObject.SetActive(false);
        matchFoundObject.SetActive(false);
        matchReconnectObject.SetActive(false);
        loadingObject.SetActive(false);
        errorObject.SetActive(false);

        cardUpgrade.SetActive(false);
        cardPurchase.SetActive(false);
        rankObject.SetActive(false);

        BackEndMatchManager.instance.JoinMatchServer();
        BackEndMatchManager.instance.HandlerSetting();

        BackEndServerManager.instance.myInfo.oldPoint = BackEndServerManager.instance.myInfo.point;

        Invoke("StartDataSetting", 0.25f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            AddChest((ChestKind)Random.Range((int)ChestKind.한, (int)ChestKind.MAX), ChestState.Idle, "", BackEndServerManager.instance.myInfo.haveChestState.FindIndex(value => value == (int)ChestState.NONE));

    }

    private void SetNickName()
    {
        var name = BackEndServerManager.instance.myNickName;
        if (name.Equals(string.Empty))
        {
            Debug.LogError("닉네임 불러오기 실패");
            name = "test123";
        }
        nameText.text = name + " | " + (CharacterKind)BackEndServerManager.instance.myInfo.nowCharacter;
    }

    public void OpenMatchUI()
    {
        SoundPlayer.instance.PlaySound("Click");

        // 매치 서버에 대기방 생성 요청
        if (BackEndMatchManager.instance.CreateMatchRoom() == true)
        {
            Debug.Log("방생성 로딩중 ...");
            loadingObject.SetActive(true);
            //BackEndMatchManager.instance.MatchMakingHandler();
        }
    }

    public void CloseMatchUI()
    {
        MatchCancelCallback();
    }

    public void SetTextUI(bool state)
    {
        textObject.SetActive(state);
    }

    public void SetReconnectUI(bool state)
    {
        if (state)
            matchReconnectMMR.text = BackEndServerManager.instance.myNickName + " (" + BackEndServerManager.instance.myInfo.point.ToString() + ")";

        matchReconnectObject.SetActive(state);
    }

    public void CreateRoomResult(bool isSuccess, List<MatchMakingUserInfo> userList = null)
    {
        // 대기 방 생성에 성공 시 대기방 UI를 활성화 시키고,
        if (isSuccess == true)
        {
            Debug.Log("방 생성 성공 !");
            loadingObject.SetActive(false);
            SetTextUI(false);
            ResetReadyPlayer();
            AddReadyPlayer(BackEndServerManager.instance.myNickName);
            ResetMatchRoom(userList);
            RequestMatch();
        }
        // 대기 방 생성에 실패 시 에러를 띄움
        else
        {
            // 실패 에러를 띄움
            // ...
            ResetReadyPlayer();
            loadingObject.SetActive(false);
            Debug.Log("방 생성 실패 !");
        }
    }

    public void ResetMatchRoom(List<MatchMakingUserInfo> userList = null)
    {
        // 대기방에 들어왔을 경우
        // UI 열어주고 플레이어 대기 그리고 플레이어 현재 현황도 써줌
        MatchRequestCallback(true);
        loadingObject.SetActive(true);

        if (userList != null)
            matchlookingObject.GetComponentsInChildren<Text>()[1].text = userList.Count.ToString() + " / " + BackEndMatchManager.instance.matchInfos[0].headCount;
        else
            matchlookingObject.GetComponentsInChildren<Text>()[1].text = readyUserList.Count.ToString() + " / " + BackEndMatchManager.instance.matchInfos[0].headCount;
    }

    public void JoinMatchProcess()
    {
        // 매치 참가
        BackEndMatchManager.instance.JoinMatchServer();
    }

    public void LeaveReadyRoom()
    {
        loadingObject.SetActive(false);
        BackEndMatchManager.instance.CancelRegistMatchMaking();
    }

    public void AddReadyPlayer(string nickname)
    {
        readyUserList.Add(nickname);
    }

    public void RemoveReadyPlayer(string nickname)
    {
        readyUserList.Remove(nickname);
    }

    public void ResetReadyPlayer()
    {
        readyUserList.Clear();
    }

    public void RequestMatch()
    {
        if (errorObject.activeSelf || matchFoundObject.activeSelf)
        {
            return;
        }

        // Random
        //BackEndMatchManager.instance.RequestMatchMaking(0);

        // MMR
        BackEndMatchManager.instance.RequestMatchMaking(1);
    }

    public void MatchRequestCallback(bool result)
    {
        if (!result)
        {
            loadingObject.SetActive(false);
            matchlookingObject.SetActive(false);
            return;
        }

        matchlookingObject.SetActive(true);
    }

    public void MatchDoneCallback()
    {
        SoundPlayer.instance.StopBGM();
        SoundPlayer.instance.PlaySound("FindMatching");

        matchlookingObject.SetActive(false);
        matchFoundObject.SetActive(true);
    }

    public void MatchCancelCallback()
    {
        matchlookingObject.SetActive(false);
        matchFoundObject.SetActive(false);
        loadingObject.SetActive(false);
    }

    public void SetErrorLog(string log = "")
    {
        errorObject.SetActive(true);
        errorText.text = log;
    }

    public void LogOutID()
    {
        BackEndMatchManager.instance.LeaveMatchServer();
        Backend.BMember.Logout();
        GameManager.instance.ChangeState(GameManager.GameState.Login);
    }

    public void StartDataSetting()
    {
        InventoryInit();
        ShopInit();
        ChestInit();

        if (BackEndServerManager.instance.myInfo.giveChest)
        {
            Debug.Log("보상 : " + BackEndServerManager.instance.myInfo.haveChestState.FindIndex(value => value == (int)ChestState.NONE));
            BackEndServerManager.instance.myInfo.giveChest = false;
            AddChest((ChestKind)Random.Range((int)ChestKind.한, (int)ChestKind.골드), ChestState.Idle, "", BackEndServerManager.instance.myInfo.haveChestState.FindIndex(value => value == (int)ChestState.NONE));
        }

        StartCoroutine(CheckingDay());

        if (BackEndServerManager.instance.myInfo.haveChests > 0)
        {
            SettingMyChest();
            if (curDisChest != -1)
                StartCoroutine(CheckingChest(curDisChest));
        }

        if (!BackEndServerManager.instance.myInfo.cardKind.Contains(99))
            ResetShopItems();
        else
            SetShopItems();

        SetNickName();
        SetTear();
        SetInventory();

        if (BackEndServerManager.instance.myInfo.ads)
            noAds.SetActive(false);

        if (BackEndServerManager.instance.myInfo.pack)
            speHideObj.SetActive(true);

        PlayerStats.instance.Save();
    }

    public void SetTear()
    {
        if (GetPointToRank(BackEndServerManager.instance.myInfo.oldPoint) != GetPointToRank(BackEndServerManager.instance.myInfo.point))
            ShowTearUp();

        tearText.text = BackEndServerManager.instance.myInfo.point.ToString();
        tearImage.sprite = tearImages[(int)GetPointToRank(BackEndServerManager.instance.myInfo.point)];
    }

    public IEnumerator OnShowBroadCast(string text = "")
    {
        if (!broadcastText.gameObject.activeSelf)
        {
            broadcastText.text = text;
            broadcastText.gameObject.SetActive(true);

            yield return new WaitForSeconds(1f);
            broadcastText.gameObject.SetActive(false);
        }
    }

    public void BuyNoAddInApp()
    {
        BackEndServerManager.instance.myInfo.ads = true;
        noAds.SetActive(false);
    }
}
