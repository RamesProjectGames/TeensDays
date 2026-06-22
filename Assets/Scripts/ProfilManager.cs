using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class SchoolRankDisplay
{
    public Image rankSlider;
    public TMP_Text rankTextProgress;
    public Vector2 rangeLimitRank;
}
public class ProfilManager : MonoBehaviour
{
    public static ProfilManager Instance { get; private set; }
    //public GameObject panelDaily;
    public Image profImage;
    public Image menuImage;
    public Slider expSliderProf;
    public TMP_Text currTextProf;
    public TMP_Text diaTextProf;
    public TMP_InputField nameTextProf;
    public Button EditNameButton;
    public Transform changeNameProf;
    public GameObject bobonPreviewPrefab;
    public InspectObject bobonPreview;

    [Header("Buttons In Profile")]
    public Button[] profileBtns;
    //public Sprite[] onClickBtns;
    //public Sprite[] onUpBtns2;
    public GameObject[] kontents;
    public int selectedIndex;

    public List<Sprite> playerIcons = new List<Sprite>();
    public List<SchoolRankDisplay> schoolRankDisplays = new List<SchoolRankDisplay>();

    public PlayerManager playerManager;

    string newName;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //panelDaily.SetActive(true);
        menuImage = profImage;

        SpawnBobonPreview();

        for (int i = 0; i < profileBtns.Length; i++)
        {
            int index = i;
            profileBtns[i].onClick.AddListener(() => OnTabClicked(index));
        }

        OnTabClicked(0); // Pilih tab pertama saat mulai

        SliderRank();
    }

    // Update is called once per frame
    void Update()
    {
        currTextProf.text = playerManager.money_text.text;
        diaTextProf.text = playerManager.diamond_text.text;
        nameTextProf.text =  (GameManager.Instance.playerData.displayName == null)? "" : GameManager.Instance.playerData.displayName;
        profImage.sprite = playerIcons[GameManager.Instance.playerData.playerIconIndex];
        expSliderProf.value = Mathf.Clamp01(GameManager.Instance.playerData.expLevel / 100f);
        SliderRank();
    }
    public void SpawnBobonPreview()
    {
        var inspectData = InspectManager.Instance.OnItemSelected(bobonPreviewPrefab);

        InspectObject inspectUI = bobonPreview.GetComponent<InspectObject>();

        inspectUI.Horizontal = true;
        inspectUI.Vertical = false;

        inspectUI.inspectGuid = inspectData.guid;
    }
    public void OpenChangeName(bool open)
    {
        if(changeNameProf == null) return;
        if(open)
        {
            changeNameProf.LeanScale(Vector3.one, 1.5f).setOnStart(() =>
            {
                changeNameProf.transform.localScale = Vector3.zero;
            });
        }
        else
        {
            changeNameProf.LeanScale(Vector3.zero, 1.5f);
        }
    }
    public void SliderRank()
    {
        int unlockedLevel = GameManager.Instance.playerData.unlockedLevel;

        foreach (var rankDisplay in schoolRankDisplays)
        {
            int minLevel = Mathf.RoundToInt(rankDisplay.rangeLimitRank.x);
            int maxLevel = Mathf.RoundToInt(rankDisplay.rangeLimitRank.y);

            if (unlockedLevel >= minLevel && unlockedLevel <= maxLevel)
            {
                int currentLevel = unlockedLevel - minLevel + 1;
                int totalLevels = maxLevel - minLevel + 1;

                rankDisplay.rankSlider.fillAmount = (float)currentLevel / totalLevels;
                rankDisplay.rankTextProgress.text = currentLevel + " / " + totalLevels;
            }
            else if (unlockedLevel > maxLevel)
            {
                // Rank completed
                int totalLevels = maxLevel - minLevel + 1;
                rankDisplay.rankSlider.fillAmount = 1f;
                rankDisplay.rankTextProgress.text = totalLevels + " / " + totalLevels;
            }
            else
            {
                // Rank not yet reached
                int totalLevels = maxLevel - minLevel + 1;
                rankDisplay.rankSlider.fillAmount = 0f;
                rankDisplay.rankTextProgress.text = $"0 / {totalLevels}";
            }
        }
    }

    public void OnTabClicked(int index)
    {
        selectedIndex = index;

        for (int i = 0; i < profileBtns.Length; i++)
        {
            Image buttonImage = profileBtns[i].GetComponent<Image>();
            TextMeshProUGUI buttonText = profileBtns[i].GetComponentInChildren<TextMeshProUGUI>();

            if (i == index)
            {
                //buttonImage.sprite = onClickBtns[i];
                kontents[i].SetActive(true);
            }
            else
            {
                //buttonImage.sprite = onUpBtns2[i];
                kontents[i].SetActive(false);
            }
        }
    }
    public void CompleteAllClass()
    {
        GameManager.Instance.UnlockedAllSegmentClass();
    }
    public void OnNameChanged(string newName)
    {
        this.newName = newName;
        OpenChangeName(true);
    }
    public void ChangeName()
    {
        DateTime lastChange = new DateTime(GameManager.Instance.playerData.replaceNameCooldown, DateTimeKind.Utc);
        bool canChangeName =DateTime.UtcNow >= lastChange;
        if(!canChangeName)
        {
            return;
        }
        if(string.IsNullOrEmpty(newName) || string.Compare(newName, GameManager.Instance.playerData.displayName) == 0)
        {            
            nameTextProf.text = GameManager.Instance.playerData.displayName;
        }
        else
        {
           nameTextProf.text = newName;            
        }
        nameTextProf.readOnly = true;
        nameTextProf.interactable = false;
        EditNameButton.interactable = true;
        nameTextProf.DeactivateInputField();
        GameManager.Instance.playerData.replaceNameCooldown = DateTimeOffset.UtcNow.AddHours(72).ToUnixTimeMilliseconds();
        GameManager.Instance.playerData.displayName = nameTextProf.text;
        GameManager.Instance.SavePlayerDataToCloud();
    }
    public void CancelChange()
    {
        nameTextProf.text = GameManager.Instance.playerData.displayName;
        nameTextProf.readOnly = false;
        nameTextProf.interactable = true;
        nameTextProf.ActivateInputField();
        EditNameButton.interactable = false;
        OpenChangeName(false);
    }
    public void ToggleNameInputField()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long remaining = GameManager.Instance.playerData.replaceNameCooldown  - currentTime;

        if (remaining > 0)
        {
            TimeSpan timeLeft = TimeSpan.FromMilliseconds(remaining);

            string remainingTimeText = $"{timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m left";

            //Added To UI, Disable Button Change
        }
        else
        {
            // Debug.Log("You can change your name now!");
            if (nameTextProf.readOnly)
            {
                nameTextProf.readOnly = false;
                nameTextProf.interactable = true;
                nameTextProf.ActivateInputField();
                EditNameButton.interactable = false;
            }
        }
    }
}
