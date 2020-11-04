using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameManager : MonoBehaviour
{
    //#region 플레이어 관리 변수들
    //[HideInInspector] 
    //public PlayerScript p1, p2;
    //#endregion

    //void Start()
    //{     
    //    p1 = GameObject.Find("Player").GetComponent<PlayerScript>();
    //    p2 = GameObject.Find("Player2").GetComponent<PlayerScript>();
    //}
    
    //void Update()
    //{
    //    GameCalculation();
    //}
    //void GameCalculation()
    //{
    //    if(p1.State == PlayerCurState.WeakAttack && p1.GetAttackPoint() == true)
    //    {
    //        if (p2.State == PlayerCurState.Defense && p2.Direction == p1.Direction)
    //        {
    //            p2.SufferDamage(p2.Stats.curWeapon.Damage * (100.0f - ((p2.Stats.curChest.Defense +
    //                p2.Stats.curHelmet.Defense
    //                + p2.Stats.curWeapon.Defense) - p2.Stats.curWeapon.CrashDefense)) * 0.1f);

    //            p2.DelayOn(p1.Stats.curWeapon.AttackDelay);
    //        }
    //        else
    //            p2.SufferDamage(p2.Stats.curWeapon.Damage * (100.0f - ((p2.Stats.curChest.Defense +
    //                p2.Stats.curHelmet.Defense
    //                + p2.Stats.curWeapon.Defense) - p2.Stats.curWeapon.CrashDefense)));
    //    } // p1 약공
    //    if (p2.State == PlayerCurState.WeakAttack && p2.GetAttackPoint() == true)
    //    {
    //        if (p1.State == PlayerCurState.Defense && p1.Direction == p2.Direction)
    //        {
    //            p1.SufferDamage(p1.Stats.curWeapon.Damage * (100.0f - ((p1.Stats.curChest.Defense +
    //                p1.Stats.curHelmet.Defense
    //                + p1.Stats.curWeapon.Defense) - p1.Stats.curWeapon.CrashDefense)) * 0.1f);

    //            p1.DelayOn(p2.Stats.curWeapon.AttackDelay);
    //        }
    //        else
    //            p2.SufferDamage(p1.Stats.curWeapon.Damage * (100.0f - ((p1.Stats.curChest.Defense +
    //                p1.Stats.curHelmet.Defense
    //                + p1.Stats.curWeapon.Defense) - p1.Stats.curWeapon.CrashDefense)));
    //    } // p2 약공
    //    if (p1.State == PlayerCurState.StrongAttack && p1.GetAttackPoint() == true)
    //    {
    //        if (p2.State == PlayerCurState.Defense && p2.Direction == p1.Direction)
    //        {
    //            p2.SufferDamage(p2.Stats.curWeapon.Damage * (100.0f - ((p2.Stats.curChest.Defense +
    //                p2.Stats.curHelmet.Defense
    //                + p2.Stats.curWeapon.Defense) - p2.Stats.curWeapon.CrashDefense)) * 0.1f);

    //            p2.DelayOn(p1.Stats.curWeapon.AttackDelay * 1.5f);
    //        }
    //        else
    //            p2.SufferDamage(p2.Stats.curWeapon.Damage * (100.0f - ((p2.Stats.curChest.Defense +
    //                p2.Stats.curHelmet.Defense
    //                + p2.Stats.curWeapon.Defense) - p2.Stats.curWeapon.CrashDefense)));
    //    } //p1 강공
    //    if (p2.State == PlayerCurState.StrongAttack && p2.GetAttackPoint() == true)
    //    {
    //        if (p1.State == PlayerCurState.Defense && p1.Direction == p2.Direction)
    //        {
    //            p1.SufferDamage(p1.Stats.curWeapon.Damage * (100.0f - ((p1.Stats.curChest.Defense +
    //                p1.Stats.curHelmet.Defense
    //                + p1.Stats.curWeapon.Defense) - p1.Stats.curWeapon.CrashDefense)) * 0.1f);

    //            p1.DelayOn(p2.Stats.curWeapon.AttackDelay * 1.5f);
    //        }
    //        else
    //            p2.SufferDamage(p1.Stats.curWeapon.Damage * (100.0f - ((p1.Stats.curChest.Defense +
    //                p1.Stats.curHelmet.Defense
    //                + p1.Stats.curWeapon.Defense) - p1.Stats.curWeapon.CrashDefense)));
    //    } //p2 강공
    //} // 데미지 계산
}
