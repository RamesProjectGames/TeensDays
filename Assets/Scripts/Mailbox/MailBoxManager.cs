using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

[System.Serializable]
public class MailMessage
{
    public string id;
    public string title;
    public string content;
    public bool isRead;
    public string date;

    public MailMessage(string title, string date, string content)
    {
        this.id = System.Guid.NewGuid().ToString();
        this.title = title;
        this.content = content;
        this.isRead = false;
        this.date = date;
    }
}

[System.Serializable]
public class MailboxData
{
    public List<MailMessage> messages = new List<MailMessage>();
}

public class MailBoxManager : MonoBehaviour
{
    public static MailBoxManager Instance;
    private string savePath;
    public MailboxData mailboxData = new MailboxData();

    private async void Start()
    {
        await LoadMailboxAsync();

        // ✅ Tambah data dummy kalau kosong
        if (mailboxData.messages.Count == 0)
        {
            AddMessage("Selamat Datang!", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"), "Terima kasih sudah memainkan game ini.");
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        // savePath = Path.Combine(Application.streamingAssetsPath, "mailbox.json");
        // Debug.Log(savePath);
    }

    public async Task LoadMailboxAsync()
    {
        string json = await CloudManager.Instance.LoadFromJSONCloud("mailboxData");
        if (!string.IsNullOrEmpty(json))
            mailboxData = JsonUtility.FromJson<MailboxData>(json);
        else
            mailboxData = new MailboxData();
    }

    public void AddMessage(string title,string date, string content)
    {
        mailboxData.messages.Add(new MailMessage(title,date, content));
        SaveMailbox();
    }

    public void MarkAsRead(string id)
    {
        MailMessage msg = mailboxData.messages.Find(m => m.id == id);
        if (msg != null) msg.isRead = true;
        SaveMailbox();
    }

    public void DeleteMessage(string id)
    {
        mailboxData.messages.RemoveAll(m => m.id == id);
        SaveMailbox();
    }

    public void SaveMailbox()
    {
        string json = JsonUtility.ToJson(mailboxData.messages, true);
        CloudManager.Instance.SaveToCloudAsJSON("mailboxData", json);
    }
}