using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject creditsScreen;
    [SerializeField] private GameObject settingsMenu; 
    [SerializeField] private Transform[] lightPositions;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] public GameObject toolTip;
    
    public static GameManager instance;
    private int guidedTransform = 0;

    private bool isFlipped = false;
    private bool wasFlipped = false;

    private bool isPaused = true;
    private PlayerController playerController;
    
    void Awake()
    {
        instance = this;
        playerController = FindAnyObjectByType<PlayerController>();
        ShowCursor();
    }

    void Start()
    {
        
        isPaused = true;
        Time.timeScale = 0;
        
        
        ShowCursor();
        
        
        
            mainMenu.SetActive(true);
            settingsMenu.SetActive(false);
    }
    
    void Update()
    {
        Time.timeScale = 1.0f;
        isFlipped = playerController.GetFlip();
        if(isFlipped != wasFlipped ) 
        {
            wasFlipped = isFlipped;
            GameObject[] lights = GameObject.FindGameObjectsWithTag("LightPlaced");

            for(int i = 0; i < lights.Length; ++i) 
            {
                lights[i].GetComponent<LightIntensity>().InvertLight();
            }
        }

        if (isPaused)
        {
            ShowCursor();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);
            ShowCursor();
        }
    }

    public void Win()
    {
        isPaused = !isPaused;
        winScreen.gameObject.SetActive(true);
        StartCoroutine(wait());
        
    }

    private IEnumerator wait()
    {
         yield return new WaitForSeconds(1.0f);
         creditsScreen.gameObject.SetActive(true);
    }

    private void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    private void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void OnStartButtonClick()
    {
        StateResume();
    }
    
    public void OpenSettingsMenu()
    {
        if (mainMenu != null)
        {
            mainMenu.SetActive(false);
        }
        
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(true);
        }
    }
    
    public void CloseSettingsMenu()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }
        
        if (mainMenu != null)
        {
            mainMenu.SetActive(true);
        }
    }

    public void StatePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        ShowCursor();
        
        if (mainMenu != null)
        {
            mainMenu.SetActive(true);
        }
        
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }
        
    }

    public void StateResume()
    {
        isPaused = false;
        Time.timeScale = 1;
        HideCursor();
        
        if (mainMenu != null)
        {
            mainMenu.SetActive(false);
        }
        
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
    }

    public static GameManager GetInstance()
    {
        return instance;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public Transform GetLightGoToPosition()
    {
        guidedTransform++;
        if (guidedTransform == lightPositions.Length) guidedTransform = 0;
        Transform newLightPosition = lightPositions[guidedTransform];
        return newLightPosition;
    }

    public Transform GetPlayerPosition()
    {
        return player.transform;
    }

    public Transform GetLightCurrentPosition()
    {
        if (guidedTransform == lightPositions.Length) guidedTransform = 0;
        Transform current = lightPositions[guidedTransform];
        return current;
    }
    

    public bool IsPaused()
    {
        return isPaused;
    }

    public void ChangeLightToNextPosition()
    {
       
        if (guidedTransform == lightPositions.Length) guidedTransform = 0;
        guidedTransform++;
    }

}
