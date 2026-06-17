using TMPro;
using UnityEngine;

public class IAPPurchaseDetails : MonoBehaviour
{
    [Header("Purchase Confirmed Info")]
    public TextMeshProUGUI purchaseInfoText;
    public TextMeshProUGUI purchaseAmountInfoText;
    public GameObject diamondIcon;
    public GameObject purchaseInfoPanel;

    public void ShowPurchaseStateInfo(bool success, string amountInfo = "")
    {
        purchaseInfoPanel.LeanScale(Vector3.one, 0.3f).setEaseOutBack();
        if (success)
        {
            diamondIcon.SetActive(true);
            purchaseInfoText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,112.5f);
            purchaseInfoText.text = "Purchased!";
            purchaseAmountInfoText.text = amountInfo;
        }
        else
        {
            diamondIcon.SetActive(false);
            purchaseInfoText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
            purchaseInfoText.text = "Purchase Canceled!";
            purchaseAmountInfoText.text = "";
        }        
    }
    public void ClosePurchaseInfoPanel()
    {
        // AudioManager.Singleton.SFXOneShot("Purchase Success"); // Play purchase sound effect on successful purchase
        purchaseInfoPanel.LeanScale(Vector3.zero, 0.3f).setEaseInBack();
    }
}
