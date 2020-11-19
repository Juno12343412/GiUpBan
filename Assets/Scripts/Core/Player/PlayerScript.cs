using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager.Pooling;
using Protocol;
using BackEnd.Tcp;
using BackEnd;

public enum PlayerCurState
{
    NONE = 0,
    IDLE = 1,   // 아무것도 안하는 메시지
    WEAK_ATTACK = 2,   // 약한 공격 메시지
    STRONG_ATTACK = 3,   // 강한 공격 메시지
    DEFENSE = 4,   // 방어 메시지
    STUN = 5,  // 스턴 메시지
    ATTACK_END = 6  // 공격 종료 메세지
} // 플레이어 행동 상태

public enum PlayerDirection : sbyte
{
    Left,
    Right,
    NONE = 99
} // 플레이어 행동 방향

public class PlayerScript : PoolingObject
{
    #region 플레이어 관련 변수들
    [SerializeField]
    private PlayerStats.Player stats = new PlayerStats.Player();
    public PlayerStats.Player Stats { get { return stats; } }

    public PlayerCurState State = new PlayerCurState();
    public Direction Direction;
    #endregion

    #region 화면 조작 관련 변수들
    private Vector2 touchBeganPos;
    private Vector2 touchEndedPos;
    private Vector2 touchDif;
    private Vector2 firstPressPos;
    private Vector2 secondPressPos;
    private Vector2 currentSwipe;
    private bool isLong = false;
    private bool isSwipe = false;
    [Tooltip("스와이프 민감도")] public float swipeSensitivity = 0.3f;
    #endregion

    #region 기타 변수들
    private bool Cancel = false;
    private bool AttackPoint = false;
    private bool isStun = false;
    private bool isDelay = false;
    public Animator Anim;
    #endregion

    // New Var
    public SessionId index { get; private set; } = 0;
    public string nickName = string.Empty;
    public bool isMe { get; private set; } = false;
    public bool isLive { get; private set; } = false;
    // New Var

    public override string objectName => "Player";

    public override void Init()
    {
        base.Init();
    }

    public override void Release()
    {
        base.Release();
    }

    void Update()
    {
        if (stats.CurHp <= 0)
            WorldPackage.instance.playerDie(index);

        if (isMe)
        {
            if (!isStun)
            {
                if (!isDelay)
                    PlayerControl();
            }
            else { State = PlayerCurState.STUN; }

            transform.position = WorldPackage.instance.startingPoint[0].position;
            transform.rotation = WorldPackage.instance.startingPoint[0].rotation;
        }
        else
        {
            transform.position = WorldPackage.instance.startingPoint[1].position;
            transform.rotation = WorldPackage.instance.startingPoint[1].rotation;
        }

        if (!BackEndMatchManager.instance.isHost)
        {
            return;
        }
    }

    public void PlayerSetting(bool isMe, SessionId index, string nickName)
    {
        this.isMe = isMe;
        this.index = index;
        this.nickName = nickName;

        Anim = GetComponent<Animator>();
        if (this.isMe)
        {
            // 자기 자신만 해야할 설정 (카메라 등)
            // ...
            stats = BackEndServerManager.instance.myInfo;
            State = PlayerCurState.IDLE;

            Direction = Direction.NONE;
            StartCoroutine(CR_StaminaHeal());

        }

        isLive = true;
    }

    public IEnumerator CR_Stun(float Time)
    {
        if (!isStun)
        {
            isStun = true;
            yield return new WaitForSeconds(Time);
            isStun = false;
        }
    }// 스턴 코루틴
    public IEnumerator CR_StaminaHeal()
    {
        while(GameManager.instance.gameState != GameManager.GameState.InGame || GameManager.instance.gameState != GameManager.GameState.GameStart)
        {
            if (!Anim.GetBool("isAttack"))
            {
                yield return new WaitForSeconds(0.1f);
                if (stats.Stamina < 100)
                    stats.Stamina += 1;
            }
            yield return null;
        }
    } // 스테미너 회복

    IEnumerator Ielong = null; // 긴 터치 코루틴 담는 변수 코루틴스탑시 오류 방지
    public void PlayerControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isLong = false;
            isSwipe = false;
            firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Ielong = CR_LongTouch();
            StartCoroutine(Ielong);
            
