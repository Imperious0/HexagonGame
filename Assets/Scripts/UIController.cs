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

    [SerializeField]
    private AudioSource musicPlayer;
    [SerializeField]
    private AudioSource sfxPlayer;

    [SerializeField]
    private AudioClip[] sfxList;

    private void Start()
    {
        musicPlayer.mute = PlayerPrefs.GetInt("Music", 1) == 0 ? true : false;
        sfxPlayer.mute = PlayerPrefs.GetInt("Sfx", 1) == 0 ? true : false;

        musicBtn.GetComponentInChildren<TextMeshProUGUI>().text = PlayerPrefs.GetInt("Music", 1) == 0 ? "OFF" : "ON";
        sfxBtn.GetComponentInChildren<TextMeshProUGUI>().text = PlayerPrefs.GetInt("Sfx", 1) == 0 ? "OFF" : "ON";
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
        Camera.main.GetComponent<UIController>().playSfx(SfxTypes.Button);
        SceneManager.LoadScene(0);
    }

    public void showMenu()
    {
        Camera.main.GetComponent<UIController>().playSfx(SfxTypes.Button);
        if (!menuGUI.GetComponent<Animator>().GetBool("showMenu"))
            menuGUI.GetComponent<Animator>().SetBool("showMenu", true);
        else
            menuGUI.GetComponent<Animator>().SetBool("showMenu", false);
    }
    public void showSettingMenu() 
    {
        Camera.main.GetComponent<UIController>().playSfx(SfxTypes.Button);
        if (!menuGUI.GetComponent<Animator>().GetBool("showSettingsMenu"))
            menuGUI.GetComponent<Animator>().SetBool("showSettingsMenu", true);
        else
            menuGUI.GetComponent<Animator>().SetBool("showSettingsMenu", false);
    }
    public void showGGMenu()
    {
        Camera.main.GetComponent<UIController>().playSfx(SfxTypes.Button);
        if (!ggGUI.GetComponent<Animator>().GetBool("showMenu"))
            ggGUI.GetComponent<Animator>().SetBool("showMenu", true);
        else
            ggGUI.GetComponent<Animator>().SetBool("showMenu", false);
    }
    public void toggleSfx() 
    {
        Camera.main.GetComponent<UIController>().playSfx(SfxTypes.Button);
        if (PlayerPrefs.GetInt("Sfx", 1) == 1)
        {
            PlayerPrefs.SetInt("Sfx", 0);
            PlayerPrefs.Save();
            sfxPlayer.mute = true;
        }
        else
        {
            PlayerPrefs.SetInt("Sfx", 1);
            PlayerPrefs.Save();
            sfxPlayer.mute = false;
        }
        sfxBtn.GetComponentInChildren<TextMeshProUGUI>().text = PlayerPrefs.GetInt("Sfx", 1) == 0 ? "OFF" : "ON";
    }
    public void toggleMusic() 
    {
        Camera.main.GetComponent<UIController>().playSfx(SfxTypes.Button);
        if (PlayerPrefs.GetInt("Music", 1) == 1)
        {
            PlayerPrefs.SetInt("Music", 0);
            PlayerPrefs.Save();
            musicPlayer.mute = true;
        }
        else
        {
            PlayerPrefs.SetInt("Music", 1);
            PlayerPrefs.Save();
            musicPlayer.mute = false;
        }
        musicBtn.GetComponentInChildren<TextMeshProUGUI>().text = PlayerPrefs.GetInt("Music", 1) == 0 ? "OFF" : "ON";
    }
    public void playSfx(SfxTypes types) 
    {
        switch (types)
        {
            case SfxTypes.Button:
                sfxPlayer.PlayOneShot(sfxList[0]);
                break;
            case SfxTypes.Select:
                sfxPlayer.PlayOneShot(sfxList[1]);
                break;
            case SfxTypes.Bubble:
                sfxPlayer.PlayOneShot(sfxList[2]);
                break;
            case SfxTypes.BubbleScs:
                sfxPlayer.PlayOneShot(sfxList[3]);
                break;
        }
    }

    public void exitGame()
    {
        Application.Quit();
    }

}
public enum SfxTypes { Button, Select, Bubble, BubbleScs }