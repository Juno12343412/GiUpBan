using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 //NONE = 0,
 //   IDLE = 1,   // 아무것도 안하는 메시지
 //   WEAK_ATTACK = 2,   // 약한 공격 메시지
 //   STRONG_ATTACK = 3,   // 강한 공격 메시지
 //   DEFENSE = 4,   // 방어 메시지
 //   STUN = 5,  // 스턴 메시지
 //   ATTACK_END = 6  // 공격 종료 메세지

public class AIScript : MonoBehaviour
{
    public PlayerScript ps;
    public bool isStart = false;

    void Start()
    {
        ps = GetComponent<PlayerScript>();
        ps.State = PlayerCurState.IDLE;
        //StartCoroutine("OnFSM");
    }

    public IEnumerator OnFSM()
    {
        Debug.Log("머신 작동 : " + ps.State.ToString());
        isStart = true;
        StartCoroutine(ps.State.ToString());
        StartCoroutine(ps.CR_StaminaHeal());
        yield return null;
    }

    public IEnumerator IDLE()
    {
        ps.Anim.SetBool("isAttack", false);
        while (ps.State == PlayerCurState.IDLE)
        {
            // TO DO..
            Debug.Log("IDLE");

            yield return new WaitForSeconds(Random.Range(1f, 3f));
            if (!ps.isStun)
            {
                if (!ps.isDelay)
                {
                    ps.Direction = (Protocol.Direction)Random.Range((int)Protocol.Direction.Left, 2);
                    ps.State = (PlayerCurState)Random.Range((int)PlayerCurState.WEAK_ATTACK, (int)PlayerCurState.STUN);
                }
            }
        }
        ChanageState();
    }

    public IEnumerator WEAK_ATTACK()
    {
        ps.PlayerAITouch();
        
        while (ps.State == PlayerCurState.WEAK_ATTACK)
        {
            // TO DO..
            Debug.Log("WA");

            yield return null;
        }
        ChanageState();
    }

    public IEnumerator STRONG_ATTACK()
    {
        ps.PlayerAISwipe();
        while (ps.State == PlayerCurState.STRONG_ATTACK)
        {
            // TO DO..
            Debug.Log("SA");

            yield return null;
        }
        ChanageState();
    }

    public IEnumerator DEFENSE()
    {
        ps.PlayerAILongTouch();
        while (ps.State == PlayerCurState.DEFENSE)
        {
            // TO DO..
            Debug.Log("DEFENSE");

            yield return new WaitForSeconds(1.5f);
            ps.State = PlayerCurState.IDLE;
        }
        ChanageState();
    }

    public IEnumerator STUN()
    {
        ps.State = PlayerCurState.IDLE;
        while (ps.State == PlayerCurState.STUN)
        {
            // TO DO..
            Debug.Log("STUN");

            yield return new WaitForSeconds(1.5f);
        }
        ChanageState();
    }

    public IEnumerator ATTACK_END()
    {
        while (ps.State == PlayerCurState.ATTACK_END)
        {
            // TO DO..
            Debug.Log("AE");

            yield return null;
        }
        ChanageState();
    }

    public void ChanageState()
    {
        StartCoroutine(ps.State.ToString());
    }
}
