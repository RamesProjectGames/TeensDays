using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBoxExpand : MonoBehaviour
{
    public bool isExpanded;
    public Button[] otherButtons;
    public Button toolboxButton;

    private void Start()
    {
        for (int i = 0; i < otherButtons.Length; i++)
        {
            otherButtons[i].transform.position = toolboxButton.transform.position;
            otherButtons[i].gameObject.SetActive(false);
        }
    }
    public void ToogleExpand()
    {
        isExpanded = !isExpanded;
        for (int i = 0; i < otherButtons.Length; i++)
        {
            var btn = otherButtons[i];
            if (isExpanded)
            {
                btn.gameObject.SetActive(true);
                Vector3 targetPos = toolboxButton.transform.position + new Vector3((i + 1) * 120, 0, 0);
                btn.transform.position = toolboxButton.transform.position;
                LeanTween.move(btn.gameObject, targetPos, 0.3f).setDelay(0.05f * i);
            }
            else
            {
                // Kembali ke posisi toolbox lalu sembunyikan
                LeanTween.move(btn.gameObject, toolboxButton.transform.position, 0.3f)
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


