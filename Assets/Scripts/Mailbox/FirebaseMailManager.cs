using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using System.Linq;
using System.Threading.Tasks;

public class FirebaseMailManager : MonoBehaviour
{
    [Header("Mailbox")]
    private DatabaseReference db;
    private DatabaseReference userMailRef;
    [SerializeField] private Transform mailListParent;
    [SerializeField] private GameObject mailPrefab;
    [SerializeField] public Sprite unreadIcon, readIcon;
    //[SerializeField] private GameObject notifBubble;
    List<MailItem> mailItems;
    [Header("Mail View")]
    [SerializeField] private GameObject mailView;
    [SerializeField] private TextMeshProUGUI mailTitle, mailDate, mailBody;
    [SerializeField] private Transform mailRewardsParent;
    [SerializeField] private Button claimButton, deleteButton;
    [SerializeField] private Sprite diamondIcon, coinIcon, claimedFrame, unclaimedFrame;
    private List<MailRewardUI> currentMailRewardList;
    private MailItem focusMail;
    [Header("Reward Info")]
    [SerializeField] private GameObject rewardPrefab;
    [SerializeField] private GameObject rewardModal;
    [SerializeField] private TextMeshProUGUI rewardName, rewardDesc, rewardOwned;
    [SerializeField] private GameObject allMailRewardModal;
    [Header("All Rewards Claim")]
    private List<MailRewardUI> allMailRewardList;
    [SerializeField] private Transform allMailRewardsParent;
    [Header("Delete Modal")]
    [SerializeField] private GameObject deleteConfirmationModal;
    [SerializeField] private GameObject deleteAllConfirmationModal;

    public static FirebaseMailManager Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        db = FirebaseDatabase.DefaultInstance.RootReference;
        currentMailRewardList = new List<MailRewardUI>();
        allMailRewardList = new List<MailRewardUI>();
        InitializeMailBox();
    }

    private IEnumerator CheckExpiredMail()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f); // Check every second

            for (int i = mailItems.Count - 1; i >= 0; i--)
            {
                MailItem mail = mailItems[i];
                if ((mail.endDate - DateTime.UtcNow).TotalSeconds < 0)
                {
                    Debug.Log($"Mail expired: {mail.title}");
                    Destroy(mail.gameObject);
                    mailItems.RemoveAt(i);
                    //if (mailItems.FindAll(x => !x.isClaimed).Count() <= 0)
                    //    notifBubble.SetActive(false);
                }
                else if ((mail.endDate - DateTime.UtcNow).TotalSeconds < 60)
                    mail.datePreview.text = $"Less than a minute remaining";
                else if ((mail.endDate - DateTime.UtcNow).TotalMinutes < 60)
                    mail.datePreview.text = $"{Math.Floor((mail.endDate - DateTime.UtcNow).TotalMinutes)} minutes remaining";
                else if ((mail.endDate - DateTime.UtcNow).TotalHours < 24)
                    mail.datePreview.text = $"{Math.Floor((mail.endDate - DateTime.UtcNow).TotalHours)} hours remaining";
                else if ((mail.endDate - DateTime.UtcNow).TotalDays < 14)
                    mail.datePreview.text = $"{Math.Floor((mail.endDate - DateTime.UtcNow).TotalDays)} days remaining";
                else if ((mail.endDate - DateTime.UtcNow).TotalDays < 30)
                    mail.datePreview.text = $"{Math.Floor((mail.endDate - DateTime.UtcNow).TotalDays / 7)} weeks remaining";
                else if ((mail.endDate - DateTime.UtcNow).TotalDays < 365)
                    mail.datePreview.text = $"{Math.Floor((mail.endDate - DateTime.UtcNow).TotalDays / 30)} months remaining";
                else if (mail.endDate.Date == DateTime.MaxValue.Date)
                    mail.datePreview.text = $"Permanent";
            }
        }
    }

    public async void InitializeMailBox()
    {
        //while (AuthenticationManager.Singleton?.auth?.CurrentUser == null)
        //{
        //    await Task.Yield(); // or await Task.Delay(100); to save CPU cycles
        //}
        userMailRef = db.Child($"systemMail/{AuthenticationManager.Singleton?.auth?.CurrentUser.UserId}");
        if (mailItems != null)
        {
            foreach (MailItem item in mailItems)
            {
                Destroy(item.gameObject);
            }
            mailItems.Clear();
            mailItems.TrimExcess();
        }
        DataSnapshot mailSnapshot = await userMailRef.GetValueAsync();
        if (!mailSnapshot.Exists)
        {
            DataSnapshot systemMailSnapshot = await db.Child("mainSystemMail").GetValueAsync();

            if (systemMailSnapshot.Exists && systemMailSnapshot.ChildrenCount > 0)
            {
                Dictionary<string, object> updateBatch = new Dictionary<string, object>();
                long currentUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                foreach (DataSnapshot message in systemMailSnapshot.Children)
                {
                    var expiresAtNode = message.Child("expiresAt");

                    if (expiresAtNode.Exists && expiresAtNode.Value != null)
                    {
                        Debug.Log($"Message is valid {message.Key}");
                        if (long.TryParse(expiresAtNode.Value.ToString(), out long expiresAtMs))
                        {
                            if (currentUnixMs <= expiresAtMs)
                            {
                                // Extract the message data as a Dictionary/Map
                                // Using GetValue(true) preserves Firebase types correctly
                                var messageData = message.GetValue(true);

                                // Relative path from userMailRef: "messageId"
                                updateBatch.Add(message.Key, messageData);
                            }
                        }
                    }
                }

                Debug.Log($"Mail Count: {updateBatch.Count}");
                if (updateBatch.Count > 0)
                {
                    try
                    {
                        // Writes all messages atomically under userMailRef
                        await userMailRef.UpdateChildrenAsync(updateBatch);
                        Debug.Log($"{updateBatch.Count} system messages successfully copied.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Multi-path update failed: {ex.Message}");
                    }
                }
            }

            // Refresh snapshot so the rest of your script has the newly written data
            mailSnapshot = await userMailRef.GetValueAsync();
        }
        mailItems = new List<MailItem>();
        if (mailSnapshot != null)
        {
            foreach (DataSnapshot message in mailSnapshot.Children)
            {
                if (message.Child("isDeleted").Exists && !Convert.ToBoolean(message.Child("isDeleted").Value) &&
                message.Child("startsAt").Exists && DateTime.Now >= DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(message.Child("startsAt").Value)).DateTime &&
                message.Child("expiresAt").Exists && DateTime.Now <= DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(message.Child("expiresAt").Value)).DateTime)
                {
                    MailItem mailItem = Instantiate(mailPrefab, mailListParent).GetComponent<MailItem>();
                    mailItem.mailId = message.Key;
                    mailItem.title = message.Child("title").Value.ToString(); //No checks needed bcs it's a must
                    mailItem.titlePreview.text = mailItem.title;
                    mailItem.body = message.Child("body").Exists ? message.Child("title").Value.ToString() : string.Empty;
                    mailItem.sentDate = message.Child("startsAt").Exists ? DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(message.Child("startsAt").Value)).DateTime : DateTime.Now;
                    mailItem.endDate = message.Child("expiresAt").Exists ? DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(message.Child("expiresAt").Value)).DateTime : DateTime.Now;
                    mailItem.isClaimed = message.Child("isClaimed").Exists ? bool.Parse(message.Child("isClaimed").Value.ToString()) : true;
                    mailItem.isRead = message.Child("isRead").Exists ? bool.Parse(message.Child("isRead").Value.ToString()) : true;
                    mailItem.iconPreview.sprite = mailItem.isRead ? readIcon : unreadIcon;

                    //if (!mailItem.isClaimed)
                    //    notifBubble.SetActive(true);

                    List<object> rewardList = message.Child("rewards").Exists ? message.Child("rewards").Value as List<object> : null;

                    if (rewardList != null)
                    {
                        mailItem.rewardItems = new List<RewardItem>();
                        int i = 0;
                        foreach (Dictionary<string, object> rewardItems in rewardList)
                        {
                            if (rewardItems.ContainsKey("type"))
                            {
                                RewardItem temp = new RewardItem();
                                switch (rewardItems["type"])
                                {
                                    case "Diamond":
                                        if (rewardItems.ContainsKey("amount"))
                                        {
                                            temp.name = rewardItems["type"].ToString();
                                            temp.amount = int.Parse(rewardItems["amount"].ToString());
                                            temp.type = RewardType.Gems;
                                            i++;
                                        }
                                        break;
                                    case "Money":
                                        if (rewardItems.ContainsKey("amount"))
                                        {
                                            temp.name = rewardItems["type"].ToString();
                                            temp.amount = int.Parse(rewardItems["amount"].ToString());
                                            temp.type = RewardType.Coins;
                                            i++;
                                        }
                                        break;
                                }
                                mailItem.rewardItems.Add(temp);
                            }
                        }
                    }
                    if ((mailItem.endDate - DateTime.UtcNow).TotalSeconds < 60)
                        mailItem.datePreview.text = $"Less than a minute remaining";
                    else if ((mailItem.endDate - DateTime.UtcNow).TotalMinutes < 60)
                        mailItem.datePreview.text = $"{Math.Floor((mailItem.endDate - DateTime.UtcNow).TotalMinutes)} minutes remaining";
                    else if ((mailItem.endDate - DateTime.UtcNow).TotalHours < 24)
                        mailItem.datePreview.text = $"{Math.Floor((mailItem.endDate - DateTime.UtcNow).TotalHours)} hours remaining";
                    else if ((mailItem.endDate - DateTime.UtcNow).TotalDays < 14)
                        mailItem.datePreview.text = $"{Math.Floor((mailItem.endDate - DateTime.UtcNow).TotalDays)} days remaining";
                    else if ((mailItem.endDate - DateTime.UtcNow).TotalDays < 30)
                        mailItem.datePreview.text = $"{Math.Floor((mailItem.endDate - DateTime.UtcNow).TotalDays / 7)} weeks remaining";
                    else if ((mailItem.endDate - DateTime.UtcNow).TotalDays < 365)
                        mailItem.datePreview.text = $"{Math.Floor((mailItem.endDate - DateTime.UtcNow).TotalDays / 30)} months remaining";
                    else if (mailItem.endDate.Date == DateTime.MaxValue.Date)
                        mailItem.datePreview.text = $"Permanent";
                    mailItems.Add(mailItem);
                }
            }
            StartCoroutine(CheckExpiredMail());
        }
    }

    #region Message Details
    public async void OpenMessage(MailItem selectedMail)
    {
        string readPath = $"{selectedMail.mailId}/isRead";
        DataSnapshot mailSnapshot = await userMailRef.GetValueAsync();
        if (mailSnapshot.Child(readPath).Value is bool isRead && !isRead)
        {
            try
            {
                // The UpdateAsync method is a partial update (merge) by default
                await userMailRef.Child(readPath).SetValueAsync(true);
                Debug.Log($"Mail read status updated");
                selectedMail.isRead = true;
                selectedMail.iconPreview.sprite = readIcon;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating mail status: {e.Message}");
            }
        }
        mailTitle.text = selectedMail.title;
        mailBody.text = selectedMail.body;
        if ((selectedMail.endDate - DateTime.UtcNow).TotalSeconds < 60)
            mailDate.text = $"Less than a minute remaining";
        else if ((selectedMail.endDate - DateTime.UtcNow).TotalMinutes < 60)
            mailDate.text = $"{Math.Floor((selectedMail.endDate - DateTime.UtcNow).TotalMinutes)} minutes remaining";
        else if ((selectedMail.endDate - DateTime.UtcNow).TotalHours < 24)
            mailDate.text = $"{Math.Floor((selectedMail.endDate - DateTime.UtcNow).TotalHours)} hours remaining";
        else if ((selectedMail.endDate - DateTime.UtcNow).TotalDays < 14)
            mailDate.text = $"{Math.Floor((selectedMail.endDate - DateTime.UtcNow).TotalDays)} days remaining";
        else if ((selectedMail.endDate - DateTime.UtcNow).TotalDays < 30)
            mailDate.text = $"{Math.Floor((selectedMail.endDate - DateTime.UtcNow).TotalDays / 7)} weeks remaining";
        else if ((selectedMail.endDate - DateTime.UtcNow).TotalDays < 365)
            mailDate.text = $"{Math.Floor((selectedMail.endDate - DateTime.UtcNow).TotalDays / 30)} months remaining";
        else if (selectedMail.endDate.Date == DateTime.MaxValue.Date)
            mailDate.text = $"Permanent";

        foreach (RewardItem reward in selectedMail.rewardItems)
        {
            MailRewardUI temp = Instantiate(rewardPrefab, mailRewardsParent).GetComponent<MailRewardUI>();
            temp.rewardData = reward;
            temp.amount.text = reward.amount.ToString();
            temp.frame.sprite = selectedMail.isClaimed ? claimedFrame : unclaimedFrame;
            switch (reward.type)
            {
                case RewardType.Gems:
                    temp.icon.sprite = diamondIcon;
                    temp.icon.preserveAspect = true;
                    break;
                case RewardType.Coins:
                    temp.icon.sprite = coinIcon;
                    temp.icon.preserveAspect = true;
                    break;
            }
            currentMailRewardList.Add(temp);
        }
        claimButton.gameObject.SetActive(!selectedMail.isClaimed);
        claimButton.interactable = !selectedMail.isClaimed;
        deleteButton.interactable = selectedMail.isClaimed;
        deleteButton.gameObject.SetActive(selectedMail.isClaimed);
        focusMail = selectedMail;
        Canvas.ForceUpdateCanvases();
        mailView.SetActive(true);
    }

    public void CloseMessage()
    {
        //AudioManager.Singleton.SFXOneShot("Back");
        mailTitle.text = string.Empty;
        mailDate.text = string.Empty;
        mailBody.text = string.Empty;
        claimButton.gameObject.SetActive(false);
        claimButton.interactable = false;
        deleteButton.interactable = false;
        deleteButton.gameObject.SetActive(false);
        foreach (MailRewardUI temp in currentMailRewardList)
        {
            if (temp != null && temp.gameObject != null)
            {
                Destroy(temp.gameObject);
            }
        }
        currentMailRewardList.Clear();
        currentMailRewardList.TrimExcess();
        focusMail = null;
    }
    #endregion

    #region Mail Rewards
    public void CheckRewardDetails(MailRewardUI item)
    {
        switch (item.rewardData.type)
        {
            case RewardType.Gems:
                rewardName.text = item.rewardData.name;
                rewardDesc.text = "Premium Currency";
                rewardOwned.text = $"{GameManager.Instance.playerData.currDiamond}";
                break;
            case RewardType.Coins:
                rewardName.text = item.rewardData.name;
                rewardDesc.text = "Common currency";
                rewardOwned.text = $"{GameManager.Instance.playerData.currMoney}";
                break;
        }
        rewardModal.transform.GetChild(0).gameObject.GetComponent<RectTransform>().position = new Vector2(item.GetComponent<RectTransform>().position.x, item.GetComponent<RectTransform>().position.y + item.GetComponent<RectTransform>().rect.height*2f/3f);
        rewardModal.SetActive(true);
    }

    public void CloseRewardDetails()
    {
        rewardModal.SetActive(false);
    }
    #endregion

    #region Delete Mail

    public void ToggleDeleteConfirmation(bool isActive)
    {
        if (isActive)
        {
            //AudioManager.Singleton.SFXOneShot("Click");
        }
        else
        {
            //AudioManager.Singleton.SFXOneShot("Back");
        }
        deleteConfirmationModal.SetActive(isActive);
    }

    public void ToggleDeleteAllConfirmation(bool isActive)
    {
        if (isActive)
        {
            //AudioManager.Singleton.SFXOneShot("Click");
        }
        else
        {
            //AudioManager.Singleton.SFXOneShot("Back");
        }
        deleteAllConfirmationModal.SetActive(isActive);
    }

    public async void DeleteAllMail()
    {
        ToggleDeleteAllConfirmation(false);

        if (mailItems.FindAll(x => x.isClaimed).Count <= 0)
        {
            Debug.Log("No mails to delete.");
            return;
        }

        DataSnapshot mailSnapshot = await userMailRef.GetValueAsync();
        if (!mailSnapshot.Exists)
        {
            Debug.Log("User mail document does not exist. Nothing to delete.");
            return;
        }

        for (int i = mailItems.Count - 1; i >= 0; i--)
        {
            MailItem mail = mailItems[i];
            if (mail == null)
                continue;
            string claimPath = $"{mail.mailId}/isClaimed";
            string deletePath = $"{mail.mailId}/isDeleted";
            if (mailSnapshot.Child(claimPath).Value is bool isClaimed && isClaimed &&
                mailSnapshot.Child(deletePath).Value is bool isDeleted && !isDeleted)
            {
                await userMailRef.Child(deletePath).SetValueAsync(true);
                mailItems.Remove(mail);
                Destroy(mail.gameObject);
            }
        }
        CloseMessage();
    }

    public async void DeleteMail()
    {
        ToggleDeleteConfirmation(false);
        if (focusMail == null)
            return;
        string deletePath = $"{focusMail.mailId}/isDeleted";
        string claimPath = $"{focusMail.mailId}/isClaimed";
        DataSnapshot mailSnapshot = await userMailRef.GetValueAsync();
        if (mailSnapshot.Child(claimPath).Value is bool isClaimed && isClaimed &&
            mailSnapshot.Child(deletePath).Value is bool isDeleted && !isDeleted)
        {
            try
            {
                // The UpdateAsync method is a partial update (merge) by default
                await userMailRef.Child(deletePath).SetValueAsync(true);
                Debug.Log($"Mail availability status updated");
                mailItems.Remove(focusMail);
                Destroy(focusMail.gameObject);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating mail status: {e.Message}");
            }
        }
        CloseMessage();
    }

    #endregion

    #region Claim Mail Rewards
    public async void ClaimAllMail()
    {
        //AudioManager.Singleton.SFXOneShot("Click");
        if (mailItems.FindAll(x => !x.isClaimed).Count <= 0)
        {
            Debug.Log("No mails to claim.");
            return;
        }
        // Dictionary to hold ALL the updates for the single write operation
        DataSnapshot mailSnapshot = await userMailRef.GetValueAsync();

        if (!mailSnapshot.Exists)
        {
            Debug.Log("User mail document does not exist. Nothing to delete.");
            return;
        }

        foreach (MailItem mail in mailItems)
        {
            string claimPath = $"{mail.mailId}/isClaimed";
            string readPath = $"{mail.mailId}/isRead";

            if (mailSnapshot.Child(readPath).Value is bool isRead && !isRead)
            {
                await userMailRef.Child(readPath).SetValueAsync(true);
                mail.isRead = true;
                mail.iconPreview.sprite = readIcon;
                Debug.Log($"Mail read status updated");
            }
            if (mailSnapshot.Child(claimPath).Value is bool isClaimed && !isClaimed)
            {
                try
                {
                    // The UpdateAsync method is a partial update (merge) by default
                    await userMailRef.Child(claimPath).SetValueAsync(true);
                    foreach (RewardItem reward in mail.rewardItems)
                    {
                        if (allMailRewardList.Find(x => x.name == reward.name))
                        {
                            MailRewardUI temp = allMailRewardList.Find(x => x.name == reward.name);
                            temp.rewardData.amount += reward.amount;
                            temp.amount.text = temp.amount.ToString();
                            switch (reward.type)
                            {
                                case RewardType.Gems:
                                    GameManager.Instance.playerData.currDiamond += reward.amount;
                                    break;
                                case RewardType.Coins:
                                    GameManager.Instance.playerData.currMoney += reward.amount;
                                    break;
                            }
                        }
                        else
                        {
                            MailRewardUI temp = Instantiate(rewardPrefab, allMailRewardsParent).GetComponent<MailRewardUI>();
                            temp.amount.text = reward.amount.ToString();
                            temp.frame.sprite = unclaimedFrame;
                            temp.rewardData = reward;
                            switch (reward.type)
                            {
                                case RewardType.Gems:
                                    GameManager.Instance.playerData.currDiamond += reward.amount;
                                    temp.icon.sprite = diamondIcon;
                                    break;
                                case RewardType.Coins:
                                    GameManager.Instance.playerData.currMoney += reward.amount;
                                    temp.icon.sprite = coinIcon;
                                    break;
                            }
                            allMailRewardList.Add(temp);
                        }
                    }
                    mail.isClaimed = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error updating mail status: {e.Message}");
                }
            }
        }
        Debug.Log($"Mail claim status updated");
        ToggleMailRewardPanel(true);
        Debug.Log($"Successfully claimed all mails.");
        if(focusMail != null)
        {
            claimButton.gameObject.SetActive(!focusMail.isClaimed);
            claimButton.interactable = !focusMail.isClaimed;
            deleteButton.interactable = focusMail.isClaimed;
            deleteButton.gameObject.SetActive(focusMail.isClaimed);
        }
        //notifBubble.SetActive(false);

        //CloudManager.Singleton.SaveToCloud();
    }

    public void ToggleMailRewardPanel(bool isActive)
    {
        if (isActive)
        {
            //AudioManager.Singleton.SFXOneShot("Click");
        }
        else
        {
            //AudioManager.Singleton.SFXOneShot("Back");
        }
        allMailRewardModal.SetActive(isActive);
        if (!isActive)
        {
            foreach (MailRewardUI temp in allMailRewardList)
            {
                if (temp != null && temp.gameObject != null)
                {
                    Destroy(temp.gameObject);
                }
            }
            allMailRewardList.Clear();
            allMailRewardList.TrimExcess();
        }
    }

    public async void ClaimMail()
    {
        if (focusMail == null)
            return;

        //AudioManager.Singleton.SFXOneShot("Click");
        focusMail.isClaimed = true;
        claimButton.gameObject.SetActive(!focusMail.isClaimed);
        claimButton.interactable = !focusMail.isClaimed;
        deleteButton.interactable = focusMail.isClaimed;
        deleteButton.gameObject.SetActive(focusMail.isClaimed);

        string claimPath = $"{focusMail.mailId}/isClaimed";
        DataSnapshot mailSnapshot = await userMailRef.GetValueAsync();
        if (mailSnapshot.Child(claimPath).Value is bool isClaimed && !isClaimed)
        {
            try
            {
                await userMailRef.Child(claimPath).SetValueAsync(true);
                Debug.Log($"Mail claim status updated");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating mail status: {e.Message}");
            }

            //Update player data
            foreach (RewardItem rewardItem in focusMail.rewardItems)
            {
                MailRewardUI temp = Instantiate(rewardPrefab, allMailRewardsParent).GetComponent<MailRewardUI>();
                temp.amount.text = rewardItem.amount.ToString();
                temp.frame.sprite = unclaimedFrame;
                temp.rewardData = rewardItem;
                switch (rewardItem.type)
                {
                    case RewardType.Gems:
                        GameManager.Instance.playerData.currDiamond += rewardItem.amount;
                        temp.icon.sprite = diamondIcon;
                        break;
                    case RewardType.Coins:
                        GameManager.Instance.playerData.currMoney += rewardItem.amount;
                        temp.icon.sprite = coinIcon;
                        break;
                }
                allMailRewardList.Add(temp);
            }
            //if (mailItems.FindAll(x => !x.isClaimed).Count() <= 0)
            //    notifBubble.SetActive(false);
            ToggleMailRewardPanel(true);
            //CloudManager.Instance.SaveToCloud();
        }
    }

    #endregion
}
