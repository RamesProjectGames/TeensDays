using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Tooltip("Daftar nama scene yang bisa dipilih lewat index")]
    public string[] sceneNames;  // array nama scene

    public void LoadSceneByIndex(int index)
    {
        if (index >= 0 && index < sceneNames.Length)
        {
            SceneManager.LoadScene(sceneNames[index]);
        }
        else
        {
            Debug.LogWarning("Index scene tidak valid: " + index);
        }
    }
}
