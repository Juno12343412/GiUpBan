using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour
{

    [SerializeField] private int num = 0;
    [SerializeField] private GameObject UpgradeArrow;
    [SerializeField] private GameObject LevelBase;
    private bool have = false;

    private void Start()
    {
        LevelBase = transform.GetChild(0).gameObject;
        UpgradeArrow = LevelBase.transform.GetChild(0).gameObject;
    }

    void FixedUpdate()
    {
        for (int i = 0; i < BackEndServerManager.instance.myInfo.haveCharacters.Count; i++)
        {
            if (BackEndServerManager.instance.myInfo.haveCharacters[i] == num)
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
