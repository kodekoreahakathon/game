using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            return _instance;
        }
    }
    
    AudioSource audioSource;
    
    public AudioClip backcgroundAudioClip;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = this.GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartBackgroundMusic(backcgroundAudioClip);
    }

    void StartBackgroundMusic(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Stop();
        audioSource.loop = true;
        audioSource.Play();
    }
}
