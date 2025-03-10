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
    private GameObject[] walls;
    public bool debug = true;
    private Vector3 wallNormal = Vector3.zero;
    private GameObject nearestWall = null;
    Vector3 calculatedMoveDir = Vector3.zero;
    void Start()
    {
        originalTransform = referencePosition;
        walls = GameObject.FindGameObjectsWithTag("Wall");
    }

    void Update()
    {
        if (updatePosition)
        {
            MoveTowardsTarget();
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

    public void PathFinding()
    {
        FindNearestWall();
        calculatedMoveDir = CalculateFollowDirection();
        
    }

    void FindNearestWall()
    {
        float closestDistance = float.MaxValue;
        
        wallNormal = Vector3.zero;
        nearestWall = null;

        foreach (GameObject wall in walls)
        {
            Collider wallCollider = wall.GetComponent<Collider>();
            if (wallCollider == null) continue;
            
            Vector3 closestPoint = wallCollider.ClosestPoint(transform.position);
            float distance = Vector3.Distance(transform.position, closestPoint);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                
                // find the normal of the direction of the closest point on the wall
                wallNormal = (transform.position - closestPoint).normalized;

            }

            if (nearestWall != null && closestDistance < avoidanceAmount)
            {
                // This will force the light away from the wall
                transform.position = transform.position + wallNormal * (avoidanceAmount - closestDistance);
            }
            
        }
    }

    Vector3 CalculateFollowDirection()
    {
        // direction along the wall
        Vector3 alongWall = Vector3.Cross(wallNormal, Vector3.up).normalized;
        
        Vector3 targetPostion = GameManager.GetInstance().GetLightCurrentPosition().position;

        float dotAgainstTheWall = Vector3.Dot(alongWall, targetPostion - transform.position);
        if (dotAgainstTheWall < 0)
        {
            alongWall = -alongWall;
        }
        
        // Weight the direction towards the target position
        float distance = Vector3.Distance(transform.position, targetPostion);

        float targetWeight = Mathf.Clamp01(1.0f / distance);
        
        Vector3 blendedDirection = Vector3.Lerp(alongWall, targetDirection, targetWeight);
        
        return blendedDirection.normalized;
    }

    void MoveTowardsTarget()
    {
        transform.position = calculatedMoveDir * (speed * Time.deltaTime);
    }
}