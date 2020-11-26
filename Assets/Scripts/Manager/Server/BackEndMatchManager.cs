using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using BackEnd.Tcp;
using Protocol;
using Manager.Dispatcher;

public class ServerInfo
{
    public string host;
    public ushort port;
    public string roomToken;
}

public class MatchRecord
{
    public MatchType matchType;
    public MatchModeType modeType;
    public string matchTitle;
    public string score = "-";
    public int win = -1;
    public int numOfMatch = 0;
    public double winRate = 0;
}

public partial class BackEndMatchManager : MonoBehaviour
{
    public static BackEndMatchManager instance = null; 

    // 콘솔에서 생성한 매칭 카드 정보
    public class MatchInfo
    {
        public string title;                // 매칭 명
        public string inDate;               // 매칭 inDate (UUID)
        public MatchType matchType;         // 매치 타입
        public MatchModeType matchModeType; // 매치 모드 타입
        public string headCount;            // 매칭 인원
    }
    public List<MatchInfo> matchInfos { get; private set; } = new List<MatchInfo>(); // 콘솔에서 생성한 매칭 카드들의 리스트

    public Dictionary<SessionId, MatchUserGameRecord> gameRecords { get; private set; } = null; // 매치에 참가중인 유저들의 매칭 기록
    public List<SessionId> sessionIdList { get; private set; }  // 매치에 참가중인 유저들의 세션 목록
    public SessionId hostSession { get; private set; }  // 호스트 세션

    private string inGameRoomToken = string.Empty;  // 게임 룸 토큰 (인게임 접속 토큰)
    private ServerInfo roomInfo = null;             // 게임 룸 정보

    public  bool isReconnectEnable { get; private set; } = false;
    public  bool isConnectMatchServer { get; private set; } = false;
    private bool isConnectInGameServer = false;
    private bool isJoinGameRoom = false;
    public  bool isReconnectProcess { get; private set; } = false;

    private int numOfClient = 2;                    // 매치에 참가한 유저의 총 수

