using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager.View;
using UnityEngine.UI;

[System.Serializable]
public class Item
{
    public GameObject obj = null;  // 아이템 오브젝트
    public GameObject hideObj = null; // 아이템 품절 시 가리기용 오브젝트
    public Image itemImage = null; // 아이템 이미지
    public Text itemNameText = null, itemCountText = null; // 아이템 이름과 개수
    public Text itemPriceText = null;
    public int itemPrice = 100; // 아이템 가격
    public int itemKind = 0; // 아이템 종류
    public int itemCount = 1, itemMaxCount = 1; // 아이템 현재,최대 개수
}

// 상점 관련
public partial class MainUI : BaseScreen<MainUI>
{
    // Product
    [Header("Product")]
    // 캐릭터 스프라이트
    [SerializeField] private Sprite[] characterImgs = null;

    // 상점에 배치되어 있는 아이템들
    [SerializeField] private Item[] items = null;

    // 현재 고른 아이템
    int curSelectItem = 0;
    // Product

    // Purchase
    [Header("Purchase")]
    [SerializeField] private Image purchaseCharacterImg = null;
    [SerializeField] private Text itemNameText = null, itemCountText = null;
    [SerializeField] private Text itemPriceText = null;
    [SerializeField] private GameObject hideObj = null;
    // Purchase

    void ShopInit()
    {
        foreach (var item in items)
        {
            item.itemImage = item.obj.transform.GetChild(0).GetComponent<Image>();
            item.hideObj = item.obj.transform.GetChild(1).gameObject;
            item.itemNameText = item.obj.transform.GetChild(2).GetComponent<Text>();
            item.itemCountText = item.obj.transform.GetChild(3).GetComponent<Text>();
            item.itemPriceText = item.obj.transform.GetChild(4).GetComponent<Text>();
        }
    }

    // 상점 열기
    // 현재 카드 구매 가능여부 보여주기
    public void ShowShop()
    {
        foreach(var item in items)
        {
            if (item.itemCount <= 0)
            {
                hideObj.SetActive(true);

                item.hideObj.SetActive(true);
                item.itemCountText.text = itemCountText.text = "판매 종료";
                item.itemPriceText.text = itemPriceText.text = "";
            }
            else
            {
                item.hideObj.SetActive(false);
                item.itemCountText.text = item.itemCount + "/" + item.itemMaxCount;
                item.itemPriceText.text = item.itemPrice.ToString();
            }
            item.itemNameText.text = ((CharacterKind)item.itemKind).ToString();
        }
    }

    // 카드 구매
    // 카드가 다 소모되면 회색으로 바꿔주고 카드 줄여주기
    public void PurchaseItem(int index)
    {
        //if (BackEndServerManager.instance.myInfo.gold >= items[index].itemPrice)
        //{
        //    BackEndServerManager.instance.myInfo.gold -= items[index].itemPrice;
        //    BackEndServerManager.instance.myInfo.levelExp[items[index].itemKind] += items[index].itemCount;
        //    items[index].itemCount = 0;
        //    ShowShop();
        //}
        
        if (BackEndServerManager.instance.myInfo.haveCharacters.Contains(items[curSelectItem].itemKind))
        {
            Debug.Log("캐릭터 있음");

            BackEndServerManager.instance.myInfo.gold -= items[index].itemPrice;
            BackEndServerManager.instance.myInfo.levelExp[items[index].itemKind] += items[index].itemCount;
            items[index].itemCount = 0;
            ShowShop();
        }
        else
        {
            Debug.Log("캐릭터 없음 : " + items[index].itemKind);

            BackEndServerManager.instance.myInfo.haveCharacters.Add(items[curSelectItem].itemKind);
            BackEndServerManager.instance.myInfo.charactersLevel.Add(1);
            BackEndServerManager.instance.myInfo.levelExp.Add(-1);
            BackEndServerManager.instance.myInfo.gold -= items[index].itemPrice;

            var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == items[curSelectItem].itemKind);
            BackEndServerManager.instance.myInfo.levelExp[value] += items[index].itemCount;
            items[index].itemCount = 0;
            ShowShop();
        }
        SetInventory();
    }

    // 상점 아이템 재설정
    public void SetShopItems()
    {
        hideObj.SetActive(false);

        foreach(var item in items)
        {
            // 이미지 설정하는 코드 추가 [...]
            item.itemKind = Random.Range(0, 3);

            item.itemMaxCount = Random.Range(1, 65);
            item.itemCount = item.itemMaxCount;

            item.itemPrice = item.itemMaxCount * 10;

            item.hideObj.SetActive(false);
            item.itemNameText.text = ((CharacterKind)item.itemKind).ToString();
            item.itemCountText.text = item.itemMaxCount + "x";
            item.itemPriceText.text = item.itemPrice.ToString() + "C";

            item.itemImage.sprite = characterImgs[0];
        }
    }

    public void OpenPurchaseUI(int index)
    {
        curSelectItem = index;
        if (items[curSelectItem].itemCount > 0)
        {
            hideObj.SetActive(false);
            purchaseCharacterImg.sprite = characterImgs[0];
            itemNameText.text = ((CharacterKind)items[index].itemKind).ToString();
            itemCountText.text = items[index].itemCount + "x";
            itemPriceText.text = items[index].itemPrice.ToString() + "C";

            cardPurchase.SetActive(true);
        }
    }

    public void ClosePurchaseUI()
    {
        cardPurchase.SetActive(false);
    }

    public void OnPurchaseItem()
    {
        PurchaseItem(curSelectItem);
    }
}

