using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    [SerializeField] private AudioClip LevelBackgroundMusic;
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(PlayLevelBackgroundMusic), audioSource.clip.length);
    }

    void PlayLevelBackgroundMusic() 
    {
        audioSource.clip = LevelBackgroundMusic;
        audioSource.loop = true;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
