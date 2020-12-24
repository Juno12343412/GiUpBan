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
    public GameObject[] CharactersPrefab = null;
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
    [HideInInspector] public Animator Anim;
    public Camera characterCamera = null;
    private CameraFuncs cameraFuncs = null;

    [SerializeField] private GameObject[] effectObjs = null;
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
        if (GameManager.instance.gameState == GameManager.GameState.InGame)
        {
            if (stats.CurHp <= 0)
            {
                if (BackEndMatchManager.instance.IsMySessionId(index))
                    cameraFuncs.SetShakeTime(0, 0);

                Time.timeScale = 1;

                WorldPackage.instance.playerDie(index);
            }

            if (isMe)
            {
                if (!isStun)
                {
                    if (!isDelay)
                        PlayerControl();
                }
            }
        }
    }

    public void PlayerSetting(bool isMe, SessionId index, string nickName)
    {
        this.isMe = isMe;
        this.index = index;
        this.nickName = nickName;

        if (this.isMe)
        {
            //Anim = CharactersPrefab[stats.nowCharacter].GetComponent<Animator>();

            // 여기다가 자기 자신 캐릭터에 따라 캐릭터 레벨에 따라 스탯 변경되는거 넣기
            // ...
            foreach (var prefab in CharactersPrefab)
            {
                prefab.SetActive(false);
            }
            //CharactersPrefab[stats.nowCharacter].SetActive(true);
            cameraFuncs = characterCamera.GetComponent<CameraFuncs>();
            stats = (PlayerStats.Player)BackEndServerManager.instance.myInfo.Copy();

            //Debug.Log("현재 캐릭터들 개수 : " + stats.charactersLevel[stats.nowCharacter]);
            //Debug.Log("현재 캐릭터 : " + stats.nowCharacter);

            if (stats.charactersLevel[stats.nowCharacter] == 1)
            {
                // 여기 수정 ...
                //stats.MaxHp = stats.pMaxHp[stats.nowCharacter];
                //stats.Stamina = stats.pStamina[stats.nowCharacter];
                //stats.StaminaM = stats.pStaminaM[stats.nowCharacter];
                //stats.Damage = stats.pDamage[stats.nowCharacter];
                //stats.Penetration = stats.pPenetration[stats.nowCharacter];
            }
            else if (stats.charactersLevel[stats.nowCharacter] >= 2)
            {
                // 여기 수정 ...
                //stats.MaxHp = stats.pMaxHp[stats.nowCharacter] * (stats.charactersLevel[stats.nowCharacter] * 0.6f);
                //stats.Stamina = stats.pStamina[stats.nowCharacter] * (stats.charactersLevel[stats.nowCharacter] * 0.6f);
                //stats.StaminaM = stats.pStaminaM[stats.nowCharacter] * (stats.charactersLevel[stats.nowCharacter] * 0.6f);
                //stats.Damage = stats.pDamage[stats.nowCharacter] * (stats.charactersLevel[stats.nowCharacter] * 0.6f);
                //stats.Penetration = stats.pPenetration[stats.nowCharacter] * (stats.charactersLevel[stats.nowCharacter] * 0.6f);
            }

            State = PlayerCurState.IDLE;

            Direction = Direction.NONE;
            //StartCoroutine(CR_StaminaHeal());

            transform.position = WorldPackage.instance.startingPoint[0].position;
            transform.rotation = WorldPackage.instance.startingPoint[0].rotation;
        }
        else
        {
            transform.position = WorldPackage.instance.startingPoint[1].position;
            transform.rotation = WorldPackage.instance.startingPoint[1].rotation;
        }

        isLive = true;
    }

    public void StunOn(double _Time)
    {
        isStun = true;
        StartCoroutine(CR_Stun((float)_Time));
    }

    public IEnumerator CR_Stun(float Time)
    {
        State = PlayerCurState.STUN;
        if (BackEndMatchManager.instance.isHost)
        {
            int keyCode = (int)State;
            KeyMessage msg = new KeyMessage(keyCode, transform.position, Direction);
            BackEndMatchManager.instance.AddMsgToLocalQueue(msg);
        }
        else
        {
            PlayerStunMessage stunMsg = new PlayerStunMessage(index);
            BackEndMatchManager.instance.SendDataToInGame(stunMsg);
        }
        yield return new WaitForSeconds(Time);
        isStun = false;
    }// 스턴 코루틴
    public IEnumerator CR_StaminaHeal()
    {
        while (GameManager.instance.gameState != GameManager.GameState.InGame || GameManager.instance.gameState != GameManager.GameState.GameStart)
        {
            if (!Anim.GetBool("isAttack") && !Anim.GetBool("isGroggy"))
            {
                yield return new WaitForSeconds(0.1f);
                if (stats.Stamina < 100)
                {
                    PlayerStaminaMessage staminaMsg = new PlayerStaminaMessage(index, stats.Stamina += 1);
                    BackEndMatchManager.instance.SendDataToInGame(staminaMsg);
                }
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
            stats.Stamina -= stats.ReductionStamina;
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
            stats.Stamina -= stats.ReductionStamina * 1.5f;
            State = PlayerCurState.STRONG_ATTACK;

            if (BackEndMatchManager.instance.isHost)
            {
                int keyCode = (int)State;
                KeyMessage msg = new KeyMessage(keyCode, transform.position, Direction);
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
                KeyMessage msg = new KeyMessage(keyCode, transform.position, Direction);
                BackEndMatchManager.instance.AddMsgToLocalQueue(msg);
            }
            else
            {
                PlayerDefenseMessage defenceMsg = new PlayerDefenseMessage(index, Direction);
                BackEndMatchManager.instance.SendDataToInGame(defenceMsg);
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

    public IEnumerator CR_DelayOn(double _Time)
    {
        yield return new WaitForSeconds((float)_Time);
        isDelay = false;
    } // 공격 딜레이 코루틴
    public void DelayOn(double _Time)
    {
        if (!isDelay)
        {
            isDelay = true;
            StartCoroutine(CR_DelayOn(_Time));
        }
    } //공격 딜레이 함수

    public void HitStop(float _Time, float _Scale)
    {
        cameraFuncs.SetShakeTime(_Time, _Scale);
        StartCoroutine(CR_TimeStop(_Time * 0.2f));
    }

    public IEnumerator CR_TimeStop(float _Time)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(_Time);
        Time.timeScale = 1;
    }

    public IEnumerator AttackEffect(float _delay)
    {
        if (!effectObjs[0].activeSelf)
        {
            effectObjs[0].gameObject.SetActive(true);
            yield return new WaitForSeconds(_delay);
            effectObjs[0].gameObject.SetActive(false);
        }
    }

    public IEnumerator DefenseEffect(float _delay)
    {
        if (!effectObjs[1].activeSelf)
        {
            effectObjs[1].gameObject.SetActive(true);
            yield return new WaitForSeconds(_delay);
            effectObjs[1].gameObject.SetActive(false);
        }
    }
}

