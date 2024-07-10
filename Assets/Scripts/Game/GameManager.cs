using System.Collections;
using TMPro;
using UnityEngine;
using Firebase.Auth;
using System;
using UnityEngine.UI;

public static class CONSTANT
{
    public static string isRemember = "isRemember";
    public static string userEmail = "userEmail";
    public static string userPassword = "userPassword";
}


public class GameManager : MonoBehaviour
{

    public static event Action hidePassword, saveDataUser; 
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject LoginUI, GameUI;
    [SerializeField] private TileBoard board;
    [SerializeField] private CanvasGroup gameOver;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hiscoreText;
    [SerializeField] private TextMeshProUGUI UserName;
    [SerializeField] private TMP_InputField changeUserNameInputField;
    public string emailUser, passwordUser;
    public string isRememberDataUser;
    private int score;

    public int Score => score;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        isRememberDataUser = PlayerPrefs.GetString(CONSTANT.isRemember);
    }

    public void NewGame()
    {
        SetScore(0);
        hiscoreText.text = LoadHiscore().ToString();

        gameOver.alpha = 0f;
        gameOver.interactable = false;

        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
        UserName.text = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
    }

    public void GameOver()
    {
        board.enabled = false;
        gameOver.interactable = true;
        StartCoroutine(Fade(gameOver, 1f, 1f));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        SaveHiscore();
    }

    private void SaveHiscore()
    {
        int hiscore = LoadHiscore();

        if (score > hiscore) {
            PlayerPrefs.SetInt("hiscore", score);
        }
    }

    private int LoadHiscore()
    {
        return PlayerPrefs.GetInt("hiscore", 0);
    }


    public void ChangeUserName()
    {
        changeUserNameInputField.gameObject.SetActive(true);
    }

    public void EnterChangeUserName()
    {
        StartCoroutine(EnterChange());
    }
    IEnumerator EnterChange()
    {
        if (changeUserNameInputField.text.Length < 10 && changeUserNameInputField.text.Length > 0)
        {
            FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user != null)
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = changeUserNameInputField.text
                };
                var tmp = user.UpdateUserProfileAsync(profile).ContinueWith(task => {
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
                yield return new WaitUntil(() => tmp.IsCompleted);
            }

        }
        changeUserNameInputField.gameObject.SetActive(false);
        UserName.text = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
    }
    private void OnApplicationQuit()
    {
        if (isRememberDataUser == "On")
        {
            PlayerPrefs.SetString(CONSTANT.isRemember, "On");
            saveDataUser?.Invoke();
        }
        else
        {
            PlayerPrefs.SetString(CONSTANT.isRemember, "Off");
            emailUser = passwordUser = null;
        }
    }
    public void ExitGame()
    {
        Load();
    }
    public void Load()
    {
        LoginUI.SetActive(!LoginUI.activeSelf);
        if(isRememberDataUser == "Off") hidePassword?.Invoke();
        GameUI.SetActive(!GameUI.activeSelf);
        if(GameUI.activeSelf) NewGame();
    }
}
