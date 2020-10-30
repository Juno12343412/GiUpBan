using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using BackEnd;
using BackEnd.Tcp;
using Manager.Pooling;

public class WorldPackage : MonoBehaviour
{
    static public WorldPackage instance;

    const int START_COUNT = 5;

    public SessionId myPlayerIndex = SessionId.None;

    public ObjectPool<PlayerScript> playerPool = new ObjectPool<PlayerScript>();
    public GameObject playerObj = null;

    private Dictionary<SessionId, PlayerScript> players = new Dictionary<SessionId, PlayerScript>();
    private const int MAX_PLAYER = 2;
    public int numOfPlayer = 0;
    private Vector3[] startingPoint = null;

    private Stack<SessionId> gameRecord = new Stack<SessionId>();
    public Action<SessionId> playerDie = null;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
    }

    void Start()
    {
        Init();
        playerPool.Init(playerObj, 2, Vector3.zero, Quaternion.identity, GameObject.Find("Objects").transform);
        if (BackEndMatchManager.instance.isReconnectProcess)
        {
            // 게임 시작전 하고 싶은 이벤트 작성
            // ...
        }
    }

    void Init()
    {
        Debug.Log("게임 초기화 진행");
        GameManager.instance.OnOver += OnGameOver;
        GameManager.instance.OnResult += OnGameResult;
        OnGameStart();
    }

    private void PlayerDieEvent(SessionId index)
    {
        players[index].gameObject.SetActive(false);

        //InGameUiManager.GetInstance().SetScoreBoard(alivePlayer);
        gameRecord.Push(index);

        // 호스트가 아니면 바로 리턴
        if (!BackEndMatchManager.instance.isHost)
        {
            return;
        }

        // 플레이어가 죽으면 바로 종료 체크
        SendGameEndOrder();
    }

    private void SendGameEndOrder()
    {
        // 게임 종료 전환 메시지는 호스트에서만 보냄
        Debug.Log("Make GameResult & Send Game End Order");
        foreach (SessionId session in BackEndMatchManager.instance.sessionIdList)
        {
            if (players[session].isActive && !gameRecord.Contains(session))
            {
                gameRecord.Push(session);
            }
        }
        GameEndMessage message = new GameEndMessage(gameRecord);
        BackEndMatchManager.instance.SendDataToInGame(message);
    }

    public void SetPlayerInfo()
    {
        if (BackEndMatchManager.instance.sessionIdList == null)
        {
            // 현재 세션ID 리스트가 존재하지 않으면, 0.5초 후 다시 실행
            Invoke("SetPlayerInfo", 0.5f);
            return;
        }
        var gamers = BackEndMatchManager.instance.sessionIdList;
        int size = gamers.Count;
        if (size <= 0)
        {
            Debug.Log("No Player Exist!");
            return;
        }
        if (size > MAX_PLAYER)
        {
            Debug.Log("Player Pool Exceed!");
            return;
        }

        players = new Dictionary<SessionId, PlayerScript>();
        BackEndMatchManager.instance.SetPlayerSessionList(gamers);

        int index = 0;
        foreach (var sessionId in gamers)
        {
            GameObject player = playerPool.Spawn(startingPoint[index]).gameObject;
            players.Add(sessionId, player.GetComponent<PlayerScript>());

            if (BackEndMatchManager.instance.IsMySessionId(sessionId))
            {
                myPlayerIndex = sessionId;
                players[sessionId].PlayerSetting(true, myPlayerIndex, BackEndMatchManager.instance.GetNickNameBySessionId(sessionId));
            }
            else
            {
                players[sessionId].PlayerSetting(true, sessionId, BackEndMatchManager.instance.GetNickNameBySessionId(sessionId));
            }
            index += 1;
        }

        if (BackEndMatchManager.instance.isHost)
        {
            // 시작 이벤트 설정 (방장만 호출)
            StartCoroutine("StartCount");
        }
    }

    public void OnGameStart()
    {
        if (BackEndMatchManager.instance.isHost)
        {
            Debug.Log("플레이어 세션정보 확인");

            if (BackEndMatchManager.instance.IsSessionListNull())
            {
                Debug.Log("Player Index Not Exist!");
                // 호스트 기준 세션데이터가 없으면 게임을 바로 종료한다.
                foreach (var session in BackEndMatchManager.instance.sessionIdList)
                {
                    // 세션 순서대로 스택에 추가
                    gameRecord.Push(session);
                }
                GameEndMessage gameEndMessage = new GameEndMessage(gameRecord);
                BackEndMatchManager.instance.SendDataToInGame(gameEndMessage);
                return;
            }
        }
        SetPlayerInfo();
    }

    IEnumerator StartCount()
    {
        StartCountMessage msg = new StartCountMessage(START_COUNT);

        // 카운트 다운
        for (int i = 0; i < START_COUNT + 1; ++i)
        {
            msg.time = START_COUNT - i;
            BackEndMatchManager.instance.SendDataToInGame(msg);
            yield return new WaitForSeconds(1); //1초 단위
        }

        // 게임 시작 메시지를 전송
        GameStartMessage gameStartMessage = new GameStartMessage();
        BackEndMatchManager.instance.SendDataToInGame(gameStartMessage);
    }

    public void OnGameOver()
    {
        Debug.Log("Game End");
        if (BackEndMatchManager.instance == null)
        {
            Debug.LogError("매치매니저가 null 입니다.");
            return;
        }
        BackEndMatchManager.instance.MatchGameOver(gameRecord);
    }

    public void OnGameResult()
    {
        Debug.Log("Game Result");
        //BackEndMatchManager.GetInstance().LeaveInGameRoom();

        if (GameManager.instance.gameState == GameManager.GameState.MatchLobby)
        {
            GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
        }
    }

    public void OnRecieve(MatchRelayEventArgs args)
    {
        if (args.BinaryUserData == null)
        {
            Debug.LogWarning(string.Format("빈 데이터가 브로드캐스팅 되었습니다.\n{0} - {1}", args.From, args.ErrInfo));
            // 데이터가 없으면 그냥 리턴
            return;
        }
        Message msg = DataParser.ReadJsonData<Message>(args.BinaryUserData);
        if (msg == null)
        {
            return;
        }
        if (BackEndMatchManager.instance.isHost != true && args.From.SessionId == myPlayerIndex)
        {
            return;
        }
        if (players == null)
        {
            Debug.LogError("Players 정보가 존재하지 않습니다.");
            return;
        }
        switch (msg.type)
        {
            case Protocol.Type.StartCount:
                // 아무것도 못하게 하기
                StartCountMessage startCount = DataParser.ReadJsonData<StartCountMessage>(args.BinaryUserData);
                break;
            case Protocol.Type.GameStart:
                // 플레이 가능하게 하기
                GameManager.instance.ChangeState(GameManager.GameState.InGame);
                break;
            case Protocol.Type.GameEnd:
                GameEndMessage endMessage = DataParser.ReadJsonData<GameEndMessage>(args.BinaryUserData);
                SetGameRecord(endMessage.count, endMessage.sessionList);
                GameManager.instance.ChangeState(GameManager.GameState.Over);
                break;
            case Protocol.Type.Idle:
                PlayerIdleMessage idleMsg = DataParser.ReadJsonData<PlayerIdleMessage>(args.BinaryUserData);
                ProcessPlayerData(idleMsg);
                break;
            case Protocol.Type.WeakAttack:
                PlayerWeakAttackMessage weakattackMsg = DataParser.ReadJsonData<PlayerWeakAttackMessage>(args.BinaryUserData);
                ProcessPlayerData(weakattackMsg);
                break;
            case Protocol.Type.StrongAttack:
                PlayerStrongAttackMessage strongattackMsg = DataParser.ReadJsonData<PlayerStrongAttackMessage>(args.BinaryUserData);
                ProcessPlayerData(strongattackMsg);
                break;
            case Protocol.Type.Defense:
                PlayerDefenseMessage defenseMsg = DataParser.ReadJsonData<PlayerDefenseMessage>(args.BinaryUserData);
                ProcessPlayerData(defenseMsg);
                break;
            case Protocol.Type.Stun:
                PlayerStunMessage stunMsg = DataParser.ReadJsonData<PlayerStunMessage>(args.BinaryUserData);
                ProcessPlayerData(stunMsg);
                break;
            case Protocol.Type.GameSync:
                GameSyncMessage syncMessage = DataParser.ReadJsonData<GameSyncMessage>(args.BinaryUserData);
                ProcessSyncData(syncMessage);
                break;
            default:
                Debug.Log("Unknown protocol type");
                return;
        }
    }

    private void ProcessPlayerData(PlayerIdleMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
    }

    private void ProcessPlayerData(PlayerWeakAttackMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
    }

    private void ProcessPlayerData(PlayerStrongAttackMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
    }

    private void ProcessPlayerData(PlayerDefenseMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
    }

    private void ProcessPlayerData(PlayerStunMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
    }

    public void OnRecieveForLocal(KeyMessage keyMessage)
    {
        // 키 이벤트 관리 함수
        // ...
    }

    private void ProcessSyncData(GameSyncMessage syncMessage)
    {
        // 플레이어 데이터 동기화
        int index = 0;
        if (players == null)
        {
            Debug.LogError("Player Poll is null!");
            return;
        }
        foreach (var player in players)
        {
            // 이 부분에서 모든 플레이어의 데이트를 동기화함
            // ...
            index++;
        }
        BackEndMatchManager.instance.SetHostSession(syncMessage.host);
    }

    private void SetGameRecord(int count, int[] arr)
    {
        gameRecord = new Stack<SessionId>();
        // 스택에 넣어야 하므로 제일 뒤에서 부터 스택에 push
        for (int i = count - 1; i >= 0; --i)
        {
            gameRecord.Push((SessionId)arr[i]);
        }
    }

    public GameSyncMessage GetNowGameState(SessionId hostSession)
    {
        // 플레이어 패킷 보내기용 함수
        int numOfClient = players.Count;
        int index = 0;

        bool[] online = new bool[numOfClient];
        foreach (var player in players)
        {
            index++;
        }

        return new GameSyncMessage(hostSession, numOfClient, null, online);
    }
}
