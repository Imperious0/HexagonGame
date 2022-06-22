
using UnityEngine;

public interface IAudioSource
{
    void muteSource(bool isMuted);
    void toggleSourceStatus();
    AudioSource Source { get; }
}