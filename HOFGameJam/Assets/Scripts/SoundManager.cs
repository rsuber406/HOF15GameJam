using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private float timeDelayForGameCheck;
    [SerializeField] private AudioSource audioController;
    [SerializeField] private Slider volumeSlider;
    private static SoundManager instance;
    private float timeCounterMenu;
    private float timeCounterGame = 0f;
    private bool isPlayingMenuMusic = false;
    private bool isPlayingGameMusic = false;
    private bool isPlaying = false;

    void Start()
    {
        instance = this;
        timeCounterMenu = timeDelayForMenuCheck;
        
        volumeSlider.value = AudioListener.volume;
        
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }
    void ChangeVolume(float volume)
    {
        AudioListener.volume = volume;
    }
    

    void Update()
    {
        if (GameManager.GetInstance().IsPaused() && timeCounterMenu >= timeDelayForMenuCheck)
        {
            if (!isPlayingMenuMusic) StartCoroutine(PlayMainMenuMusic());
            timeCounterMenu = 0f;
        }

        timeCounterMenu += Time.deltaTime;

        if (timeCounterGame >= timeDelayForGameCheck && !GameManager.GetInstance().IsPaused())
        {
            if (!isPlayingGameMusic) StartCoroutine(PlayGameMusic());
            timeCounterGame = 0f;
        }

        timeCounterGame += Time.deltaTime;

        //Ok these are both just delays so that the ambiant sound does not play as soon as you start
        //No changes needed

    }

    private IEnumerator PlayMainMenuMusic()
    {
        isPlayingMenuMusic = true;
        int randomTrack = Random.Range(0, menuMusic.Length);
        audioController.PlayOneShot(menuMusic[randomTrack], musicVolume);
        yield return new WaitForSeconds(menuMusic[randomTrack].length);
        isPlayingMenuMusic = false;
    }
    

    public IEnumerator PlayGameMusic()
    {
        isPlayingGameMusic = true;
        int randomTrack = Random.Range(0, gameMusic.Length);
        audioController.PlayOneShot(gameMusic[randomTrack], musicVolume);
        yield return new WaitForSeconds(gameMusic[randomTrack].length);
        isPlayingGameMusic = false;
    }

    private void DetermineVolumeDifferential()
    {
        // get differential for master volume
    }

    public void SetMusicVolume(float volume)
    { 
        musicVolume = volume;
    }

    // Update is called once per frame
}