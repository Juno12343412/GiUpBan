using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour
{
    [SerializeField] private int num = 0;
    [SerializeField] private GameObject UpgradeArrow;
    [SerializeField] private GameObject LevelBase;
    private bool have = false;

    void Start()
    {
        LevelBase = transform.GetChild(1).gameObject;
        UpgradeArrow = LevelBase.transform.GetChild(0).gameObject;  
    }

    void FixedUpdate()
    {
        foreach (var iter in BackEndServerManager.instance.myInfo.haveCharacters)
        {
            if (iter == num)
                have = true;
        }

        if(!have)
        {
            UpgradeArrow.GetComponent<Image>().color = new Color(160f/255f,160f/255f,160f/255f, 255f/255f);
            LevelBase.GetComponent<Image>().color = new Color(160f/255f,160f/255f,160f/255f, 255f/255f);
            GetComponent<Image>().color = new Color(160f/255f,160f/255f,160f/255f, 255f/255f);
            GetComponent<Button>().enabled = false;
        }
    }
}