            if (firstPressPos.x < Screen.width * 0.5)
            {
                Direction = Direction.Left;
            }
            else
            {
                Direction = Direction.Right;
            }

        }
        if (Input.GetMouseButtonUp(0))
        {
            if (Ielong != null)
            {
                StopCoroutine(Ielong);
                Ielong = null;
            }
            
            secondPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            currentSwipe = new Vector2(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

            currentSwipe.Normalize();

            if (Mathf.Abs(currentSwipe.y) > swipeSensitivity || Mathf.Abs(currentSwipe.x) > swipeSensitivity)
            {
                if (currentSwipe.y > 0 && Mathf.Abs(currentSwipe.y) > Mathf.Abs(currentSwipe.x))
                {
                    if (!isLong)
                        PlayerSwipe();
                }
                //else if (currentSwipe.y < 0 && Mathf.Abs(currentSwipe.y) > Mathf.Abs(currentSwipe.x))
                //{
                //}
                //else if (currentSwipe.y < 0 && Mathf.Abs(currentSwipe.y) < Mathf.Abs(currentSwipe.x))
                //{
                //}
                //else if (currentSwipe.y > 0 && Mathf.Abs(currentSwipe.y) < Mathf.Abs(currentSwipe.x))
                //{

                //}
            }
            else
            {
                if (!isLong && !isSwipe)
                    PlayerTouch();
            }
            
        }
        

    } // 화면 컨트롤

    IEnumerator CR_LongTouch()
    {
        yield return new WaitForSeconds(0.5f);
        isLong = true;
        PlayerLongTouch();
    } // 긴 터치 코루틴

    public void PlayerTouch()
    {
        if ((!Anim.GetBool("isAttack") || Cancel) && (stats.Stamina >= 10))
        {                       
            stats.Stamina -= 10;
            State = PlayerCurState.WEAK_ATTACK;

            if (BackEndMatchManager.instance.isHost)
            {
                int keyCode = (int)State;
                KeyMessage msg = new KeyMessage(keyCode, transform.position);
                msg = new KeyMessage(keyCode, transform.position, Direction);

                BackEndMatchManager.instance.AddMsgToLocalQueue(msg);
            }
            else
            {
                PlayerWeakAttackMessage weakMsg = new PlayerWeakAttackMessage(index, Direction);
                BackEndMatchManager.instance.SendDataToInGame(weakMsg);
            }
        }
    } // 일반 터치

    public void PlayerSwipe()
    {
        isSwipe = true;
        if ((!Anim.GetBool("isAttack") || Cancel) && (stats.Stamina >= 20))
        {                
            stats.Stamina -= 20;
            State = PlayerCurState.STRONG_ATTACK;

            if (BackEndMatchManager.instance.isHost)
            {
                int keyCode = (int)State;
                KeyMessage msg = new KeyMessage(keyCode, transform.position);
                msg = new KeyMessage(keyCode, transform.position, Direction);

                BackEndMatchManager.instance.AddMsgToLocalQueue(msg);
            }
            else
            {
                PlayerStrongAttackMessage strongMsg = new PlayerStrongAttackMessage(index, Direction);
                BackEndMatchManager.instance.SendDataToInGame(strongMsg);
            }
        }
    } // 스와이프

    public void PlayerLongTouch()
    {
        if ((!Anim.GetBool("isAttack") || Cancel))
        {            
            State = PlayerCurState.DEFENSE;

            if (BackEndMatchManager.instance.isHost)
            {
                int keyCode = (int)State;
                KeyMessage msg = new KeyMessage(keyCode, transform.position);
                msg = new KeyMessage(keyCode, transform.position, Direction);

                BackEndMatchManager.instance.AddMsgToLocalQueue(msg);
            }
            else
            {
                PlayerDefenseMessage defenseMsg = new PlayerDefenseMessage(index, Direction);
                BackEndMatchManager.instance.SendDataToInGame(defenseMsg);
            }
        }
    } // 긴 터치

    #region 애니메이션에 넣을 함수들
    public bool GetAttackPoint()
    {
        return AttackPoint;
    }
    public void AttackPointTrue()
    {
        AttackPoint = true;
        // 계산 관리 함수
        CalculationMessage calMessage = new CalculationMessage(BackEndMatchManager.instance.hostSession);
        BackEndMatchManager.instance.SendDataToInGame(calMessage);
    }
    public void AttackPointFalse()
    {
        AttackPoint = false;
    }


    public void isStunTrue()
    {
        isStun = true;
    }
    public void isStunFalse()
    {
        isStun = false;
    }

    public void SufferDamage(double Damage)
    {
        PlayerDamagedMessage damagedMsg = new PlayerDamagedMessage(index, Damage);
        BackEndMatchManager.instance.SendDataToInGame(damagedMsg);
    }
    public void CancelTrue()
    {
        Cancel = true;
    }
    public void CancelFalse()
    {
        Cancel = false;
    }
    public void AnimationReset()
    {
        PlayerAttackEndMessage attackEndMsg = new PlayerAttackEndMessage(index);
        BackEndMatchManager.instance.SendDataToInGame(attackEndMsg);
    }
    #endregion

    public IEnumerator CR_DelayOn(float _Time)
    {
        yield return new WaitForSeconds(_Time);
        isDelay = false;
    } // 공격 딜레이 코루틴
    public void DelayOn(float _Time)
    {
        if (!isDelay)
        {
            isDelay = true;
            StartCoroutine(CR_DelayOn(_Time));
        }
    } //공격 딜레이 함수

}
