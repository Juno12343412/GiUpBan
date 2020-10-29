using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCurState
{
    Idle,
    WeakAttack,
    StrongAttack,
    Defense,
    Stun
} // 플레이어 행동 상태

public enum PlayerDirection
{
    None,
    Left,
    Right
} // 플레이어 행동 방향

public class PlayerScript : MonoBehaviour
{
    #region 플레이어 관련 변수들
    [SerializeField]
    private PlayerStats.Player stats = new PlayerStats.Player();
    public PlayerStats.Player Stats { get { return stats; } }

    public PlayerCurState State = new PlayerCurState();
    public PlayerDirection Direction = new PlayerDirection();
    #endregion
    #region 화면 조작 관련 변수들
    private Vector2 touchBeganPos;
    private Vector2 touchEndedPos;
    private Vector2 touchDif;
    private Vector2 firstPressPos;
    private Vector2 secondPressPos;
    private Vector2 currentSwipe;
    private bool isLong = false;
    [Tooltip("스와이프 민감도")] public float swipeSensitivity = 0.3f;
    #endregion
    #region 기타 변수들
    private bool Cancel = false;
    private bool AttackPoint = false;
    private bool isStun = false;
    private bool isDelay = false;
    private Animator Anim;
    #endregion
    void Awake()
    {
        State = PlayerCurState.Idle;
        Direction = PlayerDirection.None;
        Anim = GetComponent<Animator>();
    }
    void Start()
    {               
        StartCoroutine(CR_StaminaHeal());
    } 

    void Update()
    {        
        if (!isStun)
        {
            if(!isDelay)
                PlayerControl();           
        }
        else {   State = PlayerCurState.Stun;    }
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
        if (!Anim.GetBool("isAttack"))
        {
            yield return new WaitForSeconds(0.1f);
            if(stats.Stamina < 100)
                stats.Stamina += 1;
        }
    } // 스테미너 회복

    IEnumerator Ielong = null; // 긴 터치 코루틴 담는 변수 코루틴스탑시 오류 방지
    public void PlayerControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isLong = false;
            firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Ielong = CR_LongTouch();
            StartCoroutine(Ielong);
            if (firstPressPos.x < Screen.width * 0.5)
            {
                Direction = PlayerDirection.Left;
                Anim.SetInteger("AttackDir", 0);

            }
            else
            {
                Direction = PlayerDirection.Right;
                Anim.SetInteger("AttackDir", 1);

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
                if (!isLong)
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
        if ((!Anim.GetBool("isAttack") || Cancel) && (stats.Stamina >= stats.curWeapon.StaminaMinus))
        {
            if (Cancel)
            {
                AnimationReset();
                Anim.SetInteger("AttackKind", 1);
                Anim.SetBool("isAttack", true);
                stats.Stamina -= stats.curWeapon.StaminaMinus;
                State = PlayerCurState.WeakAttack;
                Debug.Log("캔슬된" + State + " " + Direction);
                
                return;
            }
            
            Anim.SetInteger("AttackKind", 1);
            Anim.SetBool("isAttack", true);
            stats.Stamina -= stats.curWeapon.StaminaMinus;
            State = PlayerCurState.WeakAttack;
            Debug.Log(State + " " + Direction);
        }
    } // 일반 터치

    public void PlayerSwipe()
    {
        if ((!Anim.GetBool("isAttack") || Cancel) && (stats.Stamina >= stats.curWeapon.StaminaMinus))
        {
            if (Cancel)
            {
                AnimationReset();
                Anim.SetInteger("AttackKind", 2);
                Anim.SetBool("isAttack", true);
                stats.Stamina -= stats.curWeapon.StaminaMinus * 1.5f;
                State = PlayerCurState.StrongAttack;

                Debug.Log("캔슬된" + State + " " + Direction);
            }
            Anim.SetInteger("AttackKind", 2);
            Anim.SetBool("isAttack", true);
            stats.Stamina -= stats.curWeapon.StaminaMinus * 1.5f;
            State = PlayerCurState.StrongAttack;

            Debug.Log(State + " " + Direction);
        }
    } // 스와이프

    public void PlayerLongTouch()
    {
        if ((!Anim.GetBool("isAttack") || Cancel))
        {
            if (Cancel)
            {
                AnimationReset();
                Anim.SetInteger("AttackKind", 3);
                Anim.SetBool("isAttack", true);
                State = PlayerCurState.Defense;

                Debug.Log("캔슬된" + State + " " + Direction);
            }
            Anim.SetInteger("AttackKind", 3);
            Anim.SetBool("isAttack", true);
            State = PlayerCurState.Defense;

            Debug.Log(State + " " + Direction);
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


    public void SufferDamage(float Damage)
    {
        stats.CurHp -= Damage;
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
        Anim.SetBool("isAttack", false);
        AttackPoint = false;
        Cancel = false;
        Debug.Log(Anim.GetBool("isAttack"));
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
