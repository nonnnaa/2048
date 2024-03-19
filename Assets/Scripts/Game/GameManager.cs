using System.Collections;
using TMPro;
using UnityEngine;
using Firebase.Auth;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TileBoard board;
    [SerializeField] private CanvasGroup gameOver;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hiscoreText;
    [SerializeField] private TextMeshProUGUI UserName;
    [SerializeField] private TMP_InputField changeUserNameInputField;
    private int score;
    public int Score => score;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void Start()
    {
        NewGame();
    }

    public void NewGame()
    {
        // reset score
        SetScore(0);
        hiscoreText.text = LoadHiscore().ToString();

        // hide game over screen
        gameOver.alpha = 0f;
        gameOver.interactable = false;

        // update board state
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
                    DisplayName = changeUserNameInputField.text,
                    PhotoUrl = new System.Uri("https://placehold.co/600x400"),
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
}
