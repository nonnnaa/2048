using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Firebase;
public class FirebaseController : MonoBehaviour
{
    public static FirebaseAuth auth;
    public static FirebaseUser user;
    public GameObject loginPanel, registerPanel, forgetPasswordPanel;

    public TMP_InputField loginEmail, loginPassword, registerEmail, registerPassword, registerConfirmPassword, userName, forgetEmail;

    public TextMeshProUGUI message;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });

    }
    private void Start()
    {
        GameManager.hidePassword += HidePassword;
        GameManager.saveDataUser += SaveInfoUser;
        if(GameManager.Instance.isRememberDataUser == "On")
        {
            loginEmail.text = PlayerPrefs.GetString(CONSTANT.userEmail);
            loginPassword.text = PlayerPrefs.GetString(CONSTANT.userPassword);
        }
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
        StartCoroutine(SignInUser(loginEmail.text, loginPassword.text));
    }
    public void Register()
    {
       StartCoroutine(CreateUser(registerEmail.text, registerPassword.text, userName.text));
    }
    public void SubmitEmail()
    {
        string emailAddress = forgetEmail.text;
        if (user != null)
        {
            auth.SendPasswordResetEmailAsync(emailAddress).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    ShowNotificationMessage("Error :", "SendPasswordResetEmailAsync was canceled");
                    return;
                }
                if (task.IsFaulted)
                {
                    ShowNotificationMessage("Error :", task.Exception.ToString());
                    return;
                }
            });
            ShowNotificationMessage("Submited !", "");
            OpenLoginPanel();
        }
    }
    public void ShowNotificationMessage(string title, string mes)
    {
        message.text = title + " " + mes;
    }

    private IEnumerator CreateUser(string email, string password, string name)
    {
        if (name.Length > 10) yield return null;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;
            string failedMessage = "";
            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid";
                    break;
                case AuthError.EmailAlreadyInUse:
                    failedMessage += "Email Already in use";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missing";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missing";
                    break;
                default:
                    failedMessage = "Login Failed";
                    break;
            }
            ShowNotificationMessage("Error :", failedMessage);
        }
        else
        {
            UpdateUserProfile(name);
            OpenLoginPanel();
        }
    }

    IEnumerator SignInUser(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;
            string failedMessage = "";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Wrong Password";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missing";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missing";
                    break;
                default:
                    failedMessage = "Login Failed";
                    break;
            }
            ShowNotificationMessage("Error :", failedMessage);
        }
        else
        {
            var result = loginTask.Result;
            GameManager.Instance.emailUser = loginEmail.text;
            GameManager.Instance.passwordUser = loginPassword.text;
            GameManager.Instance.Load();
            Debug.Log(result);
        }
    }

    void InitializeFirebase()
    {
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
                DisplayName = name
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    return;
                }
                if (task.IsFaulted)
                {
                    return;
                }
            });
        }
    }
    public void LogOut()
    {
        if(auth != null && user != null)
        {
            auth.SignOut();
            GameManager.Instance.Load();
            loginEmail.text = string.Empty;
            HidePassword();
        }
    }
    public void HidePassword()
    {
        loginPassword.text = string.Empty;
    }

    public void SaveInfoUser()
    {
        PlayerPrefs.SetString(CONSTANT.userEmail, GameManager.Instance.emailUser);
        PlayerPrefs.SetString(CONSTANT.userPassword, GameManager.Instance.passwordUser);
    }
}
