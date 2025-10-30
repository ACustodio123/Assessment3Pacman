using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        // SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // if (Instance == this) { SceneManager.sceneLoaded -= OnSceneLoaded; }
    }

    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     if (scene.name == "Main Scene")
    //     {
    //         PlayLevelMusic();
    //     }
    // }

    void Start()
    {
        if (audioSource && audioSource.clip)
        {
            audioSource.Play();
        }
    }

    public void PlayLevelMusic()
    {
        if (musicSource && levelBackgroundMusic)
        {
            musicSource.clip = levelBackgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

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

    public void StopLevelMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

}
