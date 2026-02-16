using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    public string entryID; // contoh: "Portal_A"

    public void Teleport()
    {
        SceneEntryManager.LastEntryID = entryID;
        SceneManager.LoadScene("SDScene");
    }
}
