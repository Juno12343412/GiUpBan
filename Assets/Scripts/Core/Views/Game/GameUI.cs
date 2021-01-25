using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.View;
using Manager.Sound;

// InGame
public partial class GameUI : BaseScreen<GameUI>
{
    public GameObject baseObj = null;
    public GameObject fadeObj = null;

    [SerializeField] private Text timerText = null;
    [SerializeField] private Text matchText = null;
    [SerializeField] private Text startText = null;

    void Start()
    {
        SoundPlayer.instance.PlaySound("BattleStart");
        HideScreen();
    }

    public IEnumerator gameTimeCheck(int time = 180)
    {
        int curTime = time - 1;
        int curSec = 30;

        while (curTime >= 0)
        {
            Debug.Log("curTime : " + curTime);

            if (curSec <= 0)
            {
                curTime -= 1;
                curSec = 60;
            }

            curSec -= 1;
            if (curSec > 9)
                timerText.text = curTime + ":" + curSec;
            else
                timerText.text = curTime + ":0" + curSec;

            yield return new WaitForSecondsRealtime(1f);
        }
        timerText.text = "종료";
        WorldPackage.instance.TimeOutWinnerSetting();
    }

    public void SetStartText(string text = "")
    {
        startText.text = text;
    }
}