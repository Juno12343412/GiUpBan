using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public enum CharacterKind : byte
{
    나이트 = 0,
    벤전스,
    막시무스한,
    NONE = 99
}

public class PlayerStats : MonoBehaviour
{
    [Serializable]
    public class Player
    {
        public string name = "Ghest";
        public int gold = 100;       // 게임 재화
        public bool ads = false;     // 광고 유무
        public int mmr = 0;         /// mmr
        public double CurHp;
        public double MaxHp;
        public double Stamina;
        public double StaminaM;
        public double Damage;
        public double Penetration;
        public List<string> pName = new List<string>();
        public List<double> pCurHp = new List<double>();
        public List<double> pMaxHp = new List<double>();
        public List<double> pStamina = new List<double>();
        public List<double> pStaminaM = new List<double>();
        public List<double> pDamage = new List<double>();
        public List<double> pPenetration = new List<double>();
        public int nowCharacter = 0;
        public List<int> haveCharacters = new List<int>();
        public List<int> charactersLevel = new List<int>();
        public List<int> levelExp = new List<int>();
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
        BackEndServerManager.instance.myInfo.haveCharacters.Add(0);
        BackEndServerManager.instance.myInfo.haveCharacters.Add(1);
        BackEndServerManager.instance.myInfo.charactersLevel.Add(1);
        BackEndServerManager.instance.myInfo.charactersLevel.Add(1);
        BackEndServerManager.instance.myInfo.levelExp.Add(0);
        BackEndServerManager.instance.myInfo.levelExp.Add(0);

        param = new Param();
        param.Add("Name", BackEndServerManager.instance.myNickName);
        param.Add("Gold", BackEndServerManager.instance.myInfo.gold);
        param.Add("Ads", BackEndServerManager.instance.myInfo.ads);
        param.Add("HaveCharacters", BackEndServerManager.instance.myInfo.haveCharacters);
        param.Add("NowCharacter", BackEndServerManager.instance.myInfo.nowCharacter);
        param.Add("CharacterLevel", BackEndServerManager.instance.myInfo.charactersLevel);
        param.Add("LevelExp", BackEndServerManager.instance.myInfo.levelExp);
        SendQueue.Enqueue(Backend.GameInfo.Insert, table, param, callback =>
        {
            if (callback.IsSuccess())
            {
                param = new Param();
                param.Add("Score", BackEndServerManager.instance.myInfo.mmr);
                Backend.GameSchemaInfo.Insert("MMR", param);
            }
        });
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
        param.Add("CharacterLevel", BackEndServerManager.instance.myInfo.charactersLevel);
        param.Add("LevelExp", BackEndServerManager.instance.myInfo.levelExp);
        Backend.GameInfo.Update(table, BackEndServerManager.instance.myIndate, param);

        param = new Param();
        param.Add("Score", BackEndServerManager.instance.myInfo.mmr);
        Backend.GameSchemaInfo.Update("MMR", BackEndServerManager.instance.myIndate, param);
    }

   
    // 게임을 킬 때 사용하는 함수
    public void Load(string table = "UserData")
    {
        SendQueue.Enqueue(Backend.GameInfo.GetPrivateContents, table, 8, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log(callback.GetReturnValuetoJSON()["rows"][0]["CharacterLevel"]["L"][0]);

                Debug.Log(callback.Rows()[0]["HaveCharacters"]["L"]);
                BackEndServerManager.instance.myInfo.gold = Convert.ToInt32(callback.Rows()[0]["Gold"]["N"].ToString());
                BackEndServerManager.instance.myInfo.ads = Convert.ToBoolean(callback.Rows()[0]["Ads"]["BOOL"].ToString());
                for (int i = 0; i < callback.Rows()[0]["HaveCharacters"]["L"].Count; i++)
                    BackEndServerManager.instance.myInfo.haveCharacters.Add(Convert.ToInt32(callback.Rows()[0]["HaveCharacters"]["L"][i]["N"].ToString()));
                BackEndServerManager.instance.myInfo.nowCharacter = Convert.ToInt32(callback.Rows()[0]["NowCharacter"]["N"].ToString());
                for (int i = 0; i < callback.Rows()[0]["CharacterLevel"]["L"].Count; i++)
                    BackEndServerManager.instance.myInfo.charactersLevel.Add(Convert.ToInt32(callback.GetReturnValuetoJSON()["rows"][0]["CharacterLevel"]["L"][i]["N"].ToString()));
                for (int i = 0; i < callback.Rows()[0]["LevelExp"]["L"].Count; i++)
                    BackEndServerManager.instance.myInfo.levelExp.Add(Convert.ToInt32(callback.Rows()[0]["LevelExp"]["L"][i]["N"].ToString()));

                // 모든 캐릭터 정보
                SendQueue.Enqueue(Backend.Chart.GetChartContents, "10949", callback2 =>
                {
                    if (callback2.IsSuccess())
                    {
                        for (int i = 0; i < callback2.GetReturnValuetoJSON()["rows"].Count; i++)
                        {
                            Debug.Log("차트 불러오는 중 ... : " + i);
                            BackEndServerManager.instance.myInfo.pName.Add(callback2.GetReturnValuetoJSON()["rows"][i]["Name"]["S"].ToString());
                            BackEndServerManager.instance.myInfo.pMaxHp.Add(Convert.ToDouble(callback2.GetReturnValuetoJSON()["rows"][i]["Hp"]["S"].ToString()));
                            BackEndServerManager.instance.myInfo.pCurHp.Add(BackEndServerManager.instance.myInfo.pMaxHp[i]);
                            BackEndServerManager.instance.myInfo.pStamina.Add(Convert.ToDouble(callback2.GetReturnValuetoJSON()["rows"][i]["Stamina"]["S"].ToString()));
                            BackEndServerManager.instance.myInfo.pStaminaM.Add(Convert.ToDouble(callback2.GetReturnValuetoJSON()["rows"][i]["StaminaM"]["S"].ToString()));
                            BackEndServerManager.instance.myInfo.pDamage.Add(Convert.ToDouble(callback2.GetReturnValuetoJSON()["rows"][i]["Damage"]["S"].ToString()));
                            BackEndServerManager.instance.myInfo.pPenetration.Add(Convert.ToDouble(callback2.GetReturnValuetoJSON()["rows"][i]["Penetration"]["S"].ToString()));
                        }

                        SendQueue.Enqueue(Backend.GameSchemaInfo.Get, "MMR", BackEndServerManager.instance.myIndate, callback3 =>
                        {
                            Debug.Log("MMR : " + Convert.ToInt32(callback3.Rows()[0]["Score"]["N"].ToString()));
                        });

                        var errorLog = string.Format("로드완료 !\n이름 : {0}\n골드 : {1}\n광고 : {2}\nMMR : {3}", BackEndServerManager.instance.myNickName, BackEndServerManager.instance.myInfo.gold, BackEndServerManager.instance.myInfo.ads, BackEndServerManager.instance.myInfo.mmr);
                        Debug.Log(errorLog);
                    }
                });
            }
            else
            {
                Debug.Log("로드 정보 실패");
                Add();
            }
        });
    }

    public void SaveMMR()
    {
        param = new Param();
        param.Add("Score", BackEndServerManager.instance.myInfo.mmr);
        Backend.GameSchemaInfo.Update("MMR", BackEndServerManager.instance.myIndate, param);
    }
}
