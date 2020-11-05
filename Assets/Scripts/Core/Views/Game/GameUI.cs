using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Manager.View;
using BackEnd.Tcp;

public class GameUI : BaseScreen<GameUI>
{
    [SerializeField] private Text winnerText = null, loserText = null;

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

    void Start()
    {
        HideScreen();
    }


    public void ShowResultBoard(MatchGameResult matchGameResult)
    {
        Debug.Log("Result Board : " + matchGameResult != null);
        foreach (var user in matchGameResult.m_winners)
        {
            winnerText.text = BackEndMatchManager.instance.GetNickNameBySessionId(user);
        }

        foreach (var user in matchGameResult.m_losers)
        {
            loserText.text = BackEndMatchManager.instance.GetNickNameBySessionId(user);
        }
        ShowScreen();
    }

    public void OnLeaveGameRoom()
    {
        Debug.Log("Game Result");
        //BackEndMatchManager.instance.LeaveInGameRoom();

        if (GameManager.instance.gameState != GameManager.GameState.MatchLobby)
        {
            GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
        }
    }
}