    #region Host
    public bool isHost { get; private set; } = false;   // 호스트 여부 (서버에서 설정한 SuperGamer 정보를 가져옴)
    private Queue<KeyMessage> localQueue = null;    // 호스트에서 로컬로 처리하는 패킷을 쌓아두는 큐 (로컬처리하는 데이터는 서버로 발송 안함)
    #endregion

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
    }

    void OnApplicationQuit()
    {
        if (isConnectMatchServer)
        {
            LeaveMatchServer();
            Debug.Log("ApplicationQuit - LeaveMatchServer");
        }
    }

    public void HandlerSetting()
    {
        MatchMakingHandler();
        GameHandler();
        ExceptionHandler();
    }

    public bool IsMySessionId(SessionId session)
    {
        return Backend.Match.GetMySessionId() == session;
    }

    public string GetNickNameBySessionId(SessionId session)
    {
        return gameRecords[session].m_nickname;
    }

    public int GetMMRBySessionId(SessionId session)
    {
        return gameRecords[session].m_mmr;
    }

    public bool IsSessionListNull()
    {
        return sessionIdList == null || sessionIdList.Count == 0;
    }

    private bool SetHostSession()
    {
        // 호스트 세션 정하기
        // 각 클라이언트가 모두 수행 (호스트 세션 정하는 로직은 모두 같으므로 각각의 클라이언트가 모두 로직을 수행하지만 결과값은 같다.)

        Debug.Log("호스트 세션 설정 진입");
        // 호스트 세션 정렬 (각 클라이언트마다 입장 순서가 다를 수 있기 때문에 정렬)
        sessionIdList.Sort();
        isHost = false;
        // 내가 호스트 세션인지
        foreach (var record in gameRecords)
        {
            if (record.Value.m_isSuperGamer == true)
            {
                if (record.Value.m_sessionId.Equals(Backend.Match.GetMySessionId()))
                {
                    isHost = true;
                }
                hostSession = record.Value.m_sessionId;
                break;
            }
        }

        Debug.Log("호스트 여부 : " + isHost);

        // 호스트 세션이면 로컬에서 처리하는 패킷이 있으므로 로컬 큐를 생성해준다
        if (isHost)
        {
            localQueue = new Queue<KeyMessage>();
        }
        else
        {
            localQueue = null;
        }

        // 호스트 설정까지 끝나면 매치서버와 접속 끊음
        LeaveMatchServer();
        return true;
    }

    private void SetSubHost(SessionId hostSessionId)
    {
        Debug.Log("서브 호스트 세션 설정 진입");
        // 누가 서브 호스트 세션인지 서버에서 보낸 정보값 확인
        // 서버에서 보낸 SuperGamer 정보로 GameRecords의 SuperGamer 정보 갱신
        foreach (var record in gameRecords)
        {
            if (record.Value.m_sessionId.Equals(hostSessionId))
            {
                record.Value.m_isSuperGamer = true;
            }
            else
            {
                record.Value.m_isSuperGamer = false;
            }
        }
        // 내가 호스트 세션인지 확인
        if (hostSessionId.Equals(Backend.Match.GetMySessionId()))
        {
            isHost = true;
        }
        else
        {
            isHost = false;
        }

        hostSession = hostSessionId;

        Debug.Log("서브 호스트 여부 : " + isHost);
        // 호스트 세션이면 로컬에서 처리하는 패킷이 있으므로 로컬 큐를 생성해준다
        if (isHost)
        {
            localQueue = new Queue<KeyMessage>();
        }
        else
        {
            localQueue = null;
        }

        Debug.Log("서브 호스트 설정 완료");
    }

    // 매칭 서버 관련 이벤트 핸들러
    public void MatchMakingHandler()
    {
        Backend.Match.OnJoinMatchMakingServer = (args) =>
        {
            Debug.Log("OnJoinMatchMakingServer : " + args.ErrInfo);
            // 매칭 서버에 접속하면 호출
            ProcessAccessMatchMakingServer(args.ErrInfo);
        };
        Backend.Match.OnMatchMakingResponse = (args) =>
        {
            Debug.Log("OnMatchMakingResponse : " + args.ErrInfo + " : " + args.Reason);
            // 매칭 신청 관련 작업에 대한 호출
            ProcessMatchMakingResponse(args);
        };

        Backend.Match.OnLeaveMatchMakingServer = (args) =>
        {
            // 매칭 서버에서 접속 종료할 때 호출
            Debug.Log("OnLeaveMatchMakingServer : " + args.ErrInfo);
            isConnectMatchServer = false;

            if (args.ErrInfo.Category.Equals(ErrorCode.DisconnectFromRemote) || args.ErrInfo.Category.Equals(ErrorCode.Exception)
                || args.ErrInfo.Category.Equals(ErrorCode.NetworkTimeout))
            {
                // 서버에서 강제로 끊은 경우
                if (MainUI.instance)
                {
                    // ...
                }
            }
        };

        // 대기 방 생성/실패 여부
        Backend.Match.OnMatchMakingRoomCreate = (args) =>
        {
            Debug.Log("OnMatchMakingRoomCreate : " + args.ErrInfo + " : " + args.Reason);

            MainUI.instance.CreateRoomResult(args.ErrInfo.Equals(ErrorCode.Success) == true);
        };

        // 대기방에 유저 입장 메시지
        Backend.Match.OnMatchMakingRoomJoin = (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomJoin : {0} : {1}", args.ErrInfo, args.Reason));
            if (args.ErrInfo.Equals(ErrorCode.Success))
            {
                Debug.Log("user join in loom : " + args.UserInfo.m_nickName);
                MainUI.instance.AddReadyPlayer(args.UserInfo.m_nickName);
            }
        };

        // 대기방에 현재 입장해 있는 유저 리스트 메시지
        Backend.Match.OnMatchMakingRoomUserList = (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomUserList : {0} : {1}", args.ErrInfo, args.Reason));
            List<MatchMakingUserInfo> userList = null;
            if (args.ErrInfo.Equals(ErrorCode.Success))
            {
                userList = args.UserInfos;
                Debug.Log("ready room user count : " + userList.Count);
            }
            MainUI.instance.CreateRoomResult(args.ErrInfo.Equals(ErrorCode.Success) == true, userList);
        };

        // 대기방에 유저 퇴장 메시지
        Backend.Match.OnMatchMakingRoomLeave = (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomLeave : {0} : {1}", args.ErrInfo, args.Reason));
            if (args.ErrInfo.Equals(ErrorCode.Success) || args.ErrInfo.Equals(ErrorCode.Match_Making_KickedByOwner))
            {
                Debug.Log("user leave in loom : " + args.UserInfo.m_nickName);
                if (args.UserInfo.m_nickName.Equals(BackEndServerManager.instance.myNickName))
                {
                    MainUI.instance.CloseMatchUI();
                    MainUI.instance.SetJaehwaUI(true);
                    MainUI.instance.RemoveReadyPlayer(args.UserInfo.m_nickName);
                    return;
                }
            }
        };

        // 방장이 대기방에서 나가 대기방 파기 된 메시지
        Backend.Match.OnMatchMakingRoomDestory = (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomDestory : {0} : {1}", args.ErrInfo, args.Reason));
            MainUI.instance.CloseMatchUI();
            MainUI.instance.ResetReadyPlayer();
            MainUI.instance.SetErrorLog("방장이 대기방을 파기하였습니다.");
        };
    }

    #region Ingame

    // 인게임 서버 관련 이벤트 핸들러
    private void GameHandler()
    {
        Backend.Match.OnSessionJoinInServer = (args) =>
        {
            Debug.Log("OnSessionJoinInServer : " + args.ErrInfo);
            // 인게임 서버에 접속하면 호출
            if (args.ErrInfo != ErrorInfo.Success)
            {
                if (isReconnectProcess)
                {
                    // 패배했다는 결과만 알려줌
                    // ...
                    GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
                    MainUI.instance.SetReconnectUI(true);
                    isConnectInGameServer = false;
                }
                return;
            }
            if (isJoinGameRoom)
            {
                return;
            }
            if (inGameRoomToken == string.Empty)
            {
                Debug.LogError("인게임 서버 접속 성공했으나 룸 토큰이 없습니다.");
                return;
            }
            Debug.Log("인게임 서버 접속 성공");
            
            // 유저 캐릭터 데이터 세이브
            //PlayerStats.instance.CharacterSave();
            
            isJoinGameRoom = true;
            AccessInGameRoom(inGameRoomToken);
        };

        Backend.Match.OnSessionListInServer = (args) =>
        {
            // 세션 리스트 호출 후 조인 채널이 호출됨
            // 현재 같은 게임(방)에 참가중인 플레이어들 중 나보다 먼저 이 방에 들어와 있는 플레이어들과 나의 정보가 들어있다.
            // 나보다 늦게 들어온 플레이어들의 정보는 OnMatchInGameAccess 에서 수신됨
            Debug.Log("OnSessionListInServer : " + args.ErrInfo);

            ProcessMatchInGameSessionList(args);
        };

        Backend.Match.OnMatchInGameAccess = (args) =>
        {
            Debug.Log("OnMatchInGameAccess : " + args.ErrInfo);
            // 세션이 인게임 룸에 접속할 때마다 호출 (각 클라이언트가 인게임 룸에 접속할 때마다 호출됨)
            ProcessMatchInGameAccess(args);
        };
        
        Backend.Match.OnMatchInGameStart = () =>
        {
            // 서버에서 게임 시작 패킷을 보내면 호출
            GameSetup();
        };

        Backend.Match.OnMatchResult = (args) =>
        {
            Debug.Log("게임 결과값 업로드 결과 : " + string.Format("{0} : {1}", args.ErrInfo, args.Reason));
            // 서버에서 게임 결과 패킷을 보내면 호출
            // 내가(클라이언트가) 서버로 보낸 결과값이 정상적으로 업데이트 되었는지 확인

            if (args.ErrInfo == ErrorCode.Success)
            {
                LeaveInGameRoom();
                GameUI.instance.ShowResultBoard(matchGameResult);
                GameManager.instance.ChangeState(GameManager.GameState.Result);
            }
            else if (args.ErrInfo == ErrorCode.Match_InGame_Timeout)
            {
                Debug.Log("게임 입장 실패 : " + args.ErrInfo);
                MainUI.instance.MatchCancelCallback();
            }
            else
            {
                Debug.Log("게임 결과 업로드 실패 : " + args.ErrInfo);
            }
            // 세션리스트 초기화
            sessionIdList = null;
        };

        Backend.Match.OnMatchRelay = (args) =>
        {
            // 각 클라이언트들이 서버를 통해 주고받은 패킷들
            // 서버는 단순 브로드캐스팅만 지원 (서버에서 어떠한 연산도 수행하지 않음)

            // 게임 사전 설정
            if (PrevGameMessage(args.BinaryUserData) == true)
            {
                // 게임 사전 설정을 진행하였으면 바로 리턴
                return;
            }

            if (WorldPackage.instance == null)
            {
                // 월드 매니저가 존재하지 않으면 바로 리턴
                return;
            }
            WorldPackage.instance.OnRecieve(args);
        };

        Backend.Match.OnLeaveInGameServer = (args) =>
        {
            Debug.Log("OnLeaveInGameServer : " + args.ErrInfo + " : " + args.Reason);
            if (args.Reason.Equals("Fail To Reconnect"))
            {
                JoinMatchServer();
            }
            isConnectInGameServer = false;
        };

        Backend.Match.OnSessionOnline = (args) =>
        {
            // 다른 유저가 재접속 했을 때 호출
            var nickName = Backend.Match.GetNickNameBySessionId(args.GameRecord.m_sessionId);
            Debug.Log(string.Format("[{0}] 온라인되었습니다. - {1} : {2}", nickName, args.ErrInfo, args.Reason));
            ProcessSessionOnline(args.GameRecord.m_sessionId, nickName);
        };

        Backend.Match.OnSessionOffline = (args) =>
        {
            // 다른 유저 혹은 자기자신이 접속이 끊어졌을 때 호출
            Debug.Log(string.Format("[{0}] 오프라인되었습니다. - {1} : {2}", args.GameRecord.m_nickname, args.ErrInfo, args.Reason));
            // 인증 오류가 아니면 오프라인 프로세스 실행
            if (args.ErrInfo != ErrorCode.AuthenticationFailed)
            {
                ProcessSessionOffline(args.GameRecord.m_sessionId);
            }
            else
            {
                // 잘못된 재접속 시도 시 인증오류가 발생
            }
        };

        Backend.Match.OnChangeSuperGamer = (args) =>
        {
            Debug.Log(string.Format("이전 방장 : {0} / 새 방장 : {1}", args.OldSuperUserRecord.m_nickname, args.NewSuperUserRecord.m_nickname));
            // 호스트 재설정
            SetSubHost(args.NewSuperUserRecord.m_sessionId);
            if (isHost)
            {
                // 만약 서브호스트로 설정되면 다른 모든 클라이언트에 싱크메시지 전송
                //Invoke("SendGameSyncMessage", 1.0f);
            }
        };
    }

    private void ExceptionHandler()
    {
        // 예외가 발생했을 때 호출
        Backend.Match.OnException = (e) =>
        {
            Debug.Log(e);
        };
    }

    void Update()
    {
        if (isConnectInGameServer || isConnectMatchServer)
        {
            Debug.Log("매치 비동기 함수 실행중");
            Backend.Match.Poll();

            // 호스트의 경우 로컬 큐가 존재
            // 큐에 있는 패킷을 로컬에서 처리
            if (localQueue != null)
            {
                while (localQueue.Count > 0)
                {
                    var msg = localQueue.Dequeue();
                    WorldPackage.instance.OnRecieveForLocal(msg);
                }
            }
        }
    }

    public void GetMyMatchRecord(int index, Action<MatchRecord, bool> func)
    {
        var inDate = BackEndServerManager.instance.myIndate;

        SendQueue.Enqueue(Backend.Match.GetMatchRecord, inDate, matchInfos[index].matchType, matchInfos[index].matchModeType, matchInfos[index].inDate, callback =>
        {
            MatchRecord record = new MatchRecord();
            record.matchTitle = matchInfos[index].title;
            record.matchType = matchInfos[index].matchType;
            record.modeType = matchInfos[index].matchModeType;

            if (!callback.IsSuccess())
            {
                Debug.LogError("매칭 기록 조회 실패\n" + callback);
                func(record, false);
                return;
            }

            if (callback.Rows().Count <= 0)
            {
                Debug.Log("매칭 기록이 존재하지 않습니다.\n" + callback);
                func(record, true);
                return;
            }
            var data = callback.Rows()[0];
            var win = Convert.ToInt32(data["victory"]["N"].ToString());
            var draw = Convert.ToInt32(data["draw"]["N"].ToString());
            var defeat = Convert.ToInt32(data["defeat"]["N"].ToString());
            var numOfMatch = win + draw + defeat;
            string point = string.Empty;
            if (matchInfos[index].matchType == MatchType.MMR)
            {
                point = data["mmr"]["N"].ToString();
            }
            else if (matchInfos[index].matchType == MatchType.Point)
            {
                point = data["point"]["N"].ToString() + " P";
            }
            else
            {
                point = "-";
            }

            record.win = win;
            record.numOfMatch = numOfMatch;
            record.winRate = Math.Round(((float)win / numOfMatch) * 100 * 100) / 100;
            record.score = point;

            func(record, true);
        });
    }

    public void IsMatchGameActivate()
    {
        roomInfo = null;
        isReconnectEnable = false;

        JoinMatchServer();
    }

    public void GetMatchList(Action<bool, string> func)
    {
        // 매칭 카드 정보 초기화
        matchInfos.Clear();

        Backend.Match.GetMatchList(callback =>
        {
            // 요청 실패하는 경우 재요청
            if (callback.IsSuccess() == false)
            {
                Debug.Log("매칭카드 리스트 불러오기 실패\n" + callback);
                Dispatcher.Current.BeginInvoke(() =>
                {
                    GetMatchList(func);
                });
                return;
            }

            foreach (LitJson.JsonData row in callback.Rows())
            {
                MatchInfo matchInfo = new MatchInfo();
                matchInfo.title = row["matchTitle"]["S"].ToString();
                matchInfo.inDate = row["inDate"]["S"].ToString();
                matchInfo.headCount = row["matchHeadCount"]["N"].ToString();

                foreach (MatchType type in Enum.GetValues(typeof(MatchType)))
                {
                    if (type.ToString().ToLower().Equals(row["matchType"]["S"].ToString().ToLower()))
                    {
                        matchInfo.matchType = type;
                    }
                }

                foreach (MatchModeType type in Enum.GetValues(typeof(MatchModeType)))
                {
                    if (type.ToString().ToLower().Equals(row["matchModeType"]["S"].ToString().ToLower()))
                    {
                        matchInfo.matchModeType = type;
                    }
                }

                matchInfos.Add(matchInfo);
            }
            Debug.Log("매칭카드 리스트 불러오기 성공 : " + matchInfos.Count);
            func(true, string.Empty);
        });
    }

    public MatchInfo GetMatchInfo(string indate)
    {
        var result = matchInfos.FirstOrDefault(x => x.inDate == indate);
        if (result.Equals(default(MatchInfo)) == true)
        {
            return null;
        }
        return result;
    }

    public void AddMsgToLocalQueue(KeyMessage msg)
    {
        // 로컬 큐에 메시지 추가
        if (isHost == false || localQueue == null)
        {
            return;
        }

        localQueue.Enqueue(msg);
    }

    public void SetHostSession(SessionId host)
    {
        hostSession = host;
    }
    #endregion
}