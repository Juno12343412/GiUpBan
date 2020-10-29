using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Serializable]
    public class Player
    {
        public string name = "Ghest";
        public long gold = 100;       // 게임 재화
        public bool ads = false;     // 광고 유무
        public float CurHp = 100;
        public float MaxHp = 100;
        public float Stamina = 100;
        public Weapon curWeapon = null; // 현재 무기 정보
        public Helmet curHelmet = null; // 현재 헬멧 정보
        public Chest curChest = null;  // 현재 갑옷 정보
        
        //가지고 있는 아이템 정보
        public List<Weapon> haveWeapons = new List<Weapon>();
        public List<Helmet> haveHelmets = new List<Helmet>();
        public List<Chest> haveChests = new List<Chest>();

        
    }
    [HideInInspector] public Player playerStats = new Player();

    public static PlayerStats instance = null;

    void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        instance = GetComponent<PlayerStats>();

        playerStats.curWeapon = playerStats.haveWeapons[0];
        playerStats.curHelmet = playerStats.haveHelmets[0];
        playerStats.curChest = playerStats.haveChests[0];
    }

 
}
