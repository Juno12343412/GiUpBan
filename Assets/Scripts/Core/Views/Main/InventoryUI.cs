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
    public Image characterImg = null;
    public Image levelBaseImg = null, levelArrowImg = null;
    public Image levelUpBaseImg = null, levelUpArrowImg = null;
    public Text characterNameText = null, characterCountText = null;
    public int upgradePrice = 100, upgradeNeedCard = 10;

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
    [SerializeField] private Text characterNameText = null, characterCountText = null;
    [SerializeField] private Text characterPriceText = null;

    void Init()
    {
        int index = 0;
        foreach (var card in cards)
        {
            card.characterImg = transform.GetChild(0).GetComponent<Image>();
            card.levelBaseImg = transform.GetChild(1).GetComponent<Image>();
            card.levelArrowImg = card.levelBaseImg.transform.GetChild(0).GetComponent<Image>();
            card.levelUpBaseImg = transform.GetChild(2).GetComponent<Image>();
            card.levelUpArrowImg = card.levelBaseImg.transform.GetChild(0).GetComponent<Image>();
            card.characterNameText = transform.GetChild(3).GetComponent<Text>();
            card.characterCountText = transform.GetChild(4).GetComponent<Text>();

            if ((int)card.kind == BackEndServerManager.instance.myInfo.haveCharacters[index])
                card.isHave = true;

            index++;
        }
    }

    public void ShowInventory()
    {

    }

    public void UpgradeCard(int index)
    {

    }

    public void OnUpgrade()
    {
        UpgradeCard(curSelectCard);
    }

    public void OpenUpgradeUI(int index)
    {
        cardUpgrade.SetActive(true);
    }

    public void CloseUpgradeUI()
    {
        cardUpgrade.SetActive(false);
    }

    // 모든 카드 관련 이벤트가 발생했을 때 호출하는 함수
    public void SetInventory()
    {

    }
}
