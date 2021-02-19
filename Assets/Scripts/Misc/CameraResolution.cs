using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    private static Camera cam;

    public static void SetCamera()
    {
        Debug.Log("카메라 셋팅");

        cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        Rect rect = cam.rect;
        float scaleH = ((float)Screen.width / Screen.height) / ((float)9 / 16);
        float scaleW = 1f / scaleH;
        if (scaleH < 1)
        {
            rect.height = scaleH;

            rect.y = (1f - scaleH) / 2f;
        }
        else
        {
            rect.width = scaleW;
            rect.x = (1f - scaleW) / 2f;
        }
        cam.rect = rect;
    }

}
