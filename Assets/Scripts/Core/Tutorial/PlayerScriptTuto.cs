using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.Pooling;

enum DirectionTuto : sbyte
{
    LEFT,
    RIGHT,
    NONE
}


public class PlayerScriptTuto : PoolingObject
{
    int Stamina = 100;
    DirectionTuto dir = new DirectionTuto();
    Animator anim = null;

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
    public Text guideText;
    public List<string> textList = null;
    public Image guideImage;
    #endregion

    bool pause = false;
    int tuto = 0;

    public Image[] Effects;

    void TextPlus(string text)
    {
        if (text.Length > 0)
        {
            char[] temp = text.ToCharArray(0, text.Length);
            for (int i = 0; i < temp.Length; i++)
            {
                string temp2 = temp[i].ToString();
                textList.Add(temp2);
            }
        }
    }

    IEnumerator CR_TextPrint()
    {
        while (true)
        {
            if (textList.Count > 0 && !pause)
            {
                for (int i = 1; i <= 6; i++)
                {
                    if (textList[0].Equals(i.ToString()))
                    {
                        AnimationReset();
                        textList.RemoveAt(0);
                        tuto = i;
                        pause = true;
                        yield return null;
                    }
                }
                if (textList[0] == "+")
                {
                    Debug.Log("asdf");
                    textList.RemoveAt(0);
                    tuto = 7;
                    yield return null;
                }
                else if (textList[0] == "?")
                {
                    textList.RemoveAt(0);
                    yield return new WaitForSeconds(0.5f);

                }
                else if (textList[0] == "/")
                {
                    textList.RemoveAt(0);
                    guideText.text = " ";
                    yield return null;
                }
                else
                {
                    yield return new WaitForSeconds(0.03f);
                    guideText.text += textList[0];
                    textList.RemoveAt(0);
                }
            }
            yield return null;
        }
    }

    void Start()
    {
        for (int i = 0; i < 4; i++)
            Effects[i].enabled = false;
        TextPlus("? ? ?안녕하세요 플레이어님!!? ?/");
        TextPlus("이곳은 플레이어님의 Log Bridge를 적응을 위한 튜토리얼 맵입니다.? ?/");
        TextPlus("튜토리얼을 모두 마친 후에 플레이어님은 LogBridge를 플레이 할 수 있게 됩니다.? ?/");
        TextPlus("화면을 터치하게 되면 약한 공격이 나갑니다.? ?/");
        TextPlus("약한 공격에 성공하면 상대는 데미지를 받게 되고 기절상태가 됩니다.? ?/");
        TextPlus("약한 공격연습.?/");
        TextPlus("왼쪽 화면을 터치해주세요.1?/");
        TextPlus("잘하셨습니다! ? ?/");
        TextPlus("이번에는 오른쪽 화면을 터치해주세요.2?/");
        TextPlus("잘하셨습니다! ? ?/");
        TextPlus("위로 스와이프하게 되면 강한 공격이 나갑니다.? ?/");
        TextPlus("강한 공격에 성공하면 상대는 큰 데미지를 받게 되고 기절상태가 됩니다.? ?/");
        TextPlus("강한 공격연습.?/");
        TextPlus("왼쪽 화면을 위로 스와이프 해주세요.3?/");
        TextPlus("잘하셨습니다! ? ?/");
        TextPlus("이번에는 오른쪽 화면을 위로 스와이프 해주세요.4?/");
        TextPlus("잘하셨습니다! ? ?/");
        TextPlus("화면 터치를 길게 하게 되면 방어 자세를 취합니다.? ?/");
        TextPlus("방어 자세에 돌입하게 되면 해당 방향의 공격의 데미지를 최소화하고? ?/");
        TextPlus("방어에 성공하면 상대는 기절상태가 됩니다.? ?/");
        TextPlus("일정 시간이 지난뒤에 방어 자세는 자동으로 해제가 됩니다.? ?/");
        TextPlus("시간이 지나기 전에 방어 자세를 해제하고 싶다면, 방어자세를 제외한 다른 행동을 하면 즉시 방어 자세에서 빠져나오며 해당 행동을 하게 됩니다.? ? ?/");
        TextPlus("방어 연습.?/");
        TextPlus("왼쪽 화면을 길게 터치 해주세요.5?/");
        TextPlus("잘하셨습니다! ? ?/");
        TextPlus("이번에는 오른쪽 화면을 길게 터치 해주세요.6?/");
        TextPlus("잘하셨습니다! ? ?/");
        TextPlus("캔슬 시스템에 대해 알려드리겠습니다?/");
        TextPlus("공격을 한 뒤 일정 시간 내에 해당 공격을 제외한 다른 공격을 하게 되면 즉시 기존 공격을 취소한 뒤 다음 공격을 하게 됩니다.? ? ?/");
        TextPlus("캔슬은 Log Bridge를 플레이하면서 연습하시길 바랍니다.? ?/");
        TextPlus("LogBridge 를 플레이할 준비가 다 된것 같군요! 튜토리얼을 마치겠습니다. ? ? /");
        TextPlus("행운을 빕니다 플레이어님 ? ? +");


        anim = GetComponent<Animator>();
        StartCoroutine(CR_StaminaHeal());
        StartCoroutine(CR_TextPrint());
    }

