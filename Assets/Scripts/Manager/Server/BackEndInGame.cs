using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using BackEnd.Tcp;

public partial class BackEndMatchManager : MonoBehaviour
{
    private bool isSetHost = false;                 // 호스트 세션 결정했는지 여부

    private MatchGameResult matchGameResult;

    // 게임 로그
    private string FAIL_ACCESS_INGAME = "인게임 접속 실패 : {0} - {1}";
    private string SUCCESS_ACCESS_INGAME = "유저 인게임 접속 성공 : {0}";
    private string NUM_INGAME_SESSION = "인게임 내 세션 갯수 : {0}";

    // 게임 레디 상태일 때 호출됨
    public void OnGameReady()
    {
        if (isSetHost == false)
        {
            // 호스트가 설정되지 않은 상태이면 호스트 설정
            isSetHost = SetHostSession();
        }
        Debug.Log("호스트 설정 완료");

        if (isHost == true)
        {
            // 0.5초 후 ReadyToLoadRoom 함수 호출
            Debug.Log("룸씬으로 교체");
            Invoke("SendChangeRoomScene", 0.5f);
        }
    }

    // 현재 룸에 접속한 세션들의 정보
    // 최초 룸에 접속했을 때 1회 수신됨
    // 재접속 했을 때도 1회 수신됨
    private void ProcessMatchInGameSessionList(MatchInGameSessionListEventArgs args)
    {
        sessionIdList = new List<SessionId>();
        gameRecords = new Dictionary<SessionId, MatchUserGameRecord>();

        foreach (var record in args.GameRecords)
        {
            sessionIdList.Add(record.m_sessionId);
            gameRecords.Add(record.m_sessionId, record);
        }
        sessionIdList.Sort();
    }

    // 클라이언트 들의 게임 룸 접속에 대한 리턴값
    // 클라이언트가 게임 룸에 접속할 때마다 호출됨
    // 재접속 했을 때는 수신되지 않음
    private void ProcessMatchInGameAccess(MatchInGameSessionEventArgs args)
    {
        if (isReconnectProcess)
        {
            // 재접속 프로세스 인 경우
            // 이 메시지는 수신되지 않고, 만약 수신되어도 무시함
            Debug.Log("재접속 프로세스 진행중... 재접속 프로세스에서는 ProcessMatchInGameAccess 메시지는 수신되지 않습니다.\n" + args.ErrInfo);
            return;
        }

        Debug.Log(string.Format(SUCCESS_ACCESS_INGAME, args.ErrInfo));

        if (args.ErrInfo != ErrorCode.Success)
        {
            // 게임 룸 접속 실패
            var errorLog = string.Format(FAIL_ACCESS_INGAME, args.ErrInfo, args.Reason);
            Debug.Log(errorLog);
            LeaveInGameRoom();
            return;
        }

        // 게임 룸 접속 성공
        // 인자값에 방금 접속한 클라이언트(세션)의 세션ID와 매칭 기록이 들어있다.
        // 세션 정보는 누적되어 들어있기 때문에 이미 저장한 세션이면 건너뛴다.

        var record = args.GameRecord;
        Debug.Log(string.Format(string.Format("인게임 접속 유저 정보 [{0}] : {1}", args.GameRecord.m_sessionId, args.GameRecord.m_nickname)));
        if (!sessionIdList.Contains(args.GameRecord.m_sessionId))
        {
            // 세션 정보, 게임 기록 등을 저장
            sessionIdList.Add(record.m_sessionId);
            gameRecords.Add(record.m_sessionId, record);

            Debug.Log(string.Format(NUM_INGAME_SESSION, sessionIdList.Count));
        }
    }

    // 인게임 룸 접속
    private void AccessInGameRoom(string roomToken)
    {
        Backend.Match.JoinGameRoom(roomToken);
    }

    // 인게임 서버 접속 종료
    public void LeaveInGameRoom()
    {
        isConnectInGameServer = false;
        Backend.Match.LeaveGameServer();
    }

    // 서버에서 게임 시작 패킷을 보냈을 때 호출
    // 모든 세션이 게임 룸에 참여 후 "콘솔에서 설정한 시간" 후에 게임 시작 패킷이 서버에서 온다
    private void GameSetup()
    {
        Debug.Log("게임 시작 메시지 수신. 게임 설정 시작");
        // 게임 시작 메시지가 오면 게임을 레디 상태로 변경
        if (GameManager.instance.gameState != GameManager.GameState.Ready)
        {
            isHost = false;
            isSetHost = false;
            OnGameReady();
        }
    }

