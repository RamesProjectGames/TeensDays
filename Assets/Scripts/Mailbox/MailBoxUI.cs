using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailBoxUI : MonoBehaviour
{
    public Transform listParent;   // Kontainer isi ScrollView
    public GameObject messagePrefab; // Prefab item pesan
    public GameObject detailPanel; // Panel untuk baca detail pesan
    public TMP_Text detailTitle;
    public TMP_Text detailContent;

    void Start()
    {
        //RefreshList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            RefreshList();
            MailBoxManager.Instance.AddMessage("Pesan Baru", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"), "Isi pesan baru ini dikirim pada ");
        }
    }

    public void RefreshList()
    {
        // Hapus semua item lama
        foreach (Transform child in listParent) Destroy(child.gameObject);

        foreach (var msg in MailBoxManager.Instance.mailboxData.messages)
        {
            GameObject item = Instantiate(messagePrefab, listParent);
            TMP_Text titleText = item.transform.Find("Title").GetComponent<TMP_Text>();
            TMP_Text titleDate = item.transform.Find("DateText").GetComponent<TMP_Text>();
            titleText.text = (msg.isRead ? "" : "[NEW] ") + msg.title;
            titleDate.text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            Button readBtn = item.transform.Find("ReadBtn").GetComponent<Button>();
            readBtn.onClick.AddListener(() =>
            {
                ShowDetail(msg);
                MailBoxManager.Instance.MarkAsRead(msg.id);
                RefreshList();
            });
        }
    }

    public void ShowDetail(MailMessage msg)
    {
        detailPanel.SetActive(true);
        detailTitle.text = msg.title;
        detailContent.text = msg.content;
    }

    public void CloseDetail()
    {
        detailPanel.SetActive(false);
    }
}
