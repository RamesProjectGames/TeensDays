using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StripedLoadingBar1 : MonoBehaviour
{
    public RawImage stripeBackground;
    public float scrollSpeed = 0.5f;

    void Update()
    {
        if (stripeBackground != null)
        {
            Rect uv = stripeBackground.uvRect;
            uv.x += scrollSpeed * Time.deltaTime;
            stripeBackground.uvRect = uv;
        }
    }
}
