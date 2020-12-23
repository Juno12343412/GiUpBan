using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    public Image guideImage;
    // Start is called before the first frame update
    void Start()
    {
        guideImage.color = new Color(0, 0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator CR_GameStart(float _Time)
    {
        yield return new WaitForSeconds(_Time);
        //씬 전환
    }

    public void GameStart(float _Time)
    {
        StartCoroutine(CR_GameStart(_Time));
        StartCoroutine(CR_ImageAlpha());
        
    }

    public IEnumerator CR_ImageAlpha()
    {
        while (guideImage.color.a < 255)
        {
            yield return new WaitForSeconds(0.01f);
            guideImage.color = new Color(0, 0, 0, guideImage.color.a + 0.01f);
        }
    }
}
