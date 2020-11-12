using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
// Include Backend
using BackEnd;
using static BackEnd.SendQueue;


//서버의 접속까지의 기능이 구현되어있는 매니저
//- 게스트 로그인
//- 구글 로그인 / 미구현
//- 애플 로그인 / 미구현

public class BackEndServerManager : MonoBehaviour
{
    public static BackEndServerManager instance = null;
    public bool isLogin { get; private set; } = false;

    private string tempNickName;                        // 설정할 닉네임 (id와 동일)
    public string myNickName { get; private set; } = string.Empty;  // 로그인한 계정의 닉네임
    public string myIndate { get; private set; } = string.Empty;    // 로그인한 계정의 inDate

    public PlayerStats.Player myInfo = new PlayerStats.Player();

    private Action<bool, string> loginSuccessFunc = null;
    private const string BackendError = "statusCode : {0}\nErrorCode : {1}\nMessage : {2}";

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /*
	 * 서버 초기화
	 */
    void Start()
    {
        isLogin = true;
        Backend.Initialize(() =>
        {
            if (Backend.IsInitialized)
            {
                // 비동기 함수 큐 초기화
                Debug.Log("뒤끝 초기화 성공");

                if (IsInitialize == false)
                {
                    Debug.Log("큐 초기화 성공");
                    StartSendQueue(true);
                }
            }
            else
            {
                Debug.Log("뒤끝 초기화 실패");
            }
        });
    }

    // 게임 종료, 에디터 종료 시 호출
    // 비동기 큐 쓰레드를 중지시킴
    // 해당 함수는 실제 안드로이드, iOS 환경에서 호출이 안될 수도 있다 (각 os의 특징 때문)
    void OnApplicationQuit()
    {
        // 플레이어 정보 저장 ...
        PlayerStats.instance.Save();

        Debug.Log("OnApplicationQuit");
        StopSendQueue();
    }

    // 게임 시작, 게임 종료, 백그라운드로 돌아갈 때(홈버튼 누를 때) 호출됨
    // 위의 종료함수와는 달리 무조건 호출됨
    // 비동기 큐 종료, 재시작
    void OnApplicationPause(bool isPause)
    {
        Debug.Log("OnApplicationPause : " + isPause);
        if (isPause == false)
        {
            ResumeSendQueue();
        }
        else
        {
            PauseSendQueue();
        }
    }

    // 뒤끝 토큰으로 로그인
    public void BackendTokenLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.LoginWithTheBackendToken, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("토큰 로그인 성공");
                loginSuccessFunc = func;

                OnPrevBackendAuthorized();
                return;
            }

            Debug.Log("토큰 로그인 실패\n" + callback.ToString());

            func(false, string.Empty);
        });
    }

    public void UpdateNickname(string nickname, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.UpdateNickname, nickname, bro =>
        {
            // 닉네임이 없으면 매치서버 접속이 안됨
            if (!bro.IsSuccess())
            {
                Debug.LogError("닉네임 생성 실패\n" + bro.ToString());
                func(false, string.Format(BackendError,
                    bro.GetStatusCode(), bro.GetErrorCode(), bro.GetMessage()));
                return;
            }
            // 플레이어 정보 생성 ...
            PlayerStats.instance.Add();

            loginSuccessFunc = func;
            OnBackendAuthorized();
        });
    }

    // 유저 정보 불러오기 사전작업
    private void OnPrevBackendAuthorized()
    {
        isLogin = true;

        OnBackendAuthorized();
    }

    // 실제 유저 정보 불러오기
    private void OnBackendAuthorized()
    {
        Enqueue(Backend.BMember.GetUserInfo, callback =>
        {
            if (!callback.IsSuccess())
            {
                Debug.LogError("유저 정보 불러오기 실패\n" + callback);
                loginSuccessFunc(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                return;
            }
            Debug.Log("유저정보\n" + callback);

            var info = callback.GetReturnValuetoJSON()["row"];
            if (info["nickname"] == null)
            {
                LoginUI.instance.ActiveNickNameObject();
                return;
            }
            myNickName = info["nickname"].ToString();
            myIndate = info["inDate"].ToString();

            // 플레이어 정보 불러오기 ...
            PlayerStats.instance.Load();

            if (loginSuccessFunc != null)
            {
                BackEndMatchManager.instance.GetMatchList(loginSuccessFunc);
            }
        });
    }

    void Update()
    {
        Poll();
    }

    public void GuestLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.GuestLogin, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("게스트 로그인 성공");
                loginSuccessFunc = func;

                OnPrevBackendAuthorized();
                return;
            }

            Debug.Log("게스트 로그인 실패\n" + callback);
            Backend.BMember.DeleteGuestInfo();
            func(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
    }
}
