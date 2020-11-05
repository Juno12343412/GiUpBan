using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiControl : MonoBehaviour
{
    [SerializeField] private Text[] playerTexts = new Text[2] { null, null};
   

    // Start is called before the first frame update
    void Start()
    {
        var myName = BackEndServerManager.instance.myNickName;
        var enemyName = BackEndMatchManager.instance.GetNickNameBySessionId(WorldPackage.instance.otherPlayerIndex);

        playerTexts[0].text = myName;
        playerTexts[1].text = enemyName;
    }

    // Update is called once per frame
}
