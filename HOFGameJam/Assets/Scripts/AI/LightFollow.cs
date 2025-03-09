using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class LightFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float timeCheckDelayForPosition = 0.05f;
    [SerializeField] private float smoothTime;
    private Transform target;
    private float timeToUpdate = 0;
    private bool updatePosition = false;
    private Transform referencePosition;
    private Vector3 targetDirection;
    private float distanceToTarget;
    private bool isMoving = false;
    private Transform currentOrbitingTransform;
    private Transform originalTransform;
    private Vector3 orbitAround;
    private Vector3 velocity = Vector3.zero;
 
    

    void Start()
    {
        originalTransform = referencePosition;
    }

    void Update()
    {
        if (updatePosition)
        {
            LerpToTarget();
        }

        if (timeToUpdate >= timeCheckDelayForPosition && !updatePosition && !isMoving)
        {
            CenterChangePoint();
            timeToUpdate = 0;
        }

        if (isMoving)
        {
            UpdateStoppedLocation(orbitAround);
        }

        timeToUpdate += Time.deltaTime;
    }

    public void ChangePosition()
    {
        updatePosition = true;
        referencePosition = GameManager.GetInstance().GetLightGoToPosition();
        targetDirection = (referencePosition.position - transform.position).normalized;
        distanceToTarget = Vector3.Distance(referencePosition.position, transform.position);
    }

    void LerpToTarget()
    {
        if (Vector3.Distance(transform.position, referencePosition.position) >= 1f)
        {
            float randomYOffet = Random.Range(-2, 2);
            Vector3 directionTowardsTarget = new Vector3(referencePosition.position.x,
                referencePosition.position.y + randomYOffet, referencePosition.position.z);
            transform.position =
                Vector3.MoveTowards(transform.position, directionTowardsTarget, smoothTime * 0.5f);
        }
        else
        {
            updatePosition = false;
        }
    }

    void UpdateStoppedLocation(Vector3 targetPosition)
    {
        float randomYOffet = Random.Range(-0.5f, 0.5f);
        if (Vector3.Distance(transform.position, targetPosition) >= 1f)
        {
            
            Vector3 directionTowardsTarget = new Vector3(targetPosition.x,
                targetPosition.y + randomYOffet, targetPosition.z);
            transform.position =
                Vector3.SmoothDamp(transform.position, directionTowardsTarget, ref velocity, smoothTime);
        }

        else isMoving = false;
    }

    void CenterChangePoint()
    {
        Transform reference = GameManager.GetInstance().GetLightCurrentPosition();
        Vector3 targetLocation = Random.insideUnitCircle * 3f;
        targetLocation += reference.position;
        isMoving = true;
        orbitAround = targetLocation;
        UpdateStoppedLocation(targetLocation);
    }
}