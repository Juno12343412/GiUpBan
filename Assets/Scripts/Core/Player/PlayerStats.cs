using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public enum CharacterKind : byte
{
    나이트 = 0,
    벤전스,
    듀얼,
    도끼,
    MAX,
    NONE = 99
}

public class PlayerStats : MonoBehaviour
{
    [Serializable]
    public class Player
    {
        [Header("ETC")]
        // ETC
        public int gold = 100;        // 게임 재화
        public int diamond = 100;     // 게임 유료재화
        public bool ads = false;      // 광고 유무
        public int mmr = 0;           // mmr
        // ETC 

        [Header("Chracter")]
        // Chracter
        public int nowCharacter = 0;
        public List<int> haveCharacters = new List<int>();
        public List<int> charactersLevel = new List<int>();
        public List<int> levelExp = new List<int>();
        // Chracter

        [Header("Stats")]
        // Stats
        public double CurHp;
        public double MaxHp;
        public double Armor;
        public double Stamina;
        public double MaxStamina;
        public double ReductionStamina;
        public double WeakAttackDamage;
        public double WeakAttackStun;
        public double WeakAttackPenetration;
        public double StrongAttackDamage;
        public double StrongAttackStun;
        public double StrongAttackPenetration;
        public double DefeneseReceivingDamage;
        public double DefeneseReductionStun;
        // Stats

        [Header("Chart")]
        // Chart
        [HideInInspector] public List<string> pName = new List<string>();                           // 캐릭터 이름
        [HideInInspector] public List<double> pCurHp = new List<double>();                          // 캐릭터 체력
        [HideInInspector] public List<double> pMaxHp = new List<double>();                          // 캐릭터 최대 체력
        [HideInInspector] public List<double> pArmor = new List<double>();                          // 캐릭터 방어력
        [HideInInspector] public List<double> pStamina = new List<double>();                        // 캐릭터 스태미나
        [HideInInspector] public List<double> pMaxStamina = new List<double>();                     // 캐릭터 최대 스태미나
        [HideInInspector] public List<double> pReductionStamina = new List<double>();               // 캐릭터 스태미나 감소량
        [HideInInspector] public List<double> pWeakAttackDamage = new List<double>();               // 캐릭터 약한 공격 데미지
        [HideInInspector] public List<double> pWeakAttackStun = new List<double>();                 // 캐릭터 약한 공격 스턴시간
        [HideInInspector] public List<double> pWeakAttackPenetration = new List<double>();          // 캐릭터 강한 공격 방무
        [HideInInspector] public List<double> pStrongAttackDamage = new List<double>();             // 캐릭터 강한 공격 데미지
        [HideInInspector] public List<double> pStrongAttackStun = new List<double>();               // 캐릭터 강한 공격 스턴시간
        [HideInInspector] public List<double> pStrongAttackPenetration = new List<double>();        // 캐릭터 강한 공격 방무
        [HideInInspector] public List<double> pDefenseReceivingDamage = new List<double>();         // 캐릭터 방어시 데미지 받는 %
        [HideInInspector] public List<double> pDefenseReductionStun = new List<double>();           // 캐릭터 방어시 받는 스턴시간
        // Chart

        [Header("Chest")]
        // Chest
        public List<int> haveChestKind = new List<int>();    // 가지고 있는 상자 종류
        public List<int> haveChestState = new List<int>();   // 가지고 있는 상자 상태
        public string disStartTime = "";                     // 해제하고 있는 상자의 시작 시간
        public int disChest = 0;                             // 해제하고 있는 상자
        public int haveChests = 0;                           // 가지고 있는 상자 개수
        // Chest

        [Header("Card")]
        // Card
        public List<int> cardKind = new List<int>();         // 카드의 종류
        public List<int> cardCount = new List<int>();        // 카드의 개수
        // Card

        public object Copy()
        {
            return this.MemberwiseClone();
        }
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
        BackEndServerManager.instance.myInfo.charactersLevel.InsertRange(index: 0, collection: new List<int>() { 1, 1 });
        BackEndServerManager.instance.myInfo.levelExp.InsertRange(index: 0, collection: new List<int>() { 1, 1 });

        param = new Param();
        param.Add("Gold", BackEndServerManager.instance.myInfo.gold);
        param.Add("Diamond", BackEndServerManager.instance.myInfo.diamond);
        param.Add("Ads", BackEndServerManager.instance.myInfo.ads);
        param.Add("HaveCharacters", BackEndServerManager.instance.myInfo.haveCharacters);
        param.Add("NowCharacter", BackEndServerManager.instance.myInfo.nowCharacter);
        param.Add("CharacterLevel", BackEndServerManager.instance.myInfo.charactersLevel);
        param.Add("LevelExp", BackEndServerManager.instance.myInfo.levelExp);
        SendQueue.Enqueue(Backend.GameInfo.Insert, table, param, callback =>
        {
            if (callback.IsSuccess())
            {
                AddMMR();
                AddChest();
                AddCard();

                LoadChart();
            }
        });
    }

    // 게임을 킬 때 사용하는 함수
    public void Save(string table = "UserData")
    {
        Debug.Log("세이브중");

        param = new Param();
        param.Add("Gold", BackEndServerManager.instance.myInfo.gold);
        param.Add("Diamond", BackEndServerManager.instance.myInfo.diamond);
        param.Add("Ads", BackEndServerManager.instance.myInfo.ads);
        param.Add("HaveCharacters", BackEndServerManager.instance.myInfo.haveCharacters);
        param.Add("NowCharacter", BackEndServerManager.instance.myInfo.nowCharacter);
        param.Add("CharacterLevel", BackEndServerManager.instance.myInfo.charactersLevel);
        param.Add("LevelExp", BackEndServerManager.instance.myInfo.levelExp);
        Backend.GameInfo.Update(table, BackEndServerManager.instance.myUserDataIndate, param);

        SaveMMR();
        SaveChest();
        SaveCard();
    }


    // 게임을 킬 때 사용하는 함수
    public void Load(string table = "UserData")
    {
        SendQueue.Enqueue(Backend.GameInfo.GetPrivateContents, table, 7, callback =>
        {
            if (callback.IsSuccess())
            {
                if (callback.Rows().Count == 0)
                {
                    Debug.Log("로드 정보 실패");
                    Add();
                    return;
                }
                else
                {
                    BackEndServerManager.instance.myInfo.gold = Convert.ToInt32(callback.Rows()[0]["Gold"]["N"].ToString());
                    BackEndServerManager.instance.myInfo.diamond = Convert.ToInt32(callback.Rows()[0]["Diamond"]["N"].ToString());
                    BackEndServerManager.instance.myInfo.ads = Convert.ToBoolean(callback.Rows()[0]["Ads"]["BOOL"].ToString());
                    BackEndServerManager.instance.myInfo.nowCharacter = Convert.ToInt32(callback.Rows()[0]["NowCharacter"]["N"].ToString());
                    for (int i = 0; i < callback.Rows()[0]["HaveCharacters"]["L"].Count; i++)
                    {
                        BackEndServerManager.instance.myInfo.haveCharacters.Add(Convert.ToInt32(callback.Rows()[0]["HaveCharacters"]["L"][i]["N"].ToString()));
                    }
                    for (int i = 0; i < callback.Rows()[0]["CharacterLevel"]["L"].Count; i++)
                    {
                        BackEndServerManager.instance.myInfo.charactersLevel.Add(Convert.ToInt32(callback.GetReturnValuetoJSON()["rows"][0]["CharacterLevel"]["L"][i]["N"].ToString()));
                    }
                    for (int i = 0; i < callback.Rows()[0]["LevelExp"]["L"].Count; i++)
                    {
                        BackEndServerManager.instance.myInfo.levelExp.Add(Convert.ToInt32(callback.Rows()[0]["LevelExp"]["L"][i]["N"].ToString()));
                    }
                    BackEndServerManager.instance.myUserDataIndate = callback.Rows()[0]["inDate"]["S"].ToString();

                    LoadMMR();
                    LoadChest();
                    LoadCard();

                    LoadChart();

                    var errorLog = string.Format("로드완료 !\n이름 : {0}\n골드 : {1}\n광고 : {2}\nMMR : {3}", BackEndServerManager.instance.myNickName, BackEndServerManager.instance.myInfo.gold, BackEndServerManager.instance.myInfo.ads, BackEndServerManager.instance.myInfo.mmr);
                    Debug.Log(errorLog);

                }
            }
            else
            {
                Debug.Log("로드 정보 실패");
                Add();
            }
        });
    }

    public void LoadChart()
    {
        // 모든 캐릭터 정보
        SendQueue.Enqueue(Backend.Chart.GetChartContents, "11411", callback =>
        {
            if (callback.IsSuccess())
            {
                for (int i = 0; i < callback.GetReturnValuetoJSON()["rows"].Count; i++)
                {
                    Debug.Log("캐릭터 차트 불러오는 중 ... : " + i);
                    BackEndServerManager.instance.myInfo.pName.Add(callback.GetReturnValuetoJSON()["rows"][i]["Name"]["S"].ToString());
                    BackEndServerManager.instance.myInfo.pMaxHp.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["Hp"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pCurHp.Add(BackEndServerManager.instance.myInfo.pMaxHp[i]);
                    BackEndServerManager.instance.myInfo.pArmor.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["Armor"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pStamina.Add(100);
                    BackEndServerManager.instance.myInfo.pMaxStamina.Add(100);
                    BackEndServerManager.instance.myInfo.pReductionStamina.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["ReductionStamina"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pWeakAttackDamage.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["WeakAttack_Damage"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pWeakAttackStun.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["WeakAttack_Stun"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pWeakAttackPenetration.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["WeakAttack_ArmorPenetration"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pStrongAttackDamage.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["StrongAttack_Damage"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pStrongAttackStun.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["StrongAttack_Stun"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pStrongAttackPenetration.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["StrongAttack_ArmorPenetration"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pDefenseReceivingDamage.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["Defense_ReceivingDamage"]["S"].ToString()));
                    BackEndServerManager.instance.myInfo.pDefenseReductionStun.Add(Convert.ToDouble(callback.GetReturnValuetoJSON()["rows"][i]["Defense_ReductionStun"]["S"].ToString()));
                }

                SetStats();
            }
        });
    }

    public void AddMMR()
    {
        param = new Param();
        param.Add("Score", BackEndServerManager.instance.myInfo.mmr);
        Backend.GameSchemaInfo.Insert("MMR", param);
    }

    public void SaveMMR()
    {
        param = new Param();
        param.Add("Score", BackEndServerManager.instance.myInfo.mmr);
        Backend.GameSchemaInfo.Update("MMR", BackEndServerManager.instance.myMMRDataIndate, param);
    }

    public void LoadMMR()
    {
        SendQueue.Enqueue(Backend.GameSchemaInfo.Get, "MMR", BackEndServerManager.instance.myUserDataIndate, callback =>
        {
            Debug.Log("MMR : " + Convert.ToInt32(callback.Rows()[0]["Score"]["N"].ToString()));
            BackEndServerManager.instance.myInfo.mmr = Convert.ToInt32(callback.Rows()[0]["Score"]["N"].ToString());
            BackEndServerManager.instance.myMMRDataIndate = callback.Rows()[0]["inDate"]["S"].ToString();
        });
    }

    public void AddChest()
    {
        BackEndServerManager.instance.myInfo.haveChestKind.InsertRange(index: 0, collection: new List<int>() { 99, 99, 99 });
        BackEndServerManager.instance.myInfo.haveChestState.InsertRange(index: 0, collection: new List<int>() { 99, 99, 99 });

        param = new Param();
        param.Add("ChestKind", BackEndServerManager.instance.myInfo.haveChestKind);
        param.Add("ChestState", BackEndServerManager.instance.myInfo.haveChestState);
        param.Add("StartTime", BackEndServerManager.instance.myInfo.disStartTime);
        param.Add("DisChest", BackEndServerManager.instance.myInfo.disChest);
        param.Add("HaveChests", BackEndServerManager.instance.myInfo.haveChests);
        SendQueue.Enqueue(Backend.GameInfo.Insert, "ChestData", param, callback =>
        {
            if (callback.IsSuccess())
                Debug.Log("상자 정보 추가성공");
        });
    }

    public void SaveChest()
    {
        Debug.Log("상자 정보 세이브");

        param = new Param();
        param.Add("ChestKind", BackEndServerManager.instance.myInfo.haveChestKind);
        param.Add("ChestState", BackEndServerManager.instance.myInfo.haveChestState);
        param.Add("StartTime", BackEndServerManager.instance.myInfo.disStartTime);
        param.Add("DisChest", BackEndServerManager.instance.myInfo.disChest);
        param.Add("HaveChests", BackEndServerManager.instance.myInfo.haveChests);
        Backend.GameInfo.Update("ChestData", BackEndServerManager.instance.myChestDataIndate, param);
    }

    public void LoadChest()
    {
        SendQueue.Enqueue(Backend.GameInfo.GetPrivateContents, "ChestData", 5, callback =>
        {
            if (callback.IsSuccess())
            {
                if (callback.Rows().Count == 0)
                {
                    Debug.Log("로드 정보 실패");
                    AddChest();
                }
                else
                {
                    Debug.Log("상자 로드 성공");
                    for (int i = 0; i < callback.Rows()[0]["ChestKind"]["L"].Count; i++)
                    {
                        BackEndServerManager.instance.myInfo.haveChestKind.Add(Convert.ToInt32(callback.Rows()[0]["ChestKind"]["L"][i]["N"].ToString()));
                    }
                    for (int i = 0; i < callback.Rows()[0]["ChestState"]["L"].Count; i++)
                    {
                        BackEndServerManager.instance.myInfo.haveChestState.Add(Convert.ToInt32(callback.Rows()[0]["ChestState"]["L"][i]["N"].ToString()));
                    }
                    BackEndServerManager.instance.myInfo.disStartTime = callback.Rows()[0]["StartTime"]["S"].ToString();
                    BackEndServerManager.instance.myInfo.disChest = Convert.ToInt32(callback.Rows()[0]["DisChest"]["N"].ToString());
                    BackEndServerManager.instance.myInfo.haveChests = Convert.ToInt32(callback.Rows()[0]["HaveChests"]["N"].ToString());

                    BackEndServerManager.instance.myChestDataIndate = callback.Rows()[0]["inDate"]["S"].ToString();
                }
            }
        });
    }

    public void AddCard()
    {
        BackEndServerManager.instance.myInfo.cardKind.InsertRange(index: 0, collection: new List<int>() { 99, 99, 99 });
        BackEndServerManager.instance.myInfo.cardCount.InsertRange(index: 0, collection: new List<int>() { 99, 99, 99 });

        param = new Param();
        param.Add("CardKind", BackEndServerManager.instance.myInfo.cardKind);
        param.Add("CardCound", BackEndServerManager.instance.myInfo.cardCount);
        SendQueue.Enqueue(Backend.GameInfo.Insert, "CardData", param, callback =>
        {
            if (callback.IsSuccess())
                Debug.Log("상점 정보 추가성공");
        });
    }

    public void SaveCard()
    {
        Debug.Log("상점 정보 세이브");

        param = new Param();
        param.Add("CardKind", BackEndServerManager.instance.myInfo.cardKind);
        param.Add("CardCound", BackEndServerManager.instance.myInfo.cardCount);
        Backend.GameInfo.Update("CardData", BackEndServerManager.instance.myCardDataIndate, param);
    }

    public void LoadCard()
    {
        SendQueue.Enqueue(Backend.GameInfo.GetPrivateContents, "CardData", 2, callback =>
        {
            if (callback.IsSuccess())
            {
                if (callback.Rows().Count == 0)
                {
                    Debug.Log("로드 정보 실패");
                    AddCard();
                }
                else
                {
                    Debug.Log("상점 로드 성공");
                    for (int i = 0; i < callback.Rows()[0]["CardKind"]["L"].Count; i++)
                    {
                        BackEndServerManager.instance.myInfo.cardKind.Add(Convert.ToInt32(callback.Rows()[0]["CardKind"]["L"][i]["N"].ToString()));
                    }
                    for (int i = 0; i < callback.Rows()[0]["CardCound"]["L"].Count; i++)
                    {
                        BackEndServerManager.instance.myInfo.cardCount.Add(Convert.ToInt32(callback.Rows()[0]["CardCound"]["L"][i]["N"].ToString()));
                    }
                    BackEndServerManager.instance.myCardDataIndate = callback.Rows()[0]["inDate"]["S"].ToString();
                }
            }
        });
    }

    public void SetStats()
    {
        Debug.Log("현재 캐릭터 : " + BackEndServerManager.instance.myInfo.nowCharacter);

        BackEndServerManager.instance.myInfo.CurHp = BackEndServerManager.instance.myInfo.MaxHp = BackEndServerManager.instance.myInfo.pMaxHp[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.Armor = BackEndServerManager.instance.myInfo.pArmor[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.Stamina = BackEndServerManager.instance.myInfo.MaxStamina = 100;
        BackEndServerManager.instance.myInfo.ReductionStamina = BackEndServerManager.instance.myInfo.pReductionStamina[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.WeakAttackDamage = BackEndServerManager.instance.myInfo.pWeakAttackDamage[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.WeakAttackStun = BackEndServerManager.instance.myInfo.pWeakAttackStun[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.WeakAttackPenetration = BackEndServerManager.instance.myInfo.pWeakAttackPenetration[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.StrongAttackDamage = BackEndServerManager.instance.myInfo.pStrongAttackDamage[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.StrongAttackStun = BackEndServerManager.instance.myInfo.pStrongAttackStun[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.StrongAttackPenetration = BackEndServerManager.instance.myInfo.pStrongAttackPenetration[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.DefeneseReceivingDamage = BackEndServerManager.instance.myInfo.pDefenseReceivingDamage[BackEndServerManager.instance.myInfo.nowCharacter];
        BackEndServerManager.instance.myInfo.DefeneseReductionStun = BackEndServerManager.instance.myInfo.pDefenseReductionStun[BackEndServerManager.instance.myInfo.nowCharacter];
    }
}