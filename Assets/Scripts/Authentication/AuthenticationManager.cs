using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    private static AuthenticationManager singleton = null;
    public static AuthenticationManager Singleton => singleton;
    private bool isAuthenticated;
    public bool IsAuthenticated => isAuthenticated;
    bool googleAuthenticated;
    [Header("Firebase Configuration")]
    public bool debugSignOut = false;
    public FirebaseAuth auth;
    public FirebaseUser currentUser;
    public string GoogleAPI = "707396864543-3qshllveacsvmph6of2dv4an31micd9o.apps.googleusercontent.com";
    public static string userIdString = "omgbruh";
    private bool isGoogleSignInInitialized = false;
    void Awake()
    {
        transform.SetParent(null);
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (singleton != this)
        {
            Destroy(gameObject);
        }
    }
    async void Start()
    {

        await StartClientService();
    }
    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        //#if UNITY_EDITOR
        //    System.Environment.SetEnvironmentVariable("USE_AUTH_EMULATOR", "yes");
        //#else
        System.Environment.SetEnvironmentVariable("USE_AUTH_EMULATOR", "no");
        //#endif
        if (auth != null)
        {
            // auth.StateChanged += AuthStateChanged;
        }
    }
    public async Task StartClientService()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
                Debug.Log("Firebase initialized successfully");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                return;
            }
        }
        else
        {
            InitializeFirebase();
            Debug.Log("Firebase already initialized");
        }


        // Auto sign-in if user exists
        AutoSignIn();
    }
    private void AutoSignIn()
    {
        if(auth == null)
        {
            // Debug.LogError("FirebaseAuth is not initialized.");
            return;
        }
        if (auth.CurrentUser != null)
        {
            currentUser = auth.CurrentUser;
            isAuthenticated = true;
            Debug.Log("Auto-signed in: " + currentUser.UserId);

            if (debugSignOut)
            {
                SignOut();
            }
            else
            {
                OnSignedIn();
            }
        }
        else
        {
            Debug.Log("No user is currently signed in.");
        }
    }
    private void OnSignedIn()
    {
        StartCoroutine(InitializeAfterAuth());
    }

    private IEnumerator InitializeAfterAuth()
    {
        yield return new WaitForSeconds(0.5f);

        if (googleAuthenticated)
        {
            // Set google buttons to "Switch Account" and "Unlink"
        }
        else
        {
             
        }
        // Store user ID in playerData and save to cloud
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.userId = auth.CurrentUser.UserId;
            GameManager.Instance.SavePlayerDataToCloud();
        }

        // Initialize cloud save and other services
        var mainMenuSettings = FindAnyObjectByType<MainMenuSettings>(FindObjectsInactive.Include);
        if (mainMenuSettings != null)
        {
            mainMenuSettings.loadingPanel.SetActive(true);
            mainMenuSettings.ChangeScene("Dwiky");
        }
    }

     #region Anonymous Sign In
    public async void SignInAnonymouslyAsync()
    {
        try
        {
            var result = await auth.SignInAnonymouslyAsync();
            currentUser = result.User;
            Debug.Log($"Anonymous sign in successful: {currentUser.UserId}");
            isAuthenticated = true;
            FindAnyObjectByType<MainMenuSettings>(FindObjectsInactive.Include)?.ShowPlayButton(true);
            OnSignedIn();
        }
        catch (FirebaseException ex)
        {
            Debug.LogError($"Anonymous sign in failed: {ex.Message}");
        }
    }
    #endregion

    #region Google Sign In
    public void SignInWithGoogle()
    {
        
        try
        {
            // Initialize Google Sign-In if not already done
            if (!isGoogleSignInInitialized)
            {
                GoogleSignIn.Configuration = new GoogleSignInConfiguration
                {
                    RequestIdToken = true,
                    WebClientId = GoogleAPI,
                    RequestEmail = true
                };
                isGoogleSignInInitialized = true;
            }

            // Start Google Sign-In
            var signIn = GoogleSignIn.DefaultInstance.SignIn();
            
            signIn.ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("Google sign-in was cancelled.");
                    // ShowError(ErrorMenu.Action.SignIn, "Google sign-in was cancelled.", "OK");
                    return;
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError($"Google sign-in failed: {task.Exception}");
                    // ShowError(ErrorMenu.Action.SignIn, "Google sign-in failed. Please try again.", "OK");
                    return;
                }

                var googleUser = task.Result;
                if (googleUser == null)
                {
                    // ShowError(ErrorMenu.Action.SignIn, "Google sign-in failed.", "OK");
                    return;
                }

                // Create Firebase credential with Google token
                Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
                
                // Sign in to Firebase with Google credential
                auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        Debug.Log("Firebase sign-in was cancelled.");
                        // ShowError(ErrorMenu.Action.SignIn, "Sign-in was cancelled.", "OK");
                    }
                    else if (authTask.IsFaulted)
                    {
                        Debug.LogError($"Firebase sign-in failed: {authTask.Exception}");
                        // ShowError(ErrorMenu.Action.SignIn, "Sign-in failed. Please try again.", "OK");
                    }
                    else
                    {
                        currentUser = authTask.Result;
                        Debug.Log("Google Sign-In Successful: " + currentUser.UserId);
                        googleAuthenticated = true;
                        isAuthenticated = true;
                        FindAnyObjectByType<MainMenuSettings>(FindObjectsInactive.Include)?.ShowPlayButton(true);
                        OnSignedIn();
                        // OnSignedIn will be called automatically via AuthStateChanged
                    }
                });
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Google sign in failed: {ex.Message}");
            // ShowError(ErrorMenu.Action.SignIn, "Google sign-in failed. Please try again.", "OK");
        }
    }

    public void LinkWithGoogleAsync()
    {
        if (auth.CurrentUser == null)
        {
            // ShowError(ErrorMenu.Action.SignIn, "No user is signed in to link with Google.", "OK");
            return;
        }

        try
        {
            // Initialize Google Sign-In if not already done
            if (!isGoogleSignInInitialized)
            {
                GoogleSignIn.Configuration = new GoogleSignInConfiguration
                {
                    RequestIdToken = true,
                    WebClientId = GoogleAPI,
                    RequestEmail = true
                };
                isGoogleSignInInitialized = true;
            }

            // Start Google Sign-In
            var signIn = GoogleSignIn.DefaultInstance.SignIn();
            
            signIn.ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("Google sign-in was cancelled.");
                    // ShowError(ErrorMenu.Action.SignIn, "Google sign-in was cancelled.", "OK");
                    return;
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError($"Google sign-in failed: {task.Exception}");
                    // ShowError(ErrorMenu.Action.SignIn, "Google sign-in failed. Please try again.", "OK");
                    return;
                }

                var googleUser = task.Result;
                if (googleUser == null)
                {
                    // ShowError(ErrorMenu.Action.SignIn, "Google sign-in failed.", "OK");
                    return;
                }

                // Create Firebase credential with Google token
                Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
                
                // Link current user with Google account
                currentUser.LinkWithCredentialAsync(credential).ContinueWith(linkTask =>
                {
                    if (linkTask.IsCanceled)
                    {
                        Debug.Log("Google link was cancelled.");
                        // ShowError(ErrorMenu.Action.SignIn, "Google link was cancelled.", "OK");
                    }
                    else if (linkTask.IsFaulted)
                    {
                        if (linkTask.Exception != null && linkTask.Exception.InnerException is FirebaseException firebaseEx)
                        {
                            if (firebaseEx.ErrorCode == (int)AuthError.CredentialAlreadyInUse)
                            {
                                // ShowError(ErrorMenu.Action.SignIn, "This Google account is already linked to another user.", "OK");
                            }
                            else
                            {
                                // ShowError(ErrorMenu.Action.SignIn, "Failed to link Google account.", "OK");
                            }
                        }
                        else
                        {
                            // ShowError(ErrorMenu.Action.SignIn, "Failed to link Google account.", "OK");
                        }
                        Debug.LogError($"Google link failed: {linkTask.Exception}");
                    }
                    else
                    {
                        currentUser = linkTask.Result.User;
                        // Debug.Log("Google account linked successfully: " + currentUser.UserId);
                        googleAuthenticated = true;
                    }
                });
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Google link failed: {ex.Message}");
            // ShowError(ErrorMenu.Action.SignIn, "Failed to link Google account.", "OK");
        }
    }

    public void UnlinkGoogleAsync()
    {
        if (currentUser == null)
        {
            // ShowError(ErrorMenu.Action.SignIn, "No user is signed in to unlink.", "OK");
            return;
        }

        try
        {
            currentUser.UnlinkAsync(GoogleAuthProvider.ProviderId).ContinueWith(unlinkTask =>
            {
                if (unlinkTask.IsCanceled)
                {
                    Debug.Log("Google unlink was cancelled.");
                    // ShowError(ErrorMenu.Action.SignIn, "Google unlink was cancelled.", "OK");
                }
                else if (unlinkTask.IsFaulted)
                {
                    Debug.LogError($"Google unlink failed: {unlinkTask.Exception}");
                    // ShowError(ErrorMenu.Action.SignIn, "Failed to unlink Google account.", "OK");
                }
                else
                {
                    currentUser = unlinkTask.Result.User;
                    Debug.Log("Google account unlinked successfully.");
                    googleAuthenticated = false;
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Google unlink failed: {ex.Message}");
            // ShowError(ErrorMenu.Action.SignIn, "Failed to unlink Google account.", "OK");
        }
    }
    
    public async void SwitchGoogleAccount()
    {
        if (googleAuthenticated)
        {
            // Debug.Log("Signing out Google");
            GoogleSignIn.DefaultInstance.SignOut();
        }

         if (auth.CurrentUser.IsAnonymous)
         {
             Debug.Log("Deleting anonymous user");
             string userIdToDelete = !string.IsNullOrEmpty(auth.CurrentUser.UserId) ? auth.CurrentUser.UserId : 
                                   (GameManager.Instance != null && GameManager.Instance.playerData != null ? 
                                    GameManager.Instance.playerData.userId : "");
             await CloudManager.Instance.DeletePlayerData(userIdToDelete);
             await auth.CurrentUser.DeleteAsync().ContinueWith(deleteTask =>
             {
                 if (deleteTask.IsCanceled)
                 {
                     Debug.Log("Anonymous user deletion was cancelled.");
                 }
                 else if (deleteTask.IsFaulted)
                 {
                     Debug.LogError($"Anonymous user deletion failed: {deleteTask.Exception}");
                 }
                 else
                 {
                     Debug.Log("Anonymous user deleted successfully.");
                 }
             });
         }
        FirebaseAuth.DefaultInstance.SignOut();
        SignInWithGoogle();
    }
    #endregion

    public async void SignOut()
    {
        if (googleAuthenticated)
            {
                // CloudManager.Singleton.SaveToCloud();
                Debug.Log("Signing out Google");
                GoogleSignIn.DefaultInstance.SignOut();
            }

             if (auth.CurrentUser.IsAnonymous)
             {
                 Debug.Log("Deleting anonymous user");
                 string userIdToDelete = !string.IsNullOrEmpty(auth.CurrentUser.UserId) ? auth.CurrentUser.UserId : 
                                       (GameManager.Instance != null && GameManager.Instance.playerData != null ? 
                                        GameManager.Instance.playerData.userId : "");
                 await CloudManager.Instance.DeletePlayerData(userIdToDelete);
                 await auth.CurrentUser.DeleteAsync().ContinueWith(deleteTask =>
                 {
                     if (deleteTask.IsCanceled)
                     {
                         Debug.Log("Anonymous user deletion was cancelled.");
                     }
                     else if (deleteTask.IsFaulted)
                     {
                         Debug.LogError($"Anonymous user deletion failed: {deleteTask.Exception}");
                     }
                     else
                     {
                         Debug.Log("Anonymous user deleted successfully.");
                     }
                 });
            }
            FirebaseAuth.DefaultInstance.SignOut();
            // No need to delete ExistPlayer key as we're using cloud storage
        }

    public async void DeleteAccount()
    {
        Debug.Log("Deleting account");
        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("No user to delete.");
            return;
        }

        await CloudManager.Instance.DeletePlayerData(!string.IsNullOrEmpty(auth.CurrentUser.UserId) ? auth.CurrentUser.UserId : 
                                                   (GameManager.Instance != null && GameManager.Instance.playerData != null ? 
                                                    GameManager.Instance.playerData.userId : ""));
        await auth.CurrentUser.DeleteAsync().ContinueWith(deleteTask =>
        {
            if (deleteTask.IsCanceled)
            {
                Debug.Log("Anonymous user deletion was cancelled.");
            }
            else if (deleteTask.IsFaulted)
            {
                Debug.LogError($"Anonymous user deletion failed: {deleteTask.Exception}");
            }
            else
            {
                Debug.Log("Anonymous user deleted successfully.");
            }
        });
        FirebaseAuth.DefaultInstance.SignOut();
        PlayerPrefs.DeleteKey("ExistPlayer");
    }

}
