using UnityEngine;

public class DetectPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject lightSource;
    private LightFollow lightFollow;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
          // lightFollow.ChangePosition();
           GameManager.GetInstance().ChangeLightToNextPosition();
           BoxCollider boxCollider = this.GetComponent<BoxCollider>();
           boxCollider.enabled = false;
        }
    }

    void Start()
    {
        lightFollow = lightSource.GetComponentInChildren<LightFollow>();
        
    }
}
