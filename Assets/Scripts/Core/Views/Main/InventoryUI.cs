using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.View;

[System.Serializable]
public class Card
{
    public CharacterKind kind = CharacterKind.NONE;
    public GameObject obj = null;  // 아이템 오브젝트
    [HideInInspector] public Image characterImg = null;
    [HideInInspector] public Image levelBaseImg = null;
    [HideInInspector] public Image levelUpBaseImg = null;
    [HideInInspector] public Text characterNameText = null, characterLevelText = null, characterCountText = null;
    [HideInInspector] public Image selectImg = null;
    [HideInInspector] public int upgradePrice = 100, upgradeNeedCard = 10;

    public bool isHave = false;
}

// 인벤토리
public partial class MainUI : BaseScreen<MainUI>
{
    [Header("Card")]
    [SerializeField] private Card[] cards = null;

    // 현재 고른 아이템
    int curSelectCard = 0;

    [Header("Upgrade")]
    [SerializeField] private Image upgradeCharacterImg = null;
    [SerializeField] private Image levelBaseImg = null;
    [SerializeField] private Image levelUpBaseImg = null;
    [SerializeField] private Text characterNameText = null, characterLevelText = null, characterGradeText = null, characterCountText = null;
    [SerializeField] private Text characterExplainText = null;
    [SerializeField] private Text characterHPText = null, characterAttackText = null, characterArmorText = null, characterAttackSpeedText = null;
    [SerializeField] private Text characterPriceText = null;
    [SerializeField] private GameObject[] charactersView = null;

    // 1.
    void InventoryInit()
    {
        foreach (var card in cards)
        {
            card.characterImg = card.obj.transform.GetChild(0).GetComponent<Image>();
            card.levelBaseImg = card.obj.transform.GetChild(4).GetComponent<Image>();
            card.levelUpBaseImg = card.obj.transform.GetChild(5).GetComponent<Image>();
            card.characterNameText = card.obj.transform.GetChild(1).GetComponent<Text>();
            card.characterLevelText = card.obj.transform.GetChild(2).GetComponent<Text>();
            card.characterCountText = card.levelBaseImg.gameObject.transform.GetChild(0).GetComponent<Text>();
            card.selectImg = card.obj.transform.GetChild(7).GetComponent<Image>();
        }
        SetInventory();

        foreach (var characters in charactersView)
            characters.SetActive(false);

        charactersView[BackEndServerManager.instance.myInfo.nowCharacter].SetActive(true);
    }

    // 5.
    public void UpdateUpgradeUI()
    {
        if (cards[curSelectCard].isHave)
        {
            var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == (int)cards[curSelectCard].kind);

            int level = BackEndServerManager.instance.myInfo.charactersLevel[value];

            if (level == 1)
            {
                characterHPText.text = BackEndServerManager.instance.myInfo.pMaxHp[curSelectCard].ToString();
                characterAttackText.text = BackEndServerManager.instance.myInfo.pWeakAttackDamage[curSelectCard] + "-" + BackEndServerManager.instance.myInfo.pStrongAttackDamage[curSelectCard];
                characterArmorText.text = BackEndServerManager.instance.myInfo.pArmor[curSelectCard].ToString();
            }
            else
            {
                characterHPText.text = (BackEndServerManager.instance.myInfo.pMaxHp[curSelectCard] + BackEndServerManager.instance.myInfo.pMaxHp[curSelectCard] * (int)(level * 0.6)).ToString();
                characterAttackText.text = BackEndServerManager.instance.myInfo.pWeakAttackDamage[curSelectCard] + BackEndServerManager.instance.myInfo.pWeakAttackDamage[curSelectCard] * (int)(level * 0.6) + "-" + (BackEndServerManager.instance.myInfo.pStrongAttackDamage[curSelectCard] + BackEndServerManager.instance.myInfo.pStrongAttackDamage[curSelectCard] * (int)(level * 0.6));
                characterArmorText.text = (BackEndServerManager.instance.myInfo.pArmor[curSelectCard] + BackEndServerManager.instance.myInfo.pArmor[curSelectCard] * (int)(level * 0.6)).ToString();
            }
            characterCountText.text = BackEndServerManager.instance.myInfo.levelExp[value] + "/" + cards[curSelectCard].upgradeNeedCard;
            upgradeCharacterImg.sprite = characterImgs[curSelectCard];
            characterNameText.text = ((CharacterKind)curSelectCard).ToString();
            characterLevelText.text = "Lv." + level;
            characterPriceText.text = cards[curSelectCard].upgradePrice.ToString();

            switch (curSelectCard)
            {
                case 0: // 나이트
                    characterExplainText.text = "교양있는 근접 전사입니다.\n그렇기 때문에 몸에 많은 상처가 있죠.\n하지만 마음 속은 따뜻한 사람이라구요.";
                    characterAttackSpeedText.text = "보통";
                    characterGradeText.text = "일반";
                    break;
                case 1: // 벤전스
                    characterExplainText.text = "터프한 광전사입니다. \n몹집이 크고 느린데다 성깔도 사납기 그지없습니다 !";;
                    characterAttackSpeedText.text = "느림";
                    characterGradeText.text = "일반";
                    break;
                case 2: // 듀얼
                    characterExplainText.text = "브릿지 내에서 가장 빠른 전사입니다.\n그의 공격은 눈으로 보기 힘들죠 !";
                    characterAttackSpeedText.text = "빠름";
                    characterGradeText.text = "일반";
                    break;
                case 3: // 도끼
                    characterExplainText.text = "도끼를 사용하는 무서운 전사입니다.\n가끔씩 나무꾼과 토목을 하러 간다는 소문이...";
                    characterAttackSpeedText.text = "느림";
                    characterGradeText.text = "일반";
                    break;
                case 4: // 스탭
                    characterExplainText.text = "창을 사용하는 전사입니다. \n그의 창으로 어떤 것이든 뚫어버릴 수 있죠";
                    characterAttackSpeedText.text = "빠름";
                    characterGradeText.text = "일반";
                    break;
                case 5: // 시프
                    characterExplainText.text = "단검을 사용하는 전사입니다. 주머니를 조심하세요";
                    characterAttackSpeedText.text = "빠름";
                    characterGradeText.text = "일반";
                    break;
                case 6: // 피오라
                    characterExplainText.text = "레이피어를 사용하는 전사입니다. \n빠르고 정교한 움직임으로 적을 처리합니다.";
                    characterAttackSpeedText.text = "빠름";
                    characterGradeText.text = "일반";
                    break;
                case 7: // 사이드
                    characterExplainText.text = "낫을 사용하는 전사 입니다. \n그의 무시무시한 낫으로 마당 잡초들을 모조리 처치했다고 하네요";
                    characterAttackSpeedText.text = "느림";
                    characterGradeText.text = "일반";
                    break;
                case 8: // 스미스
                    characterExplainText.text = "망치를 사용하는 전사입니다. \n그의 단단한 갑옷은 쉽게 뚫지 못할겁니다.";
                    characterAttackSpeedText.text = "느림";
                    characterGradeText.text = "일반";
                    break;
                case 9: // 라운드
                    characterExplainText.text = "양손검을 사용하는 전사입니다.\n 그의 큰 검으로 적을 정리합니다.";
                    characterAttackSpeedText.text = "보통";
                    characterGradeText.text = "일반";
                    break;
                case 10: // 듀크
                    characterExplainText.text = "검과 방패를 사용하는 전사입니다. \n그의 방패는 무엇이든지 막을 수 있죠";
                    characterAttackSpeedText.text = "보통";
                    characterGradeText.text = "일반";
                    break;
                case 11: // 빈센트
                    characterExplainText.text = "한손검을 사용하는 광전사입니다. \n무거운 갑옷을 입고 큰 동작을 하다니 체력이 남아나질 않겠군요";
                    characterAttackSpeedText.text = "느림";
                    characterGradeText.text = "일반";
                    break;
                case 12: // 플레타
                    characterExplainText.text = "쌍검을 사용하는 전사입니다. \n그의 쉽틈 없는 공격에선 벗어나기 힘들겁니다.";
                    characterAttackSpeedText.text = "매우 빠름";
                    characterGradeText.text = "일반";
                    break;
                case 13: // 더스틴
                    characterExplainText.text = "도끼를 사용하는 용맹한 전사입니다.\n 그의 도끼는 방어구의 수명을 갉아 먹습니다.";
                    characterAttackSpeedText.text = "보통";
                    characterGradeText.text = "일반";
                    break;
                case 14: // 루이스
                    characterExplainText.text = "창을 사용하는 전사입니다.\n 거대한 창으로 공격합니다.";
                    characterAttackSpeedText.text = "보통";
                    characterGradeText.text = "일반";
                    break;
                case 15: // 월리
                    characterExplainText.text = "단검을 사용하는 전사입니다. \n그의 얍삽한 공격은 누구도 대응하기 쉽지 않을겁니다.";
                    characterAttackSpeedText.text = "빠름";
                    characterGradeText.text = "일반";
                    break;
                case 16: // 아일린
                    characterExplainText.text = "레이피어를 사용하는 전사입니다. \n찌르는 공격을 매우 좋아한다네요";
                    characterAttackSpeedText.text = "빠름";
                    characterGradeText.text = "일반";
                    break;
                case 17: // 체이스
                    characterExplainText.text = "낫을 사용하는 전사입니다. \n그의 낫은 적들을 절단해버립니다.";
                    characterAttackSpeedText.text = "느림";
                    characterGradeText.text = "일반";
                    break;
                case 18: // 랄프
                    characterExplainText.text = "망치를 사용하는 전사입니다. \n그와 싸우다 보면 갑옷이 강화될지도 모르겠군요";
                    characterAttackSpeedText.text = "느림";
                    characterGradeText.text = "일반";
                    break;
                case 19: // 알베토
                    characterExplainText.text = "양손검을 사용하는 전사입니다. \n큰 검을 들고 빠른 속도를 공격하다니 저건 사기입니다.";
                    characterAttackSpeedText.text = "보통";
                    characterGradeText.text = "일반";
                    break;
                default:
                    break;
            }

            if (BackEndServerManager.instance.myInfo.levelExp[value] >= cards[curSelectCard].upgradeNeedCard)
            {
                levelUpBaseImg.gameObject.SetActive(true);
            }
            else
            {
                levelUpBaseImg.gameObject.SetActive(false);
                levelBaseImg.fillAmount = (float)BackEndServerManager.instance.myInfo.levelExp[value] / cards[curSelectCard].upgradeNeedCard;

            }
        }
    }

    // 4.
    public void UpgradeCard(int index)
    {
        var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == (int)cards[curSelectCard].kind);

        if (BackEndServerManager.instance.myInfo.levelExp[value] >= cards[curSelectCard].upgradeNeedCard)
        {
            if (BackEndServerManager.instance.myInfo.gold >= cards[curSelectCard].upgradePrice)
            {
                BackEndServerManager.instance.myInfo.gold -= cards[curSelectCard].upgradePrice;
                BackEndServerManager.instance.myInfo.levelExp[value] -= cards[curSelectCard].upgradeNeedCard;
                
                if (BackEndServerManager.instance.myInfo.levelExp[value] == 0)
                    BackEndServerManager.instance.myInfo.levelExp[value] = 1;
                BackEndServerManager.instance.myInfo.charactersLevel[value]++;
                
                SetInventory();
                UpdateUpgradeUI();
            }
            else
            {
                StartCoroutine(OnShowBroadCast("클로버 부족"));
            }
        }
        else
        {
            StartCoroutine(OnShowBroadCast("카드 부족"));
        }
    }

    // 3.
    public void OnUpgrade()
    {
        UpgradeCard(curSelectCard);
    }

    // 2.
    public void OpenUpgradeUI(int index)
    {
        curSelectCard = index;
        if (cards[curSelectCard].isHave)
        {
            UpdateUpgradeUI();
            cardUpgrade.SetActive(true);
        }
    }

    public void CloseUpgradeUI()
    {
        cardUpgrade.SetActive(false);
    }

    // 모든 카드 관련 이벤트가 발생했을 때 호출하는 함수
    public void SetInventory()
    {
        Debug.Log("인벤토리 설정");

        int index = 0;
        foreach (var card in cards)
        {
            if (BackEndServerManager.instance.myInfo.haveCharacters.Contains(index))
                card.isHave = true;

            card.kind = (CharacterKind)index;

            card.characterImg.sprite = characterImgs[index];
            card.characterNameText.text = card.kind.ToString();

            if (index == BackEndServerManager.instance.myInfo.nowCharacter)
                card.selectImg.gameObject.SetActive(true);
            else
                card.selectImg.gameObject.SetActive(false);

            if (card.isHave)
            {
                var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == (int)card.kind);

                card.upgradePrice = BackEndServerManager.instance.myInfo.charactersLevel[value] * 10;
                card.upgradeNeedCard = BackEndServerManager.instance.myInfo.charactersLevel[value] * 5;

                card.characterImg.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                card.levelBaseImg.color = new Color(63f / 255f, 123f / 255f, 161f / 255f, 255f / 255f);
                card.characterLevelText.text = "Lv ." + BackEndServerManager.instance.myInfo.charactersLevel[value];

                card.characterCountText.text = BackEndServerManager.instance.myInfo.levelExp[value] + "/" + card.upgradeNeedCard;

                if (BackEndServerManager.instance.myInfo.levelExp[value] >= card.upgradeNeedCard)
                {
                    card.levelUpBaseImg.gameObject.SetActive(true);
                    card.levelBaseImg.gameObject.SetActive(false);
                }
                else
                {
                    card.levelBaseImg.gameObject.SetActive(true);
                    card.levelUpBaseImg.gameObject.SetActive(false);

                    card.levelBaseImg.fillAmount = (float)BackEndServerManager.instance.myInfo.levelExp[value] / card.upgradeNeedCard;
                }
            }
            else
            {
                card.characterCountText.text = "";

                card.characterImg.color = new Color(160f / 255f, 160f / 255f, 160f / 255f, 160f / 255f);
                card.levelBaseImg.color = new Color(160f / 255f, 160f / 255f, 160f / 255f, 160f / 255f);

                card.levelBaseImg.gameObject.SetActive(true);
                card.levelUpBaseImg.gameObject.SetActive(false);
                card.levelBaseImg.fillAmount = 0f;
            }
            index++;
        }
    }

    public void CharacterChange()
    {
        BackEndServerManager.instance.myInfo.nowCharacter = curSelectCard;
        cardUpgrade.SetActive(false);

        foreach (var characters in charactersView)
            characters.SetActive(false);

        charactersView[curSelectCard].SetActive(true);

        PlayerStats.instance.SetStats();
        SetNickName();
        SetInventory();
    }
}