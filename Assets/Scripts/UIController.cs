using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private GameObject menuGUI;
    [SerializeField]
    private GameObject ggGUI;

    [SerializeField]
    private GameObject musicBtn;
    [SerializeField]
    private GameObject sfxBtn;

    IAudioSource musicSource;
    IAudioSource sfxSource;

    [SerializeField]
    private AudioClip[] sfxList;

    private void Start()
    {
        musicSource = MusicManager.Instance.MusicHandler;
        sfxSource = MusicManager.Instance.SfxHandler;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            Application.Quit();
        }
    }

    public void startNewGame() 
    {
        playSfx(SfxTypes.Button);
        SceneManager.LoadScene(0);
    }

    public void showMenu()
    {
        playSfx(SfxTypes.Button);
        Animator anim = menuGUI.GetComponent<Animator>();
        anim.SetBool("showMenu", !anim.GetBool("showMenu"));
    }
    public void showSettingMenu() 
    {
        playSfx(SfxTypes.Button);
        Animator anim = menuGUI.GetComponent<Animator>();
        anim.SetBool("showSettingsMenu", !anim.GetBool("showSettingsMenu"));
    }
    public void showGGMenu()
    {
        playSfx(SfxTypes.Button);
        Animator anim = ggGUI.GetComponent<Animator>();
        anim.SetBool("showMenu", !anim.GetBool("showMenu"));
    }
    public void toggleSfx() 
    {
        playSfx(SfxTypes.Button);
        sfxSource.toggleSourceStatus();
    }
    public void toggleMusic() 
    {
        playSfx(SfxTypes.Button);
        musicSource.toggleSourceStatus();
    }
    public void playSfx(SfxTypes types) 
    {
        switch (types)
        {
            case SfxTypes.Button:
                sfxSource.Source.PlayOneShot(sfxList[0]);
                break;
            case SfxTypes.Select:
                sfxSource.Source.PlayOneShot(sfxList[1]);
                break;
            case SfxTypes.Bubble:
                sfxSource.Source.PlayOneShot(sfxList[2]);
                break;
            case SfxTypes.BubbleScs:
                sfxSource.Source.PlayOneShot(sfxList[3]);
                break;
        }
    }

    public void exitGame()
    {
        Application.Quit();
    }

}
public enum SfxTypes { Button, Select, Bubble, BubbleScs }