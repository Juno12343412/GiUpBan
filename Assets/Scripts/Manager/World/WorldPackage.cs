using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Protocol;
using BackEnd;
using BackEnd.Tcp;
using Manager.Pooling;

public class WorldPackage : MonoBehaviour
{
    [SerializeField] private Image[] hpImages = null;

    static public WorldPackage instance;

    const int START_COUNT = 5;

    public SessionId myPlayerIndex = SessionId.None;
    public SessionId otherPlayerIndex = SessionId.None;

    public bool myAttackPoint = false;

    public ObjectPool<PlayerScript> playerPool = new ObjectPool<PlayerScript>();
    public GameObject playerObj = null;

    private Dictionary<SessionId, PlayerScript> players = new Dictionary<SessionId, PlayerScript>();
    private const int MAX_PLAYER = 2;
    public int numOfPlayer = 0;
    public Transform[] startingPoint = null;

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
        if (BackEndMatchManager.instance.isReconnectProcess)
        {
            // 게임 시작전 하고 싶은 이벤트 작성
            // ...
        }
    }

    void Init()
    {
        Debug.Log("게임 초기화 진행");
        playerPool.Init(playerObj, 2, Vector3.zero, Quaternion.identity, GameObject.Find("Objects").transform);
        GameManager.instance.OnOver += OnGameOver;

        playerDie += PlayerDieEvent;
        OnGameStart();
    }

    private void PlayerDieEvent(SessionId index)
    {
        players[index].gameObject.SetActive(false);
        gameRecord.Push(index);

        // 호스트가 아니면 바로 리턴
        if (BackEndMatchManager.instance.isHost)
        {
            Debug.Log(players[index] + " Die");
            // 플레이어가 죽으면 바로 종료 체크
            SendGameEndOrder();
        }
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
            GameObject player = playerPool.Spawn().gameObject;
            players.Add(sessionId, player.GetComponent<PlayerScript>());

            if (BackEndMatchManager.instance.IsMySessionId(sessionId))
            {
                myPlayerIndex = sessionId;
                players[sessionId].PlayerSetting(true, myPlayerIndex, BackEndMatchManager.instance.GetNickNameBySessionId(sessionId));
            }
            else
            {
                otherPlayerIndex = sessionId;
                players[sessionId].PlayerSetting(false, sessionId, BackEndMatchManager.instance.GetNickNameBySessionId(sessionId));
            }
            index += 1;
        }

        if (BackEndMatchManager.instance.isHost)
        {
            // 이제 적의 데이터를 가져오는 패킷을 호출
            var message = instance.GetNowGameState(BackEndMatchManager.instance.hostSession);
            BackEndMatchManager.instance.SendDataToInGame(message);

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
        //if (BackEndMatchManager.instance.isHost != true && args.From.SessionId == myPlayerIndex)
        //{
        //    Debug.Log("패킷 받기 안됨");
        //    return;
        //}
        if (players == null)
        {
            Debug.Log("패킷 받기 안됨");
            return;
        }

        Debug.Log("패킷 받기 : " + (int)msg.type);
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
            case Protocol.Type.AttackEnd:
                PlayerAttackEndMessage endMsg = DataParser.ReadJsonData<PlayerAttackEndMessage>(args.BinaryUserData);
                ProcessPlayerData(endMsg);
                break;
            case Protocol.Type.GameSync:
                GameSyncMessage syncMessage = DataParser.ReadJsonData<GameSyncMessage>(args.BinaryUserData);
                ProcessSyncData(syncMessage);
                break;
            case Protocol.Type.Calculation:
                CalculationMessage calMessage = DataParser.ReadJsonData<CalculationMessage>(args.BinaryUserData);
                ProcessCalData(calMessage);
                break;
            case Protocol.Type.Damaged:
                PlayerDamagedMessage damMessage = DataParser.ReadJsonData<PlayerDamagedMessage>(args.BinaryUserData);
                ProcessPlayerData(damMessage);
                break;
            default:
                Debug.Log("Unknown protocol type");
                return;
        }
    }

    // 아무것도 안한 상태
    private void ProcessPlayerData(PlayerIdleMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
    }

    // 약한 공격 상태
    private void ProcessPlayerData(PlayerWeakAttackMessage data)
    {
        Debug.Log(data.playerSession);
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
        Debug.Log(BackEndMatchManager.instance.GetNickNameBySessionId(data.playerSession) + "가 " + data.playerDirection + "쪽으로 약한 공격을 함");
        players[data.playerSession].AnimationReset();
        if (players[data.playerSession].Direction == Direction.Left)
            players[data.playerSession].Anim.SetInteger("AttackDir", 0);
        if (players[data.playerSession].Direction == Direction.Right)
            players[data.playerSession].Anim.SetInteger("AttackDir", 1);
        players[data.playerSession].State = PlayerCurState.WEAK_ATTACK;

        players[data.playerSession].Anim.SetInteger("AttackKind", 1);
        players[data.playerSession].Anim.SetBool("isAttack", true);
    }

    // 강한 공격 상태
    private void ProcessPlayerData(PlayerStrongAttackMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
        players[data.playerSession].AnimationReset();
        if (players[data.playerSession].Direction == Direction.Left)
            players[data.playerSession].Anim.SetInteger("AttackDir", 0);
        if (players[data.playerSession].Direction == Direction.Right)
            players[data.playerSession].Anim.SetInteger("AttackDir", 1);
        players[data.playerSession].State = PlayerCurState.STRONG_ATTACK;


        players[data.playerSession].Anim.SetInteger("AttackKind", 2);
        players[data.playerSession].Anim.SetBool("isAttack", true);
    }

    // 방어 상태
    private void ProcessPlayerData(PlayerDefenseMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
        players[data.playerSession].AnimationReset();
        if (players[data.playerSession].Direction == Direction.Left)
            players[data.playerSession].Anim.SetInteger("AttackDir", 0);
        if (players[data.playerSession].Direction == Direction.Right)
            players[data.playerSession].Anim.SetInteger("AttackDir", 1);
        players[data.playerSession].State = PlayerCurState.DEFENSE;

        players[data.playerSession].Anim.SetInteger("AttackKind", 3);
        players[data.playerSession].Anim.SetBool("isAttack", true);
    }

    // 스턴 상태
    private void ProcessPlayerData(PlayerStunMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
    }

    // 공격 종료 상태
    private void ProcessPlayerData(PlayerAttackEndMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
        players[data.playerSession].Anim.SetBool("isAttack", false);
        players[data.playerSession].AttackPointFalse();
        players[data.playerSession].CancelFalse();

    }

    private void ProcessPlayerData(PlayerDamagedMessage data)
    {
        //players[data.playerSession] <- 이걸 통해서 그 플레이어 세션에 맞는 플레이어의 함수를 실행시키게함
        Debug.Log("데미지넣음" + data.damage);
        Debug.Log("넣기전 체력" + players[data.playerSession].Stats.CurHp);
        players[data.playerSession].Stats.CurHp -= data.damage;
        Debug.Log("넣은후 체력" + players[data.playerSession].Stats.CurHp);
        hpImages[1].fillAmount = (float)(players[otherPlayerIndex].Stats.CurHp / players[otherPlayerIndex].Stats.MaxHp);
        hpImages[0].fillAmount = (float)(players[myPlayerIndex].Stats.CurHp / players[myPlayerIndex].Stats.MaxHp);
    }

    public void OnRecieveForLocal(KeyMessage msg)
    {
        // 키 이벤트 관리 함수
        ProcessKeyEvent(myPlayerIndex, msg);

    }

    private void ProcessKeyEvent(SessionId index, KeyMessage keyMsg)
    {
        Debug.Log("2-1");

        if (!BackEndMatchManager.instance.isHost)
            return;

        int keyData = keyMsg.keyData;

        if ((keyData & KeyEventCode.IDLE) == KeyEventCode.IDLE)
        {
            PlayerIdleMessage msg = new PlayerIdleMessage(index);
            BackEndMatchManager.instance.SendDataToInGame(msg);
        }
        if ((keyData & KeyEventCode.WEAK_ATTACK) == KeyEventCode.WEAK_ATTACK)
        {
            PlayerWeakAttackMessage msg = new PlayerWeakAttackMessage(index, keyMsg.direction);
            BackEndMatchManager.instance.SendDataToInGame(msg);
        }
        if ((keyData & KeyEventCode.STRONG_ATTACK) == KeyEventCode.STRONG_ATTACK)
        {
            PlayerStrongAttackMessage msg = new PlayerStrongAttackMessage(index, keyMsg.direction);
            BackEndMatchManager.instance.SendDataToInGame(msg);
        }
        if ((keyData & KeyEventCode.DEFENSE) == KeyEventCode.DEFENSE)
        {
            PlayerDefenseMessage msg = new PlayerDefenseMessage(index, keyMsg.direction);
            BackEndMatchManager.instance.SendDataToInGame(msg);
        }
        if ((keyData & KeyEventCode.STUN) == KeyEventCode.STUN)
        {
            PlayerStunMessage msg = new PlayerStunMessage(index);
            BackEndMatchManager.instance.SendDataToInGame(msg);
        }
        if ((keyData & KeyEventCode.ATTACK_END) == KeyEventCode.ATTACK_END)
        {
            PlayerAttackEndMessage msg = new PlayerAttackEndMessage(index);
            BackEndMatchManager.instance.SendDataToInGame(msg);
        }
        //if ((keyData & KeyEventCode.DAMAGED) == KeyEventCode.DAMAGED)
        //{
        //    PlayerDamagedMessage msg = new PlayerDamagedMessage(index);
        //    BackEndMatchManager.instance.SendDataToInGame(msg);
        //}

        Debug.Log("2-2 상태 : " + keyData);
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
            if (!player.Value.isMe)
            {
                player.Value.Stats.MaxHp = syncMessage.MaxHp[index];
                player.Value.Stats.CurHp = syncMessage.MaxHp[index];
                // 코드값에 따라 아이템을 찾아서 넣어주는 함수 추가바람 ... 
            }

            index++;
        }
        BackEndMatchManager.instance.SetHostSession(syncMessage.host);
    }

    private void ProcessCalData(CalculationMessage calMessage)
    {
        if (players[otherPlayerIndex].State == PlayerCurState.WEAK_ATTACK && players[otherPlayerIndex].GetAttackPoint() == true)
        {
            if (players[myPlayerIndex].State == PlayerCurState.DEFENSE && players[myPlayerIndex].Direction == players[otherPlayerIndex].Direction)
            {
                players[myPlayerIndex].SufferDamage(players[otherPlayerIndex].Stats.Damage * 0.1f);

                //players[otherPlayerIndex].DelayOn(players[otherPlayerIndex].Stats.curWeapon.AttackDelay);
                players[otherPlayerIndex].AttackPointFalse();
                return;
            }
            else
            {
                players[myPlayerIndex].SufferDamage(players[otherPlayerIndex].Stats.Damage);

                players[otherPlayerIndex].AttackPointFalse();
                Debug.Log("약공 들어감");
                return;

            }
        } // 약공

        if (players[otherPlayerIndex].State == PlayerCurState.STRONG_ATTACK && players[otherPlayerIndex].GetAttackPoint() == true)
        {
            if (players[myPlayerIndex].State == PlayerCurState.DEFENSE && players[myPlayerIndex].Direction == players[otherPlayerIndex].Direction)
            {
                players[myPlayerIndex].SufferDamage(players[otherPlayerIndex].Stats.Damage * 0.1f);


                //players[otherPlayerIndex].DelayOn(players[otherPlayerIndex].Stats.curWeapon.AttackDelay);
                players[otherPlayerIndex].AttackPointFalse();

            }
            else
            {
                players[myPlayerIndex].SufferDamage(players[otherPlayerIndex].Stats.Damage * 1.5f);

                players[otherPlayerIndex].AttackPointFalse();
                Debug.Log("강공 들어감");

            }
        } // 강공
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

        double[] curHp = new double[numOfClient], maxHp = new double[numOfClient], stamina = new double[numOfClient],
            staminaM = new double[numOfClient], damage = new double[numOfClient], penetration = new double[numOfClient];
        bool[] online = new bool[numOfClient];

        foreach (var player in players)
        {
            maxHp[index] = player.Value.Stats.MaxHp;
            curHp[index] = player.Value.Stats.CurHp;
            stamina[index] = player.Value.Stats.Stamina;
            staminaM[index] = player.Value.Stats.StaminaM;
            damage[index] = player.Value.Stats.Damage;
            penetration[index] = player.Value.Stats.Penetration;           
            index++;
        }

        return new GameSyncMessage(hostSession, numOfClient, maxHp, stamina, staminaM, damage, penetration, online);
    }
}
