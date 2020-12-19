using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.View;

// InGame
public partial class GameUI : BaseScreen<GameUI>
{
    public GameObject baseObj = null;
    public GameObject fadeObj = null;
    
    [SerializeField] private Text timerText = null;
    [SerializeField] private Text matchText = null;

    void Start()
    {
        HideScreen();
    }

    public IEnumerator gameTimeCheck(int time = 180)
    {
        int curTime = (time * 60) - 20;
        int curSec = 60;

        while (curTime > 0)
        {
            if (curSec == 0 && curTime / 60 != 0) curSec = 60;

            curTime -= curSec -= 1;
            if (curSec > 9)
                timerText.text = curTime / 60 + ":" + curSec;
            else
                timerText.text = curTime / 60 + ":0" + curSec;

            yield return new WaitForSecondsRealtime(1f);
        }
        timerText.text = "종료";
        WorldPackage.instance.TimeOutWinnerSetting();
    }
}