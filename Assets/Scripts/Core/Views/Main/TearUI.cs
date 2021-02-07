using UnityEngine;
using UnityEngine.UI;
using Manager.View;

// Tear
public partial class MainUI : BaseScreen<MainUI>
{
    [Header("Tear")]
    [SerializeField] private GameObject tearInfoObject = null;
    [SerializeField] private Image tearInfoImage = null;
    [SerializeField] private Text  tearInfoText = null;
    [SerializeField] private Text  tearInfoExplainText = null;

    [SerializeField] private GameObject tearUpObject = null;
    [SerializeField] private GameObject tearLightObject = null;

    public void OpenTearInfo()
    {
        tearInfoObject.SetActive(true);
        SetTearInfo();
    }

    public void CloseTearInfo()
    {
        tearInfoObject.SetActive(false);
    }

    public void ShowTearUp()
    {
        tearUpObject.SetActive(true);
        tearLightObject.SetActive(true);
        Invoke("HideTearUp", 1f);
        OpenTearInfo();
    }

    public void HideTearUp()
    {
        tearUpObject.SetActive(false);
        tearLightObject.SetActive(false);
    }

    void SetTearInfo()
    {
        var tear = GetPointToRank(BackEndServerManager.instance.myInfo.point);

        tearInfoImage.sprite = tearImages[(int)tear];
        tearInfoText.text = tear.ToString();

        switch (tear)
        {
            case Tear.브론즈:
                tearInfoExplainText.text = "당신은 막 전장에 입장한 전사입니다\n당신의 강함을 입증해보세요 !";
                break;
            case Tear.실버:
                tearInfoExplainText.text = "막 인정받기 시작한 전사입니다\n계속 이렇게만 성장해주세요 !";
                break;
            case Tear.골드:
                tearInfoExplainText.text = "이제 고수 반열에 오른 전사입니다\n왠만한 전사들은 상대가 안되죠";
                break;
            case Tear.다이아몬드:
                tearInfoExplainText.text = "전국에서 인정받는 전사입니다\n모두들 당신에게 존경의 표시를 하고있네요 !";
                break;
            case Tear.마스터:
                tearInfoExplainText.text = "세계에 몇없는 강한 전사입니다\n당신의 강함은 모르는 사람이 없죠 !";
                break;
            default:
                break;
        }
    }
}
