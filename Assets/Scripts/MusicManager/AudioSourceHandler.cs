using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceHandler : MonoBehaviour, IAudioSource
{
    [SerializeField]
    private AudioSource _source;
    [SerializeField]
    private TextMeshProUGUI _uiText;
    [SerializeField]
    private AudioType _type;

    public AudioSource Source { get => _source; }

    private void Awake()
    {
        //Initialize 
        muteSource(!getPlayerPrefs());
    }
    public void muteSource(bool isMuted)
    {
        _source.mute = isMuted;
        setPlayerPrefs(isMuted);
        setIfTextBinded(isMuted);
    }

    public void toggleSourceStatus()
    {
        bool isMuted = !getPlayerPrefs();

        muteSource(!isMuted);
    }

    private void setIfTextBinded(bool isMuted)
    {
        if(_uiText != null)
        {
            _uiText.text = isMuted ? "OFF" : "ON";
        }
    }
    private void setPlayerPrefs(bool isMuted)
    {
        switch (_type)
        {
            case AudioType.MUSIC:
                PlayerPrefs.SetInt("MUSIC", isMuted ? 0 : 1);
                break;
            case AudioType.SFX:
                PlayerPrefs.SetInt("SFX", isMuted ? 0 : 1);
                break;
            default:
                break;
        }
    }
    private bool getPlayerPrefs()
    {
        int status = 1;
        switch (_type)
        {
            case AudioType.MUSIC:
                status = PlayerPrefs.GetInt("MUSIC", 1);
                break;
            case AudioType.SFX:
                status = PlayerPrefs.GetInt("SFX", 1);
                break;
            default:
                status = 1;
                break;
        }
        return (status == 1);
    }
    private enum AudioType { MUSIC, SFX };
}
