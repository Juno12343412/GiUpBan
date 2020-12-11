using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Manager.View;
using BackEnd.Tcp;

// Result
public partial class GameUI : BaseScreen<GameUI>
{
    [SerializeField] private Text winnerText = null, loserText = null, drawText = null;

    public override void ShowScreen()
    {
        Debug.Log("ShowScreen");
        base.ShowScreen();
    }

    public override void HideScreen()
    {
        Debug.Log("HideScreen");
        base.HideScreen();
    }

    public void ShowResultBoard(MatchGameResult matchGameResult)
    {
        Debug.Log("Result Board : " + matchGameResult + "/ Match Type : " + (int)BackEndMatchManager.instance.nowMatchType);
        BackEndMatchManager.instance.GetMyMatchRecord(0, null);

        foreach (var user in matchGameResult.m_winners)
        {
            if (BackEndMatchManager.instance.IsMySessionId(user))
            {
                BackEndServerManager.instance.myInfo.mmr = BackEndMatchManager.instance.GetMMRBySessionId(user);
                Debug.Log("MMR : " + BackEndMatchManager.instance.GetMMRBySessionId(user));
            }
            winnerText.text = BackEndMatchManager.instance.GetNickNameBySessionId(user);
            drawText.gameObject.SetActive(false);
        }

        foreach (var user in matchGameResult.m_losers)
        {
            if (BackEndMatchManager.instance.IsMySessionId(user))
            {
                BackEndServerManager.instance.myInfo.mmr = BackEndMatchManager.instance.GetMMRBySessionId(user);
                Debug.Log("MMR : " + BackEndMatchManager.instance.GetMMRBySessionId(user));
            }
            loserText.text = BackEndMatchManager.instance.GetNickNameBySessionId(user);
            drawText.gameObject.SetActive(false);
        }

        PlayerStats.instance.SaveMMR();
        ShowScreen();
    }

    public void ShowDrawBoard()
    {
        loserText.gameObject.SetActive(false);
        winnerText.gameObject.SetActive(false);
        drawText.gameObject.SetActive(true);

        ShowScreen();
    }

    public void OnLeaveGameRoom()
    {
        Debug.Log("Game Result");

        if (GameManager.instance.gameState != GameManager.GameState.MatchLobby)
        {
            GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
        }
    }
}