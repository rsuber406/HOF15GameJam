using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class LightFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float timeCheckDelayForPosition = 0.05f;
    [SerializeField] private float smoothTime;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float avoidanceAmount;
    [SerializeField] private float wallDetectionAmount;

    [Header("Obstacle Avoidance")] 
    [SerializeField] private float avoidanceDistance;

    [SerializeField] private float avoidanceForce;
    [SerializeField] private int numberOfRays;
    [SerializeField] private float rayAngle;
    private Transform target;
    private float timeToUpdate = 0;
    private bool updatePosition = false;
    private Transform referencePosition;
    private Vector3 targetDirection;
    private float distanceToTarget;
    private bool isMoving = false;
    private Transform currentOrbitingTransform;
    private Vector3 orbitAround;
    public bool debug = true;
    private GameObject nearestWall = null;
    private Vector3 avoidanceDirection;
    private int rayHits = 0;
    
    void Start()
    {
        avoidanceDirection = Vector3.zero;
        
    }

    void Update()
    {
        if (updatePosition)
        {
           PathFinding();
        }

        if (timeToUpdate >= timeCheckDelayForPosition && !updatePosition && !isMoving)
        {
            CenterChangePoint();
            timeToUpdate = 0;
        }

        if (isMoving&& !updatePosition)
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
        GameManager.GetInstance().ChangeLightToNextPosition();
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
           // transform.position = Vector3.SmoothDamp(transform.position, directionTowardsTarget, ref velocity, smoothTime);
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

    void PathFinding()
    {
        Transform target = GameManager.GetInstance().GetLightCurrentPosition();
        Debug.Log(target.position);
        Vector3 targetDirection = (target.position - transform.position).normalized;
        
        avoidanceDirection = CalculateAvoidanceDirection();

        Vector3 finalDirection = (targetDirection + avoidanceDirection  * avoidanceForce).normalized;

        Debug.DrawRay(transform.position, finalDirection, Color.red);
        if (Vector3.Distance(transform.position, target.position) > 1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(finalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
            
            transform.position +=  transform.forward * speed * Time.deltaTime;
        }
        else
        {
            updatePosition = false;
        }
    }

    Vector3 CalculateAvoidanceDirection()
    {
        Vector3 avoidDir = Vector3.zero;

        int obstacleMask = LayerMask.GetMask("Wall");
        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = (i * (360f / numberOfRays)) - rayAngle * 0.5f;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 direction = rotation * transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, avoidanceDistance, obstacleMask))
            {
                float weight = 1.0f - (hit.distance / avoidanceDistance);
                Vector3 avoidanceVector = -direction.normalized * weight;
                
                avoidDir += avoidanceVector;
                rayHits++;
            }
            
            
        }

        RaycastHit frontHit;
        if (Physics.Raycast(transform.position, transform.forward, out frontHit, avoidanceDistance, obstacleMask))
        {
            float weight = 1.0f - (frontHit.distance / avoidanceDistance);
            avoidDir += -transform.forward * (weight * 2);
        }
        return avoidDir.normalized;
    }

}