using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float musicVolume = 0.25f;
    [SerializeField] private float soundEffectsVolume = 0.5f;
    [SerializeField] private AudioClip[] ambientSounds;
    [SerializeField] private AudioClip[] menuMusic;
    [SerializeField] private AudioClip[] gameMusic;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float timeDelayForMenuCheck;
    [SerializeField] private AudioSource audioController;
    private static SoundManager instance;
    private float timeCounter;
    private bool isPlayingMenuMusic = false;
    
    void Start()
    {
        instance = this;
        timeCounter = timeDelayForMenuCheck;
    }

    void Update()
    {
        if (timeCounter >= timeDelayForMenuCheck && GameManager.GetInstance().IsPaused())
        {
            PlayMainMenuMusic();
            timeCounter = 0;
        }
        

        timeCounter += Time.deltaTime;
    }

    private void PlayMainMenuMusic()
    {
        isPlayingMenuMusic = true;
        audioController.PlayOneShot(menuMusic[Random.Range(0, menuMusic.Length)], musicVolume);
        
    }

    // Update is called once per frame

}
