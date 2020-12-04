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
        BackEndMatchManager.instance.GetMyMatchRecord((int)BackEndMatchManager.instance.nowMatchType, null);

        foreach (var user in matchGameResult.m_winners)
        {
            winnerText.text = BackEndMatchManager.instance.GetNickNameBySessionId(user) + " (" + BackEndMatchManager.instance.GetMMRBySessionId(user) + ")";
            if (BackEndMatchManager.instance.IsMySessionId(user))
            {
                BackEndServerManager.instance.myInfo.mmr = BackEndMatchManager.instance.GetMMRBySessionId(user);
                Debug.Log("MMR : " + BackEndMatchManager.instance.GetMMRBySessionId(user));
            }
        }

        foreach (var user in matchGameResult.m_losers)
        {
            loserText.text = BackEndMatchManager.instance.GetNickNameBySessionId(user) + " (" + BackEndMatchManager.instance.GetMMRBySessionId(user) + ")";
            if (BackEndMatchManager.instance.IsMySessionId(user))
            {
                BackEndServerManager.instance.myInfo.mmr = BackEndMatchManager.instance.GetMMRBySessionId(user);
                Debug.Log("MMR : " + BackEndMatchManager.instance.GetMMRBySessionId(user));
            }
        }
        PlayerStats.instance.SaveMMR();
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
