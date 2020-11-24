using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager.Scene;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private static bool isCreate = false;

    public enum GameState { Login, MatchLobby, Ready, GameStart, InGame, Over, Result, Reconnect };
    [HideInInspector] public GameState gameState;

    public event Action OnReady = null;
    public event Action OnStart = null;
    public event Action OnIngame = null;
    public event Action OnOver = null;
    public event Action OnResult = null;
    public event Action OnReconnect = null;

    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        // 60프레임 고정
        Application.targetFrameRate = 60;
        // 게임중 슬립모드 해제
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //InGameUpdateCoroutine = InGameUpdate();
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (isCreate)
        {
            DestroyImmediate(gameObject, true);
            return;
        }
        gameState = GameState.Login;
        isCreate = true;
    }

    void Login()
    {
        Loader.Load(Scene.Login);
    }

    void MatchLobby()
    {
        Loader.Load(Scene.Main);
    }

    void Ready()
    {
        OnReady();
    }

    void GameStart()
    {
        Loader.Load(Scene.Game);
        //OnStart();
    }

    void InGame()
    {
        OnIngame();
    }

    void Over()
    {
        OnOver();
    }

    void Result()
    {
        OnResult();
    }

    void Reconnect()
    {
        OnReconnect();
    }

    public void ChangeState(GameState state)
    {
        gameState = state;
        switch (gameState)
        {
            case GameState.Login:
                Login();
                break;
            case GameState.MatchLobby:
                MatchLobby();
                break;
            case GameState.Ready:
                Ready();
                break;
            case GameState.GameStart:
                GameStart();
                break;
            case GameState.InGame:
                InGame();
                break;
            case GameState.Over:
                Over();
                break;
            case GameState.Result:
                Result();
                break;
            case GameState.Reconnect:
                Reconnect();
                break;
            default:
                Debug.Log("알수없는 상태입니다.");
                break;
        }
    }
}
