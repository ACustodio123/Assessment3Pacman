using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private Image[] lifeIcons;

    [Header("Texts")]
    [SerializeField] private TMP_Text scoreTxt;
    [SerializeField] private TMP_Text gameTimerTxt;
    [SerializeField] private TMP_Text ghostTimerTxt;
    [SerializeField] private TMP_Text levelNameTxt;

    [Header("Buttons")]
    [SerializeField] private Button exitBtn;

    [Header("Config")]
    [SerializeField] private string startSceneName = "StartScene";
    [SerializeField] private string levelDisplayName = "Level 1";

    [SerializeField] private GhostController[] ghosts;

    [Header("Game Over UI")]
    [SerializeField] private CanvasGroup gameOverBlocker;
    [SerializeField] private TextMeshProUGUI gameOverTxt;
    [SerializeField] private float gameOverDuration = 3f;

    private int score = 0;
    private int lives;
    private float gameTimer = 0.0f;
    private float ghostTimer = 0.0f;

    public System.Action OnGameOver;
    private bool timerRunning = false;
    private bool isGameOver = false;

    void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        int maxLives = 3;
        if (lifeIcons != null && lifeIcons.Length > 0)
        {
            maxLives = lifeIcons.Length;
        }
        lives = Mathf.Clamp(3, 0, maxLives);
        UpdateLivesUI();

        if (scoreTxt != null) { UpdateScoreUI(); }
        if (gameTimerTxt != null) { gameTimerTxt.SetText("00:00:00"); }
        if (levelNameTxt != null) { levelNameTxt.SetText(levelDisplayName); }
        if (ghostTimerTxt != null) { ghostTimerTxt.gameObject.SetActive(false); }
        if (exitBtn != null) { exitBtn.onClick.AddListener(() => { SceneManager.LoadScene(startSceneName); }); }
        timerRunning = false;
        isGameOver = false;

        if (gameOverBlocker != null)
        {
            gameOverBlocker.gameObject.SetActive(true);
            gameOverBlocker.alpha = 0f;
            gameOverBlocker.blocksRaycasts = false;
            gameOverBlocker.interactable = false;
        }

        if (gameOverTxt != null) { gameOverTxt.gameObject.SetActive(false); }
    }

    void Update()
    {
        if (!isGameOver && timerRunning)
        {
            gameTimer += Time.deltaTime;
            gameTimerTxt.SetText(FormatMMSScc(gameTimer));
        }


        if (ghostTimer > 0.0f)
        {
            ghostTimer = Mathf.Max(0.0f, ghostTimer - Time.deltaTime);

            if (!ghostTimerTxt.gameObject.activeSelf)
            {
                ghostTimerTxt.gameObject.SetActive(true);
            }

            int secs = Mathf.CeilToInt(ghostTimer);
            ghostTimerTxt.SetText(secs.ToString());
        }

        else
        {
            if (ghostTimerTxt != null && ghostTimerTxt.gameObject.activeSelf)
            {
                ghostTimerTxt.gameObject.SetActive(false);
            }
        }
    }

    public void StartGameTimer()
    {
        timerRunning = true;
    }
    
    public void StopGameTimer()
    {
        timerRunning = false;
    }

    public void AddScore(int delta)
    {
        score = Mathf.Max(0, score + delta);
        UpdateScoreUI();
    }

    public void SetLevelName(string nameToShow)
    {
        levelNameTxt.SetText(nameToShow);
    }

    public void LoseLife()
    {
        if (lives <= 0) return;
        lives--;
        UpdateLivesUI();

        if (lives <= 0)
        {
            TriggerGameOver();
            OnGameOver?.Invoke();
        }
    }

    public void GainLife()
    {
        if (lives >= lifeIcons.Length) return;
        lives++;
        UpdateLivesUI();
    }

    public void StartGhostTimer(float durationSecs)
    {
        ghostTimer = Mathf.Max(0f, durationSecs);
        if (ghostTimer > 0f) { ghostTimerTxt.gameObject.SetActive(true); }
        else { ghostTimerTxt.gameObject.SetActive(false); }

        if (ghosts != null)
        {
            foreach (var g in ghosts)
            {
                if (g != null)
                {
                    g.SetScared(true, ghostTimer);
                    Debug.Log($"[HUD] Set {g.name} to SCARED for {ghostTimer} seconds");
                }
            }
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) { return; }
        isGameOver = true;

        StopGameTimer();

        if (gameOverBlocker != null)
        {
            gameOverBlocker.alpha = 1f;
            gameOverBlocker.blocksRaycasts = true;
            gameOverBlocker.interactable = true;
        }

        if (gameOverTxt != null)
        {
            gameOverTxt.gameObject.SetActive(true);
        }

        FreezeAllActors();
        SaveHighScore(score, gameTimer);
        StartCoroutine(ReturnToStartAfterDelay());
    }

    private void FreezeAllActors()
    {
        var player = FindObjectOfType<PacStudentController>();
        if (player != null) { player.SetFrozen(true); }

        var allGhosts = FindObjectsOfType<GhostController>();
        foreach (var g in allGhosts) { g.SetFrozen(true); }
    }

    private IEnumerator ReturnToStartAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDuration);
        SceneManager.LoadScene(startSceneName);
    }

    private void SaveHighScore(int currentScore, float currentTime)
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 9999999f);
        bool shouldSave = false;

        if (currentScore > bestScore)
        {
            shouldSave = true;
        }
        else if (currentScore == bestScore && currentTime < bestTime)
        {
            shouldSave = true;
        }

        if (shouldSave)
        {
            Debug.Log($"[HUD] Saving high score: score={currentScore}, time={currentTime}");
            PlayerPrefs.SetInt("BestScore", currentScore);
            PlayerPrefs.SetFloat("BestTime", currentTime);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log($"[HUD] NOT saving. current=({currentScore},{currentTime}) best=({bestScore},{bestTime})");
        }
    }


    private void UpdateLivesUI()
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            lifeIcons[i].enabled = (i < lives);
        }
    }

    private void UpdateScoreUI() => scoreTxt.SetText(score.ToString("D6"));

    private string FormatMMSScc(float t)
    {
        int cs = Mathf.FloorToInt(t * 100f);
        int m = cs / 6000;
        int s = (cs / 100) % 60;
        int cc = cs % 100;
        return $"{m:00}:{s:00}:{cc:00}";
    }
}
