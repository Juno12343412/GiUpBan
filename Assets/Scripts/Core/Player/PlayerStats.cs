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
        public int mmr = 0;         /// mmr
        public float CurHp = 100;
        public float MaxHp = 100;
        public float Stamina = 100;
        public int nowCharacter = 1;
        public List<int> haveCharacters;

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
        param.Add("HaveCharacters", BackEndServerManager.instance.myInfo.haveCharacters.ToArray());
        param.Add("NowCharacter", BackEndServerManager.instance.myInfo.nowCharacter);
        Backend.GameInfo.Insert(table, param);

        param = new Param();
        param.Add("Score", BackEndServerManager.instance.myInfo.mmr);
        Backend.GameSchemaInfo.Insert("MMR", param);
    }

    public void Save(string table = "UserData")
    {
        param = new Param();
        param.Add("Name", BackEndServerManager.instance.myNickName);
        param.Add("Gold", BackEndServerManager.instance.myInfo.gold);
        param.Add("Ads", BackEndServerManager.instance.myInfo.ads);
        param.Add("HaveCharacters", BackEndServerManager.instance.myInfo.haveCharacters.ToArray());
        param.Add("NowCharacter", BackEndServerManager.instance.myInfo.nowCharacter);
        Backend.GameInfo.Update(table, BackEndServerManager.instance.myIndate, param);

        param = new Param();
        param.Add("Score", BackEndServerManager.instance.myInfo.mmr);
        Backend.GameSchemaInfo.Update("MMR", BackEndServerManager.instance.myIndate, param);
    }

    public void Load(string table = "UserData")
    {
        // ...
        SendQueue.Enqueue(Backend.GameInfo.GetPrivateContents, table, 8, callback =>
        {
            Debug.Log(callback);
            BackEndServerManager.instance.myInfo.gold = Convert.ToInt32(callback.Rows()[0]["Gold"]["N"]);
            BackEndServerManager.instance.myInfo.ads = Convert.ToBoolean(callback.Rows()[0]["Ads"]["BOOL"]);
            //BackEndServerManager.instance.myInfo.haveCharacters = Convert.ToBase64CharArray(callback.Rows()[0]["NowCharacter"]["N"]);
            BackEndServerManager.instance.myInfo.nowCharacter = Convert.ToInt32(callback.Rows()[0]["NowCharacter"]["N"]);
        });

        SendQueue.Enqueue(Backend.GameSchemaInfo.Get, "MMR", BackEndServerManager.instance.myIndate, callback =>
        {
            Debug.Log("MMR : " + Convert.ToInt32(callback.Rows()[0]["Score"]["N"].ToString()));
        });

        var errorLog = string.Format("로드완료 !\n이름 : {0}\n골드 : {1}\n광고 : {2}\nMMR : {3}", BackEndServerManager.instance.myNickName, BackEndServerManager.instance.myInfo.gold, BackEndServerManager.instance.myInfo.ads, BackEndServerManager.instance.myInfo.mmr);
        Debug.Log(errorLog);
    }
}

