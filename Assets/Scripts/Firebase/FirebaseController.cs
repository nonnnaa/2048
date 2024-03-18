using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FirebaseController : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseUser user;
    public GameObject loginPanel, registerPanel, forgetPasswordPanel;

    public TMP_InputField loginEmail, loginPassword, registerEmail, registerPassword, registerConfirmPassword, userName;

    public TextMeshProUGUI message;

    public Toggle rememberMe;

    private void Awake()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                InitializeFirebase();

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
    public void OpenLoginPanel()
    {
        if(!loginPanel.activeSelf) loginPanel.SetActive(true);
        if(registerPanel.activeSelf) registerPanel.SetActive(false);
        if (forgetPasswordPanel.activeSelf) forgetPasswordPanel.SetActive(false);
    }
    public void OpenRegisterPanel()
    {
        if (loginPanel.activeSelf) loginPanel.SetActive(false);
        if (!registerPanel.activeSelf) registerPanel.SetActive(true);
        if (forgetPasswordPanel.activeSelf) forgetPasswordPanel.SetActive(false);
    }

    public void OpenForgetPasswordPanel()
    {
        if (!forgetPasswordPanel.activeSelf) forgetPasswordPanel.SetActive(true);
        if (loginPanel.activeSelf) loginPanel.SetActive(false);
        if (registerPanel.activeSelf) registerPanel.SetActive(false);
    }
    public void Login()
    {
        if(string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            Debug.Log("LOGIN");
            ShowNotificationMessage("Error :", "Email or Password are invalid!");
            return;
        }
        else
        {        
            // Do login
            SignInUser(loginEmail.text, loginPassword.text);
        }
    }
    public void Register()
    {
        if (string.IsNullOrEmpty(registerEmail.text) || 
            string.IsNullOrEmpty(registerPassword.text) || 
            string.IsNullOrEmpty(registerConfirmPassword.text))
        {
            ShowNotificationMessage("Error :", "Email or Password or Confirm Password are invalid!");
            Debug.Log("REGISTER");
            return;
        }
        else
        {
            // Do register
            CreateUser(registerEmail.text, registerPassword.text, userName.text);
        } 
    }


    public void ForgetPassword()
    {
        Debug.Log("Forget password");
    }

    public void SubmitEmail()
    {
        Debug.Log("Submit email!");
    }

    public void ShowNotificationMessage(string title, string mes)
    {
        message.text = title + " " + mes;
    }

    public void CreateUser(string email, string password, string name)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                ShowNotificationMessage("Error :", "Register failed!");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                ShowNotificationMessage("Error :", "Register failed!");
                return;
            }

            // Firebase user has been created.
            AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
            UpdateUserProfile(name);
        });
    }
    public void SignInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                ShowNotificationMessage("Error :", "Login failed!");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                ShowNotificationMessage("Error :", "Login failed!");
                return;
            }
            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
            SceneManager.LoadScene(1);
            Debug.Log("Load Game");
        });
    }

    void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }


    public void UpdateUserProfile(string name)
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            UserProfile profile = new UserProfile
            {
                DisplayName = name,
                PhotoUrl = new System.Uri("https://placehold.co/600x400"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }
                Debug.Log("User profile updated successfully.");
            });
        }
    }

}
