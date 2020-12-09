using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.View;

// InGame
public partial class GameUI : BaseScreen<GameUI>
{
    [SerializeField] private Text[] playerTexts = new Text[2] { null, null};
    [SerializeField] private Text timerText = null;

    void Start()
    {
        HideScreen();

        var myName = BackEndServerManager.instance.myNickName;
        var enemyName = WorldPackage.instance.players[WorldPackage.instance.otherPlayerIndex].nickName;

        playerTexts[0].text = myName;
        playerTexts[1].text = enemyName;

        StartCoroutine(gameTimeCheck(BackEndMatchManager.instance.matchInfos[0].matchMinute));
    }

    IEnumerator gameTimeCheck(int time = 180)
    {
        int curTime = 0;
        
        while (curTime <= time)
        {
            curTime += 1;
            timerText.text = curTime.ToString();
            yield return new WaitForSeconds(1f);
        }
        timerText.text = curTime.ToString();
    }
}
