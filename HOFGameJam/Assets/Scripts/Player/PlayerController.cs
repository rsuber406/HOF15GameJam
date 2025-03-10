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
    [SerializeField] private float cameraInversionSpeed;
    
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
    private float jumpCooldown = 0f; 
    
    
    private void Start()
    {
        lastValidCheckpoint = initialCheckpoint;
        targetCameraRotation = cameraTransform.localRotation;
        
        controller = GetComponent<CharacterController>();
    }
    
    private void Update()
    {
        // Update jump cooldown
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }
        
    
        isGrounded = (jumpCooldown <= 0) && CheckGrounded();
        
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
        if (isGrounded)
        {
            jumpCount = 0;
        }
        
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            float jumpVelocity = Mathf.Sqrt(2 * gravityStrength * jumpHeight);
            
            velocity.y = jumpVelocity * (isGravityInverted ? -1 : 1);
            jumpCount++;
            jumpCooldown = 0.1f; // 100ms cooldown
        }
    }
    
    private void ApplyGravity()
    {
        if (isGrounded && jumpCooldown <= 0)
        {
            velocity.y = -0.5f * (isGravityInverted ? -1 : 1);
        }
        else
        {
            int gravityDirection = isGravityInverted ? -1 : 1;
           
            velocity.y -= gravityStrength * gravityDirection * Time.deltaTime;
            float maxFallSpeed = 20f;
            if (isGravityInverted)
            {
                velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            }
            else
            {
                velocity.y = Mathf.Min(velocity.y, maxFallSpeed);
            }
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
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetCameraRotation, cameraInversionSpeed * Time.deltaTime);
    }

    private bool CheckGrounded()
    {
        float offsetFromCenter = controller.height / 2;
        Vector3 origin = transform.position;
        
        if (isGravityInverted)
        {
            origin.y += offsetFromCenter - groundCheckDistance * 0.5f;
        }
        else
        {
            origin.y -= offsetFromCenter - groundCheckDistance * 0.5f;
        }
        
        Vector3 direction = isGravityInverted ? Vector3.up : Vector3.down;
        float actualCheckDistance = groundCheckDistance;
        Collider[] hitColliders = Physics.OverlapSphere(origin + direction * actualCheckDistance, groundCheckRadius);
        
        bool foundGround = false;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject)
                continue;
            if (hitCollider.CompareTag("Ground"))
            {
                foundGround = true;
                break;
            }
        }
        
        if (showGroundCheckDebug)
        {
            Color debugColor = foundGround ? Color.green : Color.red;
            Debug.DrawRay(origin, direction * actualCheckDistance, debugColor, 0.01f);
            
            Vector3 sphereCenter = origin + direction * actualCheckDistance;
            Debug.DrawLine(sphereCenter, sphereCenter + Vector3.right * groundCheckRadius, debugColor, 0.01f);
            Debug.DrawLine(sphereCenter, sphereCenter + Vector3.left * groundCheckRadius, debugColor, 0.01f);
            Debug.DrawLine(sphereCenter, sphereCenter + Vector3.forward * groundCheckRadius, debugColor, 0.01f);
            Debug.DrawLine(sphereCenter, sphereCenter + Vector3.back * groundCheckRadius, debugColor, 0.01f);
            
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * 2 * Mathf.PI;
                float nextAngle = (float)(i + 1) / segments * 2 * Mathf.PI;
                
                Vector3 pos1 = sphereCenter + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * groundCheckRadius;
                Vector3 pos2 = sphereCenter + new Vector3(Mathf.Cos(nextAngle), 0, Mathf.Sin(nextAngle)) * groundCheckRadius;
                
                Debug.DrawLine(pos1, pos2, debugColor, 0.01f);
            }
        }
        
        return foundGround;
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