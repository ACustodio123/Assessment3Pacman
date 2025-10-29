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

    private int score = 0;
    private int lives;
    private float gameTimer = 0.0f;
    private float ghostTimer = 0.0f;

    void Awake()
    {
        lives = Mathf.Clamp(3, 0, lifeIcons != null ? lifeIcons.Length : 3);
        UpdateLivesUI();
        UpdateScoreUI();
        gameTimerTxt.SetText("00:00:00");
        levelNameTxt.SetText(levelDisplayName);

        if (ghostTimerTxt != null)
        {
            ghostTimerTxt.gameObject.SetActive(false);
        }

        if (exitBtn != null) 
        {
            exitBtn.onClick.AddListener(() => { SceneManager.LoadScene(startSceneName); });
        }
    }

    void Update()
    {
        gameTimer += Time.deltaTime;
        gameTimerTxt.SetText(FormatMMSScc(gameTimer));

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
