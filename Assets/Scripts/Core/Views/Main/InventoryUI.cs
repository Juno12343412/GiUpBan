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
            BackEndServerManager.instance.myInfo.levelExp[value] -= cards[curSelectCard].upgradeNeedCard;
            if (BackEndServerManager.instance.myInfo.levelExp[value] == 0)
                BackEndServerManager.instance.myInfo.levelExp[value] = 1;
            BackEndServerManager.instance.myInfo.charactersLevel[value]++;
            SetInventory();
            UpdateUpgradeUI();
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

            if (card.isHave)
            {
                var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == (int)card.kind);

                Debug.Log(value);

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
    }
}