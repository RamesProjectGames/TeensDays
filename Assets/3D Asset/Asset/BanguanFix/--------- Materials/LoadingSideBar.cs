using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSideBar : MonoBehaviour
{
    public RawImage rawImage;
    public float scrollSpeed = 0.5f;

    void Update()
    {
        Rect uvRect = rawImage.uvRect;
        uvRect.x += scrollSpeed * Time.deltaTime;
        rawImage.uvRect = uvRect;
    }

}