    void Update()
    {
        switch (tuto)
        {
            case 0:
                for (int i = 0; i < 4; i++)
                    Effects[i].enabled = false;
                break;
            case 1:
                PlayerControl(tuto);
                for(int i = 0; i < 4; i++)
                    Effects[i].enabled = false;
                Effects[0].enabled = true;
                break;
            case 2:
                PlayerControl(tuto);
                for (int i = 0; i < 4; i++)
                    Effects[i].enabled = false;
                Effects[1].enabled = true;
                break;

            case 3:
                PlayerControl(tuto);
                for (int i = 0; i < 4; i++)
                    Effects[i].enabled = false;
                Effects[2].enabled = true;
                break;

            case 4:
                PlayerControl(tuto);
                for (int i = 0; i < 4; i++)
                    Effects[i].enabled = false;
                Effects[3].enabled = true;
                break;

            case 5:
                PlayerControl(tuto);
                for (int i = 0; i < 4; i++)
                    Effects[i].enabled = false;
                Effects[0].enabled = true;
                break;
            case 6:
                PlayerControl(tuto);
                for (int i = 0; i < 4; i++)
                    Effects[i].enabled = false;
                Effects[1].enabled = true;
                break;
            case 7:
                guideImage.GetComponent<Animator>().SetBool("GameStart", true);
                characterCamera.GetComponent<Animator>().SetBool("GameStart", true);
                break;
        }

    }

    public IEnumerator CR_StaminaHeal()
    {
        if (!anim.GetBool("isAttack") && !anim.GetBool("isGroggy"))
        {
            yield return new WaitForSeconds(0.1f);
            if (Stamina < 100)
            {
                Stamina += 1;
            }
        }
        yield return null;

    } // 스테미너 회복
    public void StunOn(double _Time)
    {
        isStun = true;
        StartCoroutine(CR_Stun((float)_Time));
    }

    public IEnumerator CR_Stun(float Time)
    {
        AnimationReset();
        anim.SetBool("isGroggy", true);
        yield return new WaitForSeconds(Time);
        anim.SetBool("isGroggy", false);

        isStun = false;
    }// 스턴 코루틴
    IEnumerator Ielong = null; // 긴 터치 코루틴 담는 변수 코루틴스탑시 오류 방지
    public void PlayerControl(int num)
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
                dir = DirectionTuto.LEFT;
            }
            else
            {
                dir = DirectionTuto.RIGHT;
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
        if ((!anim.GetBool("isAttack") || Cancel))
        {
            AnimationReset();
            if (dir == DirectionTuto.LEFT && tuto == 1) { pause = false; anim.SetInteger("AttackDir", 0); tuto = 0; }
            else if (dir == DirectionTuto.RIGHT && tuto == 2) { pause = false; anim.SetInteger("AttackDir", 1); tuto = 0; }
            else return;
            anim.SetInteger("AttackKind", 1);
            anim.SetBool("isAttack", true);
        }
    } // 일반 터치

    public void PlayerSwipe()
    {
        isSwipe = true;
        if ((!anim.GetBool("isAttack") || Cancel))
        {
            AnimationReset();
            if (dir == DirectionTuto.LEFT && tuto == 3) { pause = false; anim.SetInteger("AttackDir", 0); tuto = 0; }
            else if (dir == DirectionTuto.RIGHT && tuto == 4) { pause = false; anim.SetInteger("AttackDir", 1); tuto = 0; }
            else return;

            anim.SetInteger("AttackKind", 2);
            anim.SetBool("isAttack", true);

        }
    } // 스와이프

    public void PlayerLongTouch()
    {
        if ((!anim.GetBool("isAttack") || Cancel))
        {
            AnimationReset();
            if (dir == DirectionTuto.LEFT && tuto == 5) { pause = false; anim.SetInteger("AttackDir", 0); tuto = 0; }
            else if (dir == DirectionTuto.RIGHT && tuto == 6) { pause = false; anim.SetInteger("AttackDir", 1); tuto = 0; }
            else return;

            anim.SetInteger("AttackKind", 3);
            anim.SetBool("isAttack", true);
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
        Debug.Log("asdf");
        anim.SetBool("isAttack", false);
        anim.SetBool("isGroggy", false);

        AttackPointFalse();
        CancelFalse();
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

    
}
