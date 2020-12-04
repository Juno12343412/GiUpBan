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
    [HideInInspector] public Image levelBaseImg = null, levelArrowImg = null;
    [HideInInspector] public Image levelUpBaseImg = null, levelUpArrowImg = null;
    [HideInInspector] public Text characterNameText = null, characterCountText = null;
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
    [SerializeField] private Image levelUpBaseImg = null;
    [SerializeField] private Text characterNameText = null, characterLevelText = null, characterCountText = null;
    [SerializeField] private Text characterPriceText = null;

    // 1.
    void InventoryInit()
    {
        foreach (var card in cards)
        {
            card.characterImg = card.obj.transform.GetChild(0).GetComponent<Image>();
            card.levelBaseImg = card.obj.transform.GetChild(1).GetComponent<Image>();
            card.levelArrowImg = card.levelBaseImg.transform.GetChild(0).GetComponent<Image>();
            card.levelUpBaseImg = card.obj.transform.GetChild(2).GetComponent<Image>();
            card.levelUpArrowImg = card.levelBaseImg.transform.GetChild(0).GetComponent<Image>();
            card.characterNameText = card.obj.transform.GetChild(3).GetComponent<Text>();
            card.characterCountText = card.obj.transform.GetChild(4).GetComponent<Text>();
        }
        SetInventory();
    }

    // 5.
    public void UpdateUpgradeUI()
    {
        if (cards[curSelectCard].isHave)
        {
            var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == (int)cards[curSelectCard].kind);

            upgradeCharacterImg.sprite = characterImgs[0];
            characterNameText.text = ((CharacterKind)curSelectCard).ToString();
            characterCountText.text = BackEndServerManager.instance.myInfo.levelExp[value] + "/" + cards[curSelectCard].upgradeNeedCard;
            characterLevelText.text = "레벨 " + BackEndServerManager.instance.myInfo.charactersLevel[value];
            characterPriceText.text = cards[curSelectCard].upgradePrice + "C";

            if (BackEndServerManager.instance.myInfo.levelExp[value] >= cards[curSelectCard].upgradeNeedCard)
            {
                levelUpBaseImg.gameObject.SetActive(true);
            }
            else
            {
                levelUpBaseImg.gameObject.SetActive(false);
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

            card.characterImg.sprite = characterImgs[0];
            card.characterNameText.text = card.kind.ToString();

            if (card.isHave)
            {
                var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == (int)card.kind);

                Debug.Log(value);
                
                card.upgradePrice = BackEndServerManager.instance.myInfo.charactersLevel[value] * 10;
                card.upgradeNeedCard = BackEndServerManager.instance.myInfo.charactersLevel[value] * 5;

                card.characterImg.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                card.levelBaseImg.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                card.levelArrowImg.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);

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
                }
            }
            else
            {
                card.characterCountText.text = "";

                card.characterImg.color = new Color(160f / 255f, 160f / 255f, 160f / 255f, 160f / 255f);
                card.levelBaseImg.color = new Color(160f / 255f, 160f / 255f, 160f / 255f, 160f / 255f);
                card.levelArrowImg.color = new Color(160f / 255f, 160f / 255f, 160f / 255f, 160f / 255f);

                card.levelBaseImg.gameObject.SetActive(true);
                card.levelUpBaseImg.gameObject.SetActive(false);
            }
            index++;
        }
    }

    public void CharacterChange()
    {
        BackEndServerManager.instance.myInfo.nowCharacter = curSelectCard;
        cardUpgrade.SetActive(false);
    }
}
