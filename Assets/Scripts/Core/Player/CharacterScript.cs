using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd.Tcp;
using BackEnd;
using Protocol;
using Manager.Pooling;


public class CharacterScript : MonoBehaviour
{
    private GameObject playerPrefabs = null;
    private PlayerScript playerScript = null;

    void Start()
    {
        playerPrefabs = transform.parent.gameObject;
        playerScript = playerPrefabs.GetComponent<PlayerScript>();
    }

    public void AttackPointTrue()
    {
        playerScript.AttackPointTrue();
    }
    public void AttackPointFalse()
    {
        playerScript.AttackPointFalse();

    }

    public void CancelTrue()
    {
        playerScript.CancelTrue();

    }
    public void CancelFalse()
    {
        playerScript.CancelFalse();

    }
    public void AnimationReset()
    {
        playerScript.AnimationReset();

    }
}

