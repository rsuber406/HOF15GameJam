using UnityEngine;

using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private PlayerController playerController;
    
    [Header("Camera Settings")]
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float sensitivity = 200f;
    [SerializeField] private float smoothTime = 0.1f; 
    [SerializeField] private bool invertYAxis = false;
    [SerializeField] private Vector2 verticalLimits = new Vector2(-30f, 60f);
    
    [Header("Collision")]
    [SerializeField] private float collisionOffset = 0.2f;
    [SerializeField] private bool enableCollision = true;
    [SerializeField] private LayerMask collisionLayers;
    
    
    private float currentX, currentY;
    private Vector3 currentVelocity; 
    private float cameraRoll = 0f;
    private bool wasGravityInverted;
    
    private void Start()
    {
            target = playerController.transform;
        
            playerController = target.GetComponent<PlayerController>();
        
        currentX = transform.eulerAngles.y;
        currentY = transform.eulerAngles.x;
        wasGravityInverted = playerController.IsGravityInverted;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void Update()
    {
        if (target == null || playerController == null)
            return;
        
        if (!playerController.IsTransitioning)
        {
            HandleInput();
        }
        
        UpdateCameraPosition();
    }
    
    private void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        
        if (invertYAxis)
            mouseY = -mouseY;
            
        if (playerController.IsGravityInverted)
        {
            mouseX = -mouseX;
            mouseY = -mouseY;
        }
        currentX += mouseX;
        currentY -= mouseY;
        
        currentY = Mathf.Clamp(currentY, verticalLimits.x, verticalLimits.y);
    }
    
    private void UpdateCameraPosition()
    {
        float targetRoll = playerController.IsGravityInverted ? 180f : 0f;
        float gravityFactor = playerController.CurrentGravityFactor;
        
        if (playerController.IsTransitioning)
        {
            float progress = playerController.GravityTransitionTimer / playerController.GravityTransitionDuration;
            float curveProgress = progress * progress * (3f - 2f * progress); 
            
            if (!playerController.IsGravityInverted)
                cameraRoll = Mathf.Lerp(0f, 180f, curveProgress);
            else
                cameraRoll = Mathf.Lerp(180f, 0f, curveProgress);
        }
        else
        {
            cameraRoll = Mathf.Lerp(cameraRoll, targetRoll, Time.deltaTime * 3f);
        }
        
        Quaternion rotation = Quaternion.Euler(currentY, currentX, cameraRoll);
        
        Vector3 direction = rotation * Vector3.back;
        Vector3 heightOffset = Vector3.up * height * gravityFactor;
        Vector3 targetPosition = target.position + heightOffset;
        Vector3 desiredPosition = targetPosition + direction * distance;
        
        if (enableCollision)
        {
            RaycastHit hit;
            if (Physics.Raycast(targetPosition, direction, out hit, distance, collisionLayers))
            {
                float adjustedDistance = hit.distance - collisionOffset;
                desiredPosition = targetPosition + direction * adjustedDistance;
            }
        }
        
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime / smoothTime);
        
        if (playerController.PlayerModel != null && !playerController.IsTransitioning)
        {
            float yRotation = currentX;
            
            if (playerController.IsGravityInverted)
                playerController.PlayerModel.localRotation = Quaternion.Euler(180f, -yRotation, 0f);
            else
                playerController.PlayerModel.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
    
    public void NotifyGravityInversionStarted() { }
    public void NotifyGravityInversionCompleted() { }
}