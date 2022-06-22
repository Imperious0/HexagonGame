using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    [SerializeField]
    private AudioSourceHandler _musicHandler;
    [SerializeField]
    private AudioSourceHandler _sfxHandler;

    public IAudioSource MusicHandler { get => _musicHandler; }
    public IAudioSource SfxHandler { get => _sfxHandler; }

    public static MusicManager Instance { get { return instance; } }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
}
