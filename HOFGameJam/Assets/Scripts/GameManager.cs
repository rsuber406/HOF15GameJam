using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject creditsScreen;
    [SerializeField] private Transform[] lightPositions;
    [SerializeField] private Button startButton;
    public static GameManager instance;

    private int guidedTransform = 0;
    private bool isPaused = true;
    
    void Awake()
    {
        instance = this;
        
        ShowCursor();
    }

    void Start()
    {
        Time.timeScale = 0;
        
        ShowCursor();
        
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClick);
        }
       
    }
    
    void Update()
    {
        if (isPaused)
        {
            ShowCursor();
        }
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

    public void StatePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        ShowCursor();
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
    }

    public static GameManager GetInstance()
    {
        return instance;
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
        Transform current = lightPositions[guidedTransform];
        return current;
    }
}
