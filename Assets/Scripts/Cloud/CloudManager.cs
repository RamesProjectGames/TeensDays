using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
    private static CloudManager instance = null;
    public static CloudManager Instance => instance;
    private DatabaseReference dbRef;
    // Start is called before the first frame update
    void Awake()
    {
        transform.SetParent(null);
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveToCloudAsJSON( string childData, string jsonData)
    {
        if(AuthenticationManager.Singleton.auth.CurrentUser == null)
        {
            Debug.LogWarning("Player is not signed in. Cannot save to cloud.");
            return;
        }
        string userId = AuthenticationManager.Singleton.auth.CurrentUser.UserId;
        // Simulate a cloud save operation with a delay
        dbRef.Child("users").Child(userId).Child(childData).SetRawJsonValueAsync(jsonData);
    }
    public async Task<string> LoadFromJSONCloud(string childData)
    {
        if(AuthenticationManager.Singleton.auth.CurrentUser == null)
        {
            Debug.LogWarning("Player is not signed in. Cannot load from cloud.");
            return null;
        }
        string userId = AuthenticationManager.Singleton.auth.CurrentUser.UserId;
        // Simulate a cloud load operation with a delay
        var snapshot = await dbRef.Child("users").Child(userId).Child(childData).GetValueAsync();
        if(!snapshot.Exists)
        {
            Debug.LogWarning($"No data found for user {userId} at {childData}");
            return null;
        }
        return snapshot.GetRawJsonValue();
    }
    public async void SaveToCloudAsVariable(string childData, object data)
    {
        if(AuthenticationManager.Singleton.auth.CurrentUser == null)
        {
            Debug.LogWarning("Player is not signed in. Cannot save to cloud.");
            return;
        }
        string userId = AuthenticationManager.Singleton.auth.CurrentUser.UserId;
        // Simulate a cloud save operation with a delay
        await dbRef.Child("users").Child(userId).Child(childData).SetValueAsync(data);
    }
    public async Task<object> LoadFromCloudAsVariable(string childData)
    {
        if(AuthenticationManager.Singleton.auth.CurrentUser == null)
        {
            Debug.LogWarning("Player is not signed in. Cannot load from cloud.");
            return null;
        }
        string userId = AuthenticationManager.Singleton.auth.CurrentUser.UserId;
        // Simulate a cloud load operation with a delay
        var snapshot = await dbRef.Child("users").Child(userId).Child(childData).GetValueAsync();
        if(!snapshot.Exists)
        {
            Debug.LogWarning($"No data found for user {userId} at {childData}");
            return null;
        }
        return snapshot.Value;
    }
    public async Task DeletePlayerData(string uid = "")
    {
        if (AuthenticationManager.Singleton.auth.CurrentUser == null)
        {
            Debug.LogWarning("Player is not signed in. Cannot delete from cloud.");
            return;
        }
        // StartCoroutine(RandomPlayerAPI.Singleton.UnregisterPlayer(uid));
        // DocumentReference userMailRef = FirebaseFirestore.DefaultInstance.Collection("systemMail").Document(uid);
        // Debug.Log(userMailRef);
        // 3. Execute the asynchronous delete operation
        try
        {
            // DocumentSnapshot userMails = await userMailRef.GetSnapshotAsync();
            Dictionary<string, object> updates = new Dictionary<string, object>();
            // foreach (KeyValuePair<string, object> mail in userMails.ToDictionary())
            // {
            //     updates[mail.Key] = Firebase.Firestore.FieldValue.Delete;
            // }
            // await userMailRef.UpdateAsync(updates).ContinueWith(async deleteTask =>
            // {
            //     if (deleteTask.IsCanceled)
            //     {
            //         Debug.Log("Document deletion was cancelled.");
            //     }
            //     else if (deleteTask.IsFaulted)
            //     {
            //         Debug.LogError($"Document deletion failed: {deleteTask.Exception}");
            //     }
            //     else
            //     {
            //         Debug.Log($"Document {uid} successfully deleted.");

            //         await userMailRef.DeleteAsync().ContinueWith(async deleteTask =>
            //         {
            //             if (deleteTask.IsCanceled)
            //             {
            //                 Debug.Log("Document deletion was cancelled.");
            //             }
            //             else if (deleteTask.IsFaulted)
            //             {
            //                 Debug.LogError($"Document deletion failed: {deleteTask.Exception}");
            //             }
            //             else
            //             {
            //                 Debug.Log($"Document {uid} successfully deleted.");

            //                 // Hapus data di Realtime Database
            //             }
            //         });
            //     }
            // });
             await dbRef
            .Child("users")
            .Child(uid)
            .RemoveValueAsync();

        Debug.Log("Player data deleted successfully.");

        // =========================
        // DELETE ALL LEADERBOARDS
        // =========================

        await LeaderboardSystem.Instance.DeletePlayerFromAllLeaderboards(uid);

        Debug.Log("Leaderboard entries deleted.");

        // =========================
        // DELETE RUN DATA
        // =========================

        await FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("runs")
            .Child(uid)
            .RemoveValueAsync();

        Debug.Log("Run data deleted.");

        // =========================
        // CLEAR LOCAL STORAGE
        // =========================

        PlayerPrefs.DeleteKey(AuthenticationManager.userIdString);

        Debug.Log("Local data cleared.");
            
        }
        catch (System.Exception e)
         {
             Debug.LogError($"Error deleting document: {e}");
         }
    }
}
