using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform cameraTransform;
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private int maxJumps = 1;
    [SerializeField] private float gravityStrength = 9.81f;
    [SerializeField] private float cameraInversionSpeed = 5f;
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private bool showGroundCheckDebug = true;
    
    [SerializeField] private Vector3 initialCheckpoint = Vector3.zero;
    [SerializeField] private float fallDeathY = 100f;
    
    [SerializeField] private KeyCode gravityInvertKey = KeyCode.G;
    
    private Vector3 velocity;
    private Vector3 lastValidCheckpoint;
    private int jumpCount;
    private bool isGrounded;
    private bool isGravityInverted;
    private Quaternion targetCameraRotation;
    
    
    private void Start()
    {
        lastValidCheckpoint = initialCheckpoint;
        targetCameraRotation = cameraTransform.localRotation;
        
            controller = GetComponent<CharacterController>();
            
    }
    
    private void Update()
    {
        isGrounded = CheckGrounded();
        
        HandleGravityInversion();
        HandleMovement();
        HandleJumping();
        
        ApplyGravity();
        controller.Move(velocity * Time.deltaTime);
        
        UpdateCameraRotation();
        
        CheckFallDeath();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            lastValidCheckpoint = transform.position;
        }
    }

    public bool GetFlip()
    {
        return isGravityInverted;
    }
    
    
    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
      
        
        Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = cameraTransform.right;
        Vector3 moveDirection = (horizontalInput * cameraRight) + (verticalInput * cameraForward);
        if (moveDirection.magnitude > 0.1f)
        {
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
    }
    private void HandleJumping()
    {
        if (isGravityInverted && velocity.y <= 0.1f || !isGravityInverted && velocity.y >= -0.1f)
        {
            jumpCount = 0;
        }
        
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            float jumpVelocity = Mathf.Sqrt(2 * gravityStrength * jumpHeight);
            velocity.y = jumpVelocity * (isGravityInverted ? -1 : 1);
            jumpCount++;
        }
    }
    
    private void ApplyGravity()
    {
        if (isGrounded && velocity.y * (isGravityInverted ? -1 : 1) < 0)
        {
            velocity.y = -0.5f * (isGravityInverted ? -1 : 1);
        }
        else
        {
            int gravityDirection = isGravityInverted ? -1 : 1;
            velocity.y -= gravityStrength * gravityDirection * Time.deltaTime;
        }
    }
    private void HandleGravityInversion()
    {
        if (Input.GetKeyDown(gravityInvertKey))
        {
            ToggleGravity();
        }
    }
    
    private void ToggleGravity()
    {
        isGravityInverted = !isGravityInverted;
        playerModel.localRotation = Quaternion.Euler(isGravityInverted ? 180f : 0f, 0f, 0f);
        targetCameraRotation = Quaternion.Euler(isGravityInverted ? 180f : 0f, 0f, 0f);
        velocity = Vector3.zero;
        velocity.y = 0.2f * (isGravityInverted ? -1 : 1);
        
        jumpCount = 0;
        
    }
    
    private void UpdateCameraRotation()
    {
        
        cameraTransform.localRotation = Quaternion.Slerp(
            cameraTransform.localRotation,
            targetCameraRotation,
            cameraInversionSpeed * Time.deltaTime
        );
    }
    
    private bool CheckGrounded()
    {
       
        Vector3 rayDirection = isGravityInverted ? Vector3.up : Vector3.down;
        
       
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider == null)
        {
            
            playerCollider = controller;
        }
        
        
        Vector3 rayOrigin = transform.position;
        if (isGravityInverted)
        {
            rayOrigin.y = playerCollider.bounds.max.y - 0.05f; 
        }
        else
        {
            rayOrigin.y = playerCollider.bounds.min.y + 0.05f; 
        }
        
        
        return Physics.SphereCast(rayOrigin, groundCheckRadius, rayDirection, out _, groundCheckDistance, groundLayer);
    }
    
    private void CheckFallDeath()
    {
        bool hasFallenTooFar = false;
        
        if (isGravityInverted)
        {
            hasFallenTooFar = transform.position.y >= fallDeathY;
        }
        else
        {
            hasFallenTooFar = transform.position.y <= -fallDeathY;
        }
        
        if (hasFallenTooFar)
        {
            RespawnAtCheckpoint();
        }
    }
    
    private void RespawnAtCheckpoint()
    {
        controller.enabled = false;
        transform.position = lastValidCheckpoint;
        
        velocity = Vector3.zero;
        
        if (isGravityInverted)
        {
            isGravityInverted = false;
            playerModel.localRotation = Quaternion.identity;
            targetCameraRotation = Quaternion.identity;
        }
        
        controller.enabled = true;
    }
    
    
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    public bool IsGravityInverted()
    {
        return isGravityInverted;
    }
    
    public void SetCheckpoint(Vector3 position)
    {
        lastValidCheckpoint = position;
    }
    
}
