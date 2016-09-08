using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public AudioSource _effect_audio;
    public AudioSource _music_audio;
    public AudioSource _computer_audio;
    //public AudioClip[] _effects;
    public static SoundManager _instance = null;
   
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void _play_effect_once(AudioClip clip)
    {
        _effect_audio.clip = clip;
        _effect_audio.Play();
    }

    public void _play_computer(AudioClip clip)
    {
        _computer_audio.clip = clip;
        _computer_audio.Play();
    }

    public void _play_music(AudioClip clip)
    {
        _music_audio.clip = clip;
        _music_audio.Play();
    }
}