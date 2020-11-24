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
        AttackEnd,
        Damaged,

        LoadRoomScene,      // 룸 씬으로 전환
        LoadGameScene,      // 인게임 씬으로 전환
        StartCount,         // 시작 카운트
        GameStart,          // 게임 시작
        GameEnd,            // 게임 종료
        GameSync,           // 플레이어 재접속 시 게임 현재 상황 싱크
        GameMySync,           // 플레이어 재접속 시 게임 현재 상황 싱크
        Calculation,        // 여러 계산
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

    public class PlayerAttackEndMessage : Message
    {
        public SessionId playerSession;
        public PlayerAttackEndMessage(SessionId session) : base(Type.AttackEnd)
        {
            this.playerSession = session;
        }
    }

    public class PlayerDamagedMessage : Message
    {
        public SessionId playerSession;
        public double damage;
        public PlayerDamagedMessage(SessionId session, double damage) : base(Type.Damaged)
        {
            this.playerSession = session;
            this.damage = damage;
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

    public class GameMySyncMessage : Message
    {
        public SessionId session;
        public double MaxHp = 0f;
        public double Stamina = 0f;
        public double StaminaM = 0f;
        public double Damage = 0f;
        public double Penetration = 0f;

        public GameMySyncMessage(SessionId session, double maxHp, double stamina, double staminaM, double damage, double penetration) : base(Type.GameMySync)
        {
            this.session = session;
            this.MaxHp = maxHp;
            this.Stamina = stamina;
            this.StaminaM = staminaM;
            this.Damage = damage;
            this.Penetration = penetration;
        }
    }

    public class GameSyncMessage : Message
    {
        public SessionId host;
        public int count = 0;
        public double[] MaxHp = null;
        public double[] Stamina = null;
        public double[] StaminaM = null;
        public double[] Damage = null;
        public double[] Penetration = null;
        public bool[] onlineInfo = null;

        public GameSyncMessage(SessionId host, int count, double[] maxHp, double[] stamina, double[] staminaM, double[] damage, double[] penetration, bool[] online) : base(Type.GameSync)
        {
            this.host = host;
            this.count = count;
            this.MaxHp = this.Stamina = this.StaminaM = this.Damage = this.Penetration = new double[count];
            this.onlineInfo = new bool[count];

            for (int i = 0; i < count; ++i)
            {
                this.MaxHp[i]      = maxHp[i];
                this.Stamina[i]      = stamina[i];
                this.StaminaM[i]      = staminaM[i];
                this.Damage[i]      = damage[i];
                this.Penetration[i]      = penetration[i];
                onlineInfo[i]      = online[i];
            }
        }
    }

    public class CalculationMessage : Message
    {
        public SessionId host;
        public CalculationMessage(SessionId host) : base(Type.Calculation)
        {
            this.host = host;
        }
    }

    public static class KeyEventCode
    {
        public const int NONE          = 0;
        public const int IDLE          = 1;   // 아무것도 안하는 메시지
        public const int WEAK_ATTACK   = 2;   // 약한 공격 메시지
        public const int STRONG_ATTACK = 3;   // 강한 공격 메시지
        public const int DEFENSE       = 4;   // 방어 메시지
        public const int STUN          = 5;   // 스턴 메시지
        public const int ATTACK_END    = 6;   // 공격 종료 메세지
        public const int DAMAGED       = 7;   // 공격 받는 메세지
    }

    public class KeyMessage : Message
    {
        public int keyData;
        public Direction direction;
        public float x;
        public float y;
        public float z;

        public KeyMessage(int data, Vector3 pos, Direction direction = Direction.NONE) : base(Type.Key)
        {
            this.keyData = data;
            this.x = pos.x;
            this.y = pos.y;
            this.z = pos.z;
            this.direction = direction;
        }
    }
}