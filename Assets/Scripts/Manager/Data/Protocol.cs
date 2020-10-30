using BackEnd.Tcp;
using UnityEngine;
using System.Collections.Generic;

namespace Protocol
{
    // 이벤트 타입
    public enum Type : sbyte
    {
        Key = 0,

        Idle,
        WeakAttack,
        StrongAttack,
        Defense,
        Stun,

        LoadRoomScene,      // 룸 씬으로 전환
        LoadGameScene,      // 인게임 씬으로 전환
        StartCount,     // 시작 카운트
        GameStart,      // 게임 시작
        GameEnd,        // 게임 종료
        GameSync,       // 플레이어 재접속 시 게임 현재 상황 싱크
        Max,

        NONE = 99
    }

    public enum Direction : sbyte
    {
        Left,
        Right,
        NONE = 99
    }

    // 애니메이션 싱크는 사용하지 않습니다.
    /*
    public enum AnimIndex
    {
        idle = 0,
        walk,
        walkBack,
        stop,
        max
    }
    */

    public class Message
    {
        public Type type;

        public Message(Type type)
        {
            this.type = type;
        }
    }

    #region Event

    public class PlayerIdleMessage : Message 
    {
        public SessionId playerSession;
        public PlayerIdleMessage(SessionId session) : base(Type.Idle) => this.playerSession = session;
    }

    public class PlayerWeakAttackMessage : Message
    {
        public SessionId playerSession;
        public Direction playerDirection;
        public PlayerWeakAttackMessage(SessionId session, Direction direction) : base(Type.WeakAttack)
        {
            this.playerSession = session;
            this.playerDirection = direction;
        }
    }

    public class PlayerStrongAttackMessage : Message
    {
        public SessionId playerSession;
        public Direction playerDirection;
        public PlayerStrongAttackMessage(SessionId session, Direction direction) : base(Type.StrongAttack)
        {
            this.playerSession = session;
            this.playerDirection = direction;
        }
    }

    public class PlayerDefenseMessage : Message
    {
        public SessionId playerSession;
        public Direction playerDirection;
        public PlayerDefenseMessage(SessionId session, Direction direction) : base(Type.Defense)
        {
            this.playerSession = session;
            this.playerDirection = direction;
        }
    }

    public class PlayerStunMessage : Message
    {
        public SessionId playerSession;
        public PlayerStunMessage(SessionId session) : base(Type.Stun)
        {
            this.playerSession = session;
        }
    }

    #endregion

    #region Scene

    public class LoadRoomSceneMessage : Message
    {
        public LoadRoomSceneMessage() : base(Type.LoadRoomScene)
        {

        }
    }

    public class LoadGameSceneMessage : Message
    {
        public LoadGameSceneMessage() : base(Type.LoadGameScene)
        {

        }
    }

    public class StartCountMessage : Message
    {
        public int time;
        public StartCountMessage(int time) : base(Type.StartCount)
        {
            this.time = time;
        }
    }

    public class GameStartMessage : Message
    {
        public GameStartMessage() : base(Type.GameStart) { }
    }

    public class GameEndMessage : Message
    {
        public int count;
        public int[] sessionList;
        public GameEndMessage(Stack<SessionId> result) : base(Type.GameEnd)
        {
            count = result.Count;
            sessionList = new int[count];
            for (int i = 0; i < count; ++i)
            {
                sessionList[i] = (int)result.Pop();
            }
        }
    }

    #endregion

    public class GameSyncMessage : Message
    {
        public SessionId host;
        public int count = 0;
        public int[] hpValue = null;
        public bool[] onlineInfo = null;

        public GameSyncMessage(SessionId host, int count, int[] hp, bool[] online) : base(Type.GameSync)
        {
            this.host = host;
            this.count = count;
            this.hpValue = new int[count];
            this.onlineInfo = new bool[count];

            for (int i = 0; i < count; ++i)
            {
                hpValue[i] = hp[i];
                onlineInfo[i] = online[i];
            }
        }
    }

    public class KeyMessage : Message
    {
        public int keyData;
        public float x;
        public float y;
        public float z;

        public KeyMessage(int data, Vector3 pos) : base(Type.Key)
        {
            this.keyData = data;
            this.x = pos.x;
            this.y = pos.y;
            this.z = pos.z;
        }
    }
}