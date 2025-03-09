using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject player;

    [SerializeField] private GameObject lossScreen;

    [SerializeField] private GameObject winScreen;

    [SerializeField] private GameObject creditsScreen;
    
    [SerializeField]  private Transform [] lightPositions;

    private static GameManager instance;

    private int guidedTransform = 0;
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
