using UnityEngine;
using UnityEngine.UI;
using Manager.Dispatcher;
using Manager.View;
using Manager.Scene;

public class LoginUI : BaseScreen<LoginUI>
{
    [SerializeField] private GameObject titleObject = null;
    [SerializeField] private GameObject loginObject = null;
    [SerializeField] private GameObject nicknameObject = null;
    [SerializeField] private GameObject loadingObject = null;
    [SerializeField] private GameObject errorObject = null;
    [SerializeField] private Text       errorText = null;

    private InputField nicknameField = null;

    private const string VERSION_STR = "Ang gi mo ddi ver {0}";

    void Start()
    {
        titleObject.SetActive(true);
        loginObject.SetActive(true);
        nicknameObject.SetActive(false);
        loadingObject.SetActive(false);
        errorObject.SetActive(false);

        nicknameField = nicknameObject.GetComponentInChildren<InputField>();
        titleObject.GetComponentInChildren<Text>().text = string.Format(VERSION_STR, Application.version);

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
                    errorText.text = "유저 정보 불러오기 실패\n\n" + error;
                    errorObject.SetActive(true);                    
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
            errorObject.GetComponent<Text>().text = "닉네임을 먼저 입력해주세요";
            errorObject.SetActive(true);
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
                    errorObject.GetComponent<Text>().text = "닉네임 생성 오류\n\n" + error;
                    errorObject.SetActive(true);
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
                    errorText.text = "로그인 에러\n\n" + error;
                    errorObject.SetActive(true);
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

    public override void ShowScreen()
    {
        base.ShowScreen();
    }

    public override void HideScreen()
    {
        base.HideScreen();
    }
}
