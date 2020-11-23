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
        public double CurHp;
        public double MaxHp;
        public double Stamina;
        public double StaminaM;
        public double Damage;
        public double Penetration;
        public double[] pCurHp;
        public double[] pMaxHp;
        public double[] pStamina;
        public double[] pStaminaM;
        public double[] pDamage;
        public double[] pPenetration;
        public int nowCharacter = 1;
        public List<int> haveCharacters = null;
        public List<int> charactersLevel = null;
        public List<int> levelExp = null;

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
        param.Add("CharacterLevel", BackEndServerManager.instance.myInfo.charactersLevel);
        param.Add("LevelExp", BackEndServerManager.instance.myInfo.levelExp);
        Backend.GameInfo.Insert(table, param);

        param = new Param();
        param.Add("Score", BackEndServerManager.instance.myInfo.mmr);
        Backend.GameSchemaInfo.Insert("MMR", param);
    }

    // 게임을 킬 때 사용하는 함수
    public void Save(string table = "UserData")
    {
        param = new Param();
        param.Add("Name", BackEndServerManager.instance.myNickName);
        param.Add("Gold", BackEndServerManager.instance.myInfo.gold);
        param.Add("Ads", BackEndServerManager.instance.myInfo.ads);
        param.Add("HaveCharacters", BackEndServerManager.instance.myInfo.haveCharacters);
        param.Add("NowCharacter", BackEndServerManager.instance.myInfo.nowCharacter);
        param.Add("CharacterLevel", BackEndServerManager.instance.myInfo.nowCharacter);
        param.Add("LevelExp", BackEndServerManager.instance.myInfo.levelExp);
        Backend.GameInfo.Update(table, BackEndServerManager.instance.myIndate, param);

        param = new Param();
        param.Add("Score", BackEndServerManager.instance.myInfo.mmr);
        Backend.GameSchemaInfo.Update("MMR", BackEndServerManager.instance.myIndate, param);
    }

    // 인게임에서 사용하는 함수
    public void CharacterSave(string table = "UserData")
    {
        param = new Param();
        param.Add("NowCharacter", BackEndServerManager.instance.myInfo.nowCharacter);
        param.Add("HaveCharacters", BackEndServerManager.instance.myInfo.haveCharacters);
        param.Add("CharacterLevel", BackEndServerManager.instance.myInfo.nowCharacter);
        param.Add("LevelExp", BackEndServerManager.instance.myInfo.levelExp);

        Backend.GameInfo.Update(table, BackEndServerManager.instance.myIndate, param);
    }

    // 인게임에서 사용하는 함수
    public void CharacterLoad(string table = "UserData")
    {
        SendQueue.Enqueue(Backend.GameInfo.GetPrivateContents, table, 8, callback =>
        {
            BackEndServerManager.instance.myInfo.nowCharacter = Convert.ToInt32(callback.Rows()[0]["NowCharacter"]["N"]);
            for (int i = 0; i < (callback.Rows()[0]["NowCharacter"]["L"]).Count; i++)
                BackEndServerManager.instance.myInfo.haveCharacters[i] = Convert.ToInt32(callback.Rows()[0]["HaveCharacters"]["L"][i]["N"]);
            for (int i = 0; i < (callback.Rows()[0]["CharacterLevel"]["L"]).Count; i++)
                BackEndServerManager.instance.myInfo.charactersLevel[i] = Convert.ToInt32(callback.Rows()[0]["CharacterLevel"]["L"][i]["N"]);
            for (int i = 0; i < (callback.Rows()[0]["LevelExp"]["L"]).Count; i++)
                BackEndServerManager.instance.myInfo.charactersLevel[i] = Convert.ToInt32(callback.Rows()[0]["LevelExp"]["L"][i]["N"]);
        });
    }

    // 게임을 킬 때 사용하는 함수
    public void Load(string table = "UserData")
    {
        // ...
        SendQueue.Enqueue(Backend.GameInfo.GetPrivateContents, table, 8, callback =>
        {
            BackEndServerManager.instance.myInfo.gold = Convert.ToInt32(callback.Rows()[0]["Gold"]["N"]);
            BackEndServerManager.instance.myInfo.ads = Convert.ToBoolean(callback.Rows()[0]["Ads"]["BOOL"]);
            for(int i = 0; i < (callback.Rows()[0]["NowCharacter"]["L"]).Count; i++)
                BackEndServerManager.instance.myInfo.haveCharacters[i] = Convert.ToInt32(callback.Rows()[0]["HaveCharacters"]["L"][i]["N"]);
            BackEndServerManager.instance.myInfo.nowCharacter = Convert.ToInt32(callback.Rows()[0]["NowCharacter"]["N"]);
            for (int i = 0; i < (callback.Rows()[0]["CharacterLevel"]["L"]).Count; i++)
                BackEndServerManager.instance.myInfo.charactersLevel[i] = Convert.ToInt32(callback.Rows()[0]["CharacterLevel"]["L"][i]["N"]);
            for (int i = 0; i < (callback.Rows()[0]["LevelExp"]["L"]).Count; i++)
                BackEndServerManager.instance.myInfo.charactersLevel[i] = Convert.ToInt32(callback.Rows()[0]["LevelExp"]["L"][i]["N"]);
        });

        // 모든 캐릭터 정보
        SendQueue.Enqueue(Backend.Chart.GetChartContents, "10714", callback =>
        {
            if (callback.IsSuccess())
            {
                for (int i = 0; i < callback.Rows()[0]["10714"].Count; i++)
                {
                    if (i != 0)
                    {
                        BackEndServerManager.instance.myInfo.pMaxHp[i] = Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["Character"]["S"]);
                        BackEndServerManager.instance.myInfo.pCurHp[i] = BackEndServerManager.instance.myInfo.pMaxHp[i];
                        BackEndServerManager.instance.myInfo.pStamina[i] = Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["Character"]["S"]);
                        BackEndServerManager.instance.myInfo.pStaminaM[i] = Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["Character"]["S"]);
                        BackEndServerManager.instance.myInfo.pDamage[i] = Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["Character"]["S"]);
                        BackEndServerManager.instance.myInfo.pPenetration[i] = Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["Character"]["S"]);
                    }
                    else
                    {
                        BackEndServerManager.instance.myInfo.pMaxHp[0] = 0;
                        BackEndServerManager.instance.myInfo.pCurHp[0] = 0;
                        BackEndServerManager.instance.myInfo.pStamina[0] = 0;
                        BackEndServerManager.instance.myInfo.pStaminaM[0] = 0;
                        BackEndServerManager.instance.myInfo.pDamage[0] = 0;
                        BackEndServerManager.instance.myInfo.pPenetration[0] = 0;
                    }                                       
                }                                           
            }
        });

        SendQueue.Enqueue(Backend.GameSchemaInfo.Get, "MMR", BackEndServerManager.instance.myIndate, callback =>
        {
            Debug.Log("MMR : " + Convert.ToInt32(callback.Rows()[0]["Score"]["N"].ToString()));
        });

        var errorLog = string.Format("로드완료 !\n이름 : {0}\n골드 : {1}\n광고 : {2}\nMMR : {3}", BackEndServerManager.instance.myNickName, BackEndServerManager.instance.myInfo.gold, BackEndServerManager.instance.myInfo.ads, BackEndServerManager.instance.myInfo.mmr);
        Debug.Log(errorLog);
    }
}

