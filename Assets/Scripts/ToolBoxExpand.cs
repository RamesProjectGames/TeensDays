using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBoxExpand : MonoBehaviour
{
    public bool isExpanded;
    public Button[] otherButtons;
    public Button toolboxButton;

    RectTransform toolboxRect;
    RectTransform[] buttonRects;

    private void Start()
    {
        toolboxRect = toolboxButton.GetComponent<RectTransform>();

        buttonRects = new RectTransform[otherButtons.Length];

        for (int i = 0; i < otherButtons.Length; i++)
        {
            buttonRects[i] = otherButtons[i].GetComponent<RectTransform>();
            buttonRects[i].anchoredPosition = toolboxRect.anchoredPosition;
            otherButtons[i].gameObject.SetActive(false);
        }
    }
    public void ToogleExpand()
    {
        isExpanded = !isExpanded;

        for (int i = 0; i < otherButtons.Length; i++)
        {
            var btn = otherButtons[i];
            var rect = buttonRects[i]; // ambil rect

            if (isExpanded)
            {
                btn.gameObject.SetActive(true);

                Vector2 targetPos = toolboxRect.anchoredPosition + new Vector2(+300 * (i + 1), 0);

                rect.anchoredPosition = toolboxRect.anchoredPosition;

                LeanTween.move(rect, targetPos, 0.3f).setDelay(0.05f * i);
            }
            else
            {
                LeanTween.move(rect, toolboxRect.anchoredPosition, 0.3f)
                         .setDelay(0.05f * i)
                         .setOnComplete(() => btn.gameObject.SetActive(false));
            }
        }
    }

    public void EnterShop()
    {
        ShopManager.instance.OnTabClicked(0);
    }

    public void EnterGacha()
    {
        ShopManager.instance.OnTabClicked(1);
    }
}


