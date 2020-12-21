using UnityEngine;
using UnityEngine.UI;
using Manager.Dispatcher;
using Manager.View;
using System.Collections;

public class LoginUI : BaseScreen<LoginUI>
{
    [SerializeField] private GameObject titleObject = null;
    [SerializeField] private GameObject loginObject = null;
    [SerializeField] private GameObject nicknameObject = null;
    [SerializeField] private GameObject loadingObject = null;
    [SerializeField] private GameObject errorObject = null;
    [SerializeField] private Text       errorText = null;
    [SerializeField] private Text broadcastText = null;

    private InputField nicknameField = null;

    private const string VERSION_STR = "ver {0}";

    void Start()
    {
        titleObject.SetActive(true);
        loginObject.SetActive(true);
        nicknameObject.SetActive(false);
        loadingObject.SetActive(false);
        errorObject.SetActive(false);

        nicknameField = nicknameObject.GetComponentInChildren<InputField>();
        titleObject.transform.GetChild(0).GetComponent<Text>().text = string.Format(VERSION_STR, Application.version);

        StartGame();
    }

    void StartGame()
    {
        loadingObject.SetActive(true);
        BackEndServerManager.instance.BackendTokenLogin((bool result, string error) =>
        {
            Debug.Log("함수 실행");
            Dispatcher.Current.BeginInvoke(() =>
            {
                Debug.Log("유저 정보 불러오는 중...");
                loadingObject.SetActive(false);
                if (result)
                {
                    Debug.Log("유저 정보 불러오기 성공 ..!");
                    GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
                    return;
                }
                if (!error.Equals(string.Empty))
                {
                    Debug.Log("유저 정보 불러오기 실패 ..!");
                    StartCoroutine(OnShowBroadCast("다시시도 해주세요"));
                    return;
                }
            });
        });
    }

    public void ActiveNickNameObject()
    {
        Dispatcher.Current.BeginInvoke(() =>
        {
            nicknameObject.SetActive(true);
            loginObject.SetActive(false);
            errorObject.SetActive(false);
            loadingObject.SetActive(false);
        });
    }

    public void UpdateNickName()
    {
        if (errorObject.activeSelf)
        {
            return;
        }
        string nickname = nicknameField.text;
        if (nickname.Equals(string.Empty))
        {
            StartCoroutine(OnShowBroadCast("닉네임을 입력해주세요"));
            return;
        }
        loadingObject.SetActive(true);
        BackEndServerManager.instance.UpdateNickname(nickname, (bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    loadingObject.SetActive(false);
                    StartCoroutine(OnShowBroadCast("닉네임 생성 오류"));
                    return;
                }
                GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
            });
        });
    }

    public void GoogleFederation()
    {
        if (errorObject.activeSelf)
        {
            return;
        }

        loadingObject.SetActive(true);
        BackEndServerManager.instance.GoogleAuthorizeFederation((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                loadingObject.SetActive(false);
                if (!result)
                {
                    StartCoroutine(OnShowBroadCast("다시시도 해주세요"));
                    return;
                }
                GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
            });
        });
    }

    public void GuestLogin()
    {
        if (errorObject.activeSelf)
        {
            return;
        }

        loadingObject.SetActive(true);
        BackEndServerManager.instance.GuestLogin((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                loadingObject.SetActive(false);
                if (!result)
                {
                    StartCoroutine(OnShowBroadCast("다시시도 해주세요"));
                    return;
                }
                GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
            });
        });
    }

    public void OpenRoomUI()
    {
        // 매치 서버에 대기방 생성 요청
        //if (BackEndMatchManager.GetInstance().CreateMatchRoom() == true)
        //{
        //    SetLoadingObjectActive(true);
        //}
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

    public override void ShowScreen()
    {
        base.ShowScreen();
    }

    public override void HideScreen()
    {
        base.HideScreen();
    }

}
