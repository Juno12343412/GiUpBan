using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

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
    public static PlayerStats instance = null;
    private Param param = new Param();

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
    }

    public void Add(string table = "UserData")
    {
        param = new Param();
        param.Add("Name", BackEndServerManager.instance.myNickName);
        param.Add("Gold", BackEndServerManager.instance.myInfo.gold);
        param.Add("Ads", BackEndServerManager.instance.myInfo.ads);
        param.Add("WeaponCode", BackEndServerManager.instance.myInfo.curWeapon.code);
        param.Add("HelmetCode", BackEndServerManager.instance.myInfo.curHelmet.code);
        param.Add("ChestCode", BackEndServerManager.instance.myInfo.curChest.code);
        Backend.GameInfo.Insert(table, param);
    }

    public void Save(string table = "UserData")
    {
        param = new Param();
        param.Add("Name", BackEndServerManager.instance.myNickName);
        param.Add("Gold", BackEndServerManager.instance.myInfo.gold);
        param.Add("Ads", BackEndServerManager.instance.myInfo.ads);
        param.Add("WeaponCode", BackEndServerManager.instance.myInfo.curWeapon.code);
        param.Add("HelmetCode", BackEndServerManager.instance.myInfo.curHelmet.code);
        param.Add("ChestCode", BackEndServerManager.instance.myInfo.curChest.code);
        Backend.GameInfo.Update(table, BackEndServerManager.instance.myIndate, param);
    }

    public BackendReturnObject Load(string table = "UserData", int limit = 1)
    {
        var bro = Backend.GameInfo.GetPrivateContents(table, limit);
        
        // ...

        return bro;
    }
}
