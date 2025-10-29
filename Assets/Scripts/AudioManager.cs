using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource audioSource;   
    [SerializeField] private AudioSource musicSource;  

    [Header("Music Clips")]
    [SerializeField] private AudioClip levelBackgroundMusic;
    [SerializeField] private AudioClip scaredMusic;

    [Header("Scene Refs (optional gating)")]
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private PacStudentController pacstudent;
    [SerializeField] private GameObject[] ghosts;

    public static AudioManager Instance { get; private set; }

    private float scaredEndTime = 0f;
    private Coroutine scaredRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (pacstudent) pacstudent.enabled = false;
        if (hudManager) hudManager.enabled = false;
        if (ghosts != null)
            foreach (var g in ghosts) if (g) g.SetActive(false);

        if (audioSource && audioSource.clip)
        {
            audioSource.Play();
            Invoke(nameof(PlayLevelMusic), audioSource.clip.length);
        }
        else
        {
            PlayLevelMusic();
        }
    }

    private void PlayLevelMusic()
    {
        if (musicSource && levelBackgroundMusic)
        {
            musicSource.clip = levelBackgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        if (pacstudent) pacstudent.enabled = true;
        if (hudManager) hudManager.enabled = true;
        if (ghosts != null)
            foreach (var g in ghosts) if (g) g.SetActive(true);
    }

    public void TriggerScaredMusic(float duration)
    {
        if (!musicSource || !scaredMusic) return;

        float now = Time.time;
        scaredEndTime = Mathf.Max(scaredEndTime, now + Mathf.Max(0.01f, duration));

        if (scaredRoutine == null)
            scaredRoutine = StartCoroutine(ScaredMusicLoop());
    }

    private IEnumerator ScaredMusicLoop()
    {
        musicSource.clip = scaredMusic;
        musicSource.loop = true;
        musicSource.Play();

        while (Time.time < scaredEndTime)
            yield return null;

        if (levelBackgroundMusic)
        {
            musicSource.clip = levelBackgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        scaredRoutine = null;
    }
}
