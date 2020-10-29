using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager.View;
using Manager.Scene;

public class MainUI : BaseScreen<MainUI>
{
    public override void ShowScreen()
    {
        base.ShowScreen();
    }

    public override void HideScreen()
    {
        base.HideScreen();
    }

    public void GamePvpStart()
    {
        Loader.Load(Scene.Game);
    }
}
