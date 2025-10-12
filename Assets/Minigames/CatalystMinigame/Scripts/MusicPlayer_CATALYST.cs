using UnityEngine;

public class MusicPlayer_CATALYST : MonoBehaviour, MinigameSubscriber
{
    AudioSource source;

    public void OnMinigameStart()
    {
        source.Play();
    }

    public void OnTimerEnd()
    {
        
    }

    void Start()
    {
        source = GetComponent<AudioSource>();
        MinigameManager.Subscribe(this);
    }

    
}
