using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundStartUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup blocker;
    [SerializeField] private TextMeshProUGUI countdownTxt;
    [SerializeField] private HUDManager hudManager;

    [Header("Audio")]
    [SerializeField] private AudioSource startSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip normalMusic;

    [Header("Timing")]
    [SerializeField] private float stepTime = 1f;

    private void Start()
    {
        ShowUI(true);
        StartCoroutine(DoCountdown());
    }

    private IEnumerator DoCountdown()
    {
        if (startSource != null && startSource.clip != null)
        {
            if (!startSource.enabled)
                startSource.enabled = true;
            if (!startSource.gameObject.activeInHierarchy)
                startSource.gameObject.SetActive(true);
            startSource.Play();
        }

        yield return new WaitForSeconds(1.5f);

        SetText("3");
        yield return new WaitForSeconds(stepTime);

        SetText("2");
        yield return new WaitForSeconds(stepTime);

        SetText("1");
        yield return new WaitForSeconds(stepTime);

        SetText("GO!");
        yield return new WaitForSeconds(stepTime);

        ShowUI(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        AudioManager.Instance?.PlayLevelMusic();

        if (hudManager != null)
        {
            hudManager.StartGameTimer();
        }

        var player = FindObjectOfType<PacStudentController>();
        if (player != null)
            player.SetFrozen(false);

        var ghosts = FindObjectsOfType<GhostController>();
        foreach (var g in ghosts)
            g.SetFrozen(false);
    }

    private void ShowUI(bool show)
    {
        if (blocker != null)
        {
            blocker.alpha = show ? 1 : 0;
            blocker.blocksRaycasts = show;
            blocker.interactable = show;
        }

        if (countdownTxt != null)
        {
            countdownTxt.gameObject.SetActive(show);
        }
    }

    private void SetText(string s)
    {
        if (countdownTxt != null)
        {
            countdownTxt.text = s;
        }
    }
}
