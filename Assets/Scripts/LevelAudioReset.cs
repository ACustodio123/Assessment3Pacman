using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAudioReset : MonoBehaviour
{
    private void Awake()
    {
        AudioListener.pause = false;
        AudioListener.volume = 1f;

        AudioSource[] allSources = FindObjectsOfType<AudioSource>(true);
        foreach (var src in allSources)
        {
            src.enabled = true;
            src.mute = false;
        }
    }
}
