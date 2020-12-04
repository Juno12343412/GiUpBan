using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.View;

public partial class MainUI : BaseScreen<MainUI>
{
    [Header("JaeHwa")]
    [SerializeField] private Text goldText = null;
    [SerializeField] private Text diamondText = null;

    public void SetGoldUI()
    {
        goldText.text = BackEndServerManager.instance.myInfo.gold.ToString();
        diamondText.text = BackEndServerManager.instance.myInfo.diamond.ToString();
    }
}
