using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class LightFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   [SerializeField] private float speed = 1.5f;
    private Transform target;
    private float timeCheckDelayForPosition = 0.05f;
    private float timeToUpdate = 0;
    private bool updatePosition = false;
    private Transform referencePosition;
    private Vector3 targetDirection;
    private float distanceToTarget;
    void Start()
    {
        timeToUpdate = timeCheckDelayForPosition;
    }

    void Update()
    {
        if (updatePosition)
        {
            LerpToTarget();
        }
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
            Vector3 directionTowardsTarget = new Vector3(referencePosition.position.x, referencePosition.position.y + randomYOffet, referencePosition.position.z);
            transform.position = Vector3.MoveTowards(transform.position, directionTowardsTarget, Time.deltaTime * speed);
        }
        else updatePosition = false;
    }



}
