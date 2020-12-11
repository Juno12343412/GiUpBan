using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.View;

// InGame
public partial class GameUI : BaseScreen<GameUI>
{
    [SerializeField] private Text[] playerTexts = new Text[2] { null, null };
    [SerializeField] private Text timerText = null;

    void Start()
    {
        HideScreen();
    }

    public void UISetting()
    {
        var myName = BackEndServerManager.instance.myNickName;
        var enemyName = BackEndMatchManager.instance.GetNickNameBySessionId(WorldPackage.instance.otherPlayerIndex);

        playerTexts[0].text = myName;
        playerTexts[1].text = enemyName;
    }

    public IEnumerator gameTimeCheck(int time = 180)
    {
        int curTime = (time * 60) - 20;

        while (curTime > 0)
        {
            curTime -= 1;
            timerText.text = curTime.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }
        timerText.text = "-";
        WorldPackage.instance.TimeOutWinnerSetting();
    }
}