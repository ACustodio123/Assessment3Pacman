using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    [SerializeField] private AudioClip LevelBackgroundMusic;
    [SerializeField] private HUDManager hUDManager;
    [SerializeField] private PacStudentController pacstudent;
    [SerializeField] private GameObject[] ghosts;
    void Start()
    {
        if (pacstudent != null)
            pacstudent.enabled = false;

        if (hUDManager != null)
            hUDManager.enabled = false;

        foreach (GameObject g in ghosts)
            if (g != null) g.SetActive(false);

        if (audioSource != null)
        {
            audioSource.Play();
            Invoke(nameof(PlayLevelBackgroundMusic), audioSource.clip.length);
        }
    }

    private void PlayLevelBackgroundMusic()
    {
        if (audioSource != null && LevelBackgroundMusic != null)
        {
            audioSource.clip = LevelBackgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        if (pacstudent != null)
            pacstudent.enabled = true;

        if (hUDManager != null)
            hUDManager.enabled = true;

        foreach (GameObject g in ghosts)
            if (g != null) g.SetActive(true);
    }
}
