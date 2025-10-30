using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] private TMP_Text bestScoreTxt;
    [SerializeField] private TMP_Text bestTimeTxt;

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLevelMusic();
        }

        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);

        if (bestScoreTxt != null)
        {
            bestScoreTxt.text = bestScore.ToString("D6");
        }

        if (bestTimeTxt != null)
        {
            bestTimeTxt.text = FormatMMSScc(bestTime);
        }
    }
    public void LoadLevel1()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("Main Scene");
    }

    private string FormatMMSScc(float t)
    {
        int cs = Mathf.FloorToInt(t * 100f);
        int m = cs / 6000;
        int s = (cs / 100) % 60;
        int cc = cs % 100;
        return $"{m:00}:{s:00}:{cc:00}";
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("BestScore");
        PlayerPrefs.DeleteKey("BestTime");
        PlayerPrefs.Save();
    }
}