    private void SendChangeRoomScene()
    {
        Debug.Log("게임준비 씬 전환 메시지 송신");
        SendDataToInGame(new Protocol.LoadRoomSceneMessage());
    }

    private void SendChangeGameScene()
    {
        Debug.Log("게임 씬 전환 메시지 송신");
        SendDataToInGame(new Protocol.LoadGameSceneMessage());
    }

    // 서버로 게임 결과 전송
    // 서버에서 각 클라이언트가 보낸 결과를 종합
    public void MatchGameOver(Stack<SessionId> record)
    {
        Debug.Log("Match Over");
        if (nowModeType == MatchModeType.OneOnOne)
        {
            matchGameResult = OneOnOneRecord(record);
        }
        else
        {
            Debug.LogError("게임 결과 종합 실패 - 알수없는 매치모드타입입니다.\n" + nowModeType);
            return;
        }

        // 승/패 나누는 메세지 띄움
        Backend.Match.MatchEnd(matchGameResult);
    }

    // 1:1 게임 결과
    private MatchGameResult OneOnOneRecord(Stack<SessionId> record)
    {
        Debug.Log("1대1 결과 도출");
        MatchGameResult nowGameResult = new MatchGameResult();

        nowGameResult.m_winners = new List<SessionId>();
        nowGameResult.m_winners.Add(record.Pop());

        nowGameResult.m_losers = new List<SessionId>();
        nowGameResult.m_losers.Add(record.Pop());

        nowGameResult.m_draws = null;

        return nowGameResult;
    }

    // 호스트에서 보낸 세션리스트로 갱신
    public void SetPlayerSessionList(List<SessionId> sessions)
    {
        sessionIdList = sessions;
    }

    // 서버로 데이터 패킷 전송
    // 서버에서는 이 패킷을 받아 모든 클라이언트(패킷 보낸 클라이언트 포함)로 브로드캐스팅 해준다.
    public void SendDataToInGame<T>(T msg)
    {
        var byteArray = DataParser.DataToJsonData<T>(msg);
        Backend.Match.SendDataToInGameRoom(byteArray);
    }

    private void ProcessSessionOffline(SessionId sessionId)
    {
        //if (hostSession.Equals(sessionId))
        //{
        //    // 호스트 연결 대기를 띄움
        //    //InGameUiManager.GetInstance().SetHostWaitBoard();
        //}
        //else
        //{
        //    // 호스트가 아니면 단순히 UI 만 띄운다.
        //}

        if (isHost)
        {
            // 현재 매치모드가 1대1 일때 플레이거가 오프라인을 하면 게임을 종료시킴
            if (nowModeType == MatchModeType.OneOnOne)
            {
                WorldPackage.instance.playerDie(sessionId);
            }
        }
    }

    private void ProcessSessionOnline(SessionId sessionId, string nickName)
    {
        // 호스트가 아니면 아무 작업 안함 (호스트가 해줌)
        if (isHost)
        {
            // 재접속 한 클라이언트가 인게임 씬에 접속하기 전 게임 정보값을 전송 시 nullptr 예외가 발생하므로 조금
            // 2초정도 기다린 후 게임 정보 메시지를 보냄
            Invoke("SendGameSyncMessage", 2.0f);
        }
    }

    // Invoke로 실행됨
    private void SendGameSyncMessage()
    {
        // 현재 게임 상황 (위치, hp 등등...)
        var message = WorldPackage.instance.GetNowGameState(hostSession);
        SendDataToInGame(message);
    }

    public bool PrevGameMessage(byte[] BinaryUserData)
    {
        Protocol.Message msg = DataParser.ReadJsonData<Protocol.Message>(BinaryUserData);
        if (msg == null)
        {
            return false;
        }

        // 게임 설정 사전 작업 패킷 검사 
        switch (msg.type)
        {
            case Protocol.Type.LoadRoomScene:
                if (isHost)
                {
                    Debug.Log("2.5초 후 게임 씬 전환 메시지 송신");
                    Invoke("SendChangeGameScene", 2.5f);
                }
                return true;
            case Protocol.Type.LoadGameScene:
                GameManager.instance.ChangeState(GameManager.GameState.GameStart);
                return true;
        }
        return false;
    }

}
