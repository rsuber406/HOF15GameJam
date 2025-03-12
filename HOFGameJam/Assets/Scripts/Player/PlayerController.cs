using System.Collections;
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
    [SerializeField] private float cameraInversionSpeed = 2f;

    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private bool showGroundCheckDebug = true;

    [SerializeField] private Vector3 initialCheckpoint = Vector3.zero;
    [SerializeField] private float fallDeathY = 100f;

    private KeyCode gravityInvertKey = KeyCode.G;

    [SerializeField] private float gravityTransitionDuration = 1.0f;
    [SerializeField] private float gravityInversionCooldown = 1.5f;
    [SerializeField] private float timeSlowFactor = 0.5f;

    private Vector3 velocity;
    private Vector3 lastValidCheckpoint;
    private int jumpCount;
    private bool isGrounded;
    private bool isGravityInverted;
    private Quaternion targetCameraRotation;
    private float jumpCooldown = 0f;

    private float gravityTransitionTimer = 0f;
    private float gravityInversionCooldownTimer = 0f;
    private float currentGravityFactor = 1f;
    private bool isTransitioning = false;
    private float defaultTimeScale;
    private Quaternion initialCameraRotation;

    private bool canInvert = false;

    private void Start()
    {
        lastValidCheckpoint = initialCheckpoint;
        targetCameraRotation = cameraTransform.localRotation;
        defaultTimeScale = Time.timeScale;

        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }

        if (gravityInversionCooldownTimer > 0)
        {
            gravityInversionCooldownTimer -= Time.deltaTime;
        }

        if (isTransitioning)
        {
            UpdateGravityTransition();
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
        else if (other.CompareTag("AbilityTrigger"))
        {
            canInvert = true;
            GameManager.instance.toolTip.SetActive(true);
            StartCoroutine(wait());
        }
        else if (other.CompareTag("GameWin"))
        {
            GameManager.instance.Win();
        }
    }

    private IEnumerator wait()
    {
        yield return new WaitForSeconds(5.0f);
        GameManager.instance.toolTip.SetActive(false);
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
            velocity.y = jumpVelocity * (currentGravityFactor);
            jumpCount++;
            jumpCooldown = 0.1f;
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded && jumpCooldown <= 0)
        {
            velocity.y = -0.5f * currentGravityFactor;
        }
        else
        {
            velocity.y -= gravityStrength * currentGravityFactor * Time.deltaTime;

            float maxFallSpeed = 20f;
            float absGravityFactor = Mathf.Abs(currentGravityFactor);
            float minVelocity = -maxFallSpeed * absGravityFactor;
            float maxVelocity = maxFallSpeed * absGravityFactor;
            velocity.y = Mathf.Clamp(velocity.y, minVelocity, maxVelocity);
        }
    }

    private void HandleGravityInversion()
    {
        if (canInvert)
        {
            if (Input.GetKeyDown(gravityInvertKey) && !isTransitioning && gravityInversionCooldownTimer <= 0)
            {
                StartGravityTransition();
            }
            
        }
    }

    private void StartGravityTransition()
    {
        isTransitioning = true;
        gravityTransitionTimer = 0f;
        gravityInversionCooldownTimer = gravityInversionCooldown;
        initialCameraRotation = playerModel.localRotation;

        Time.timeScale = defaultTimeScale * timeSlowFactor;
    }

    private void UpdateGravityTransition()
    {
        gravityTransitionTimer += Time.unscaledDeltaTime;
        float progress = gravityTransitionTimer / gravityTransitionDuration;

        if (progress >= 1.0f)
        {
            isTransitioning = false;
            isGravityInverted = !isGravityInverted;
            
            if (isGravityInverted)
            {
                currentGravityFactor = -1f;
            }
            else
            {
                currentGravityFactor = 1f;
            }
            
            Time.timeScale = defaultTimeScale;

            velocity = Vector3.zero;
            velocity.y = 0.2f * currentGravityFactor;

            jumpCount = 0;
            
            float targetAngle;
            if (isGravityInverted)
            {
                targetAngle = 180f;
            }
            else
            {
                targetAngle = 0f;
            }
            
            Vector3 currentEuler = initialCameraRotation.eulerAngles;
            targetCameraRotation = Quaternion.Euler(targetAngle, currentEuler.y, currentEuler.z);
            playerModel.localRotation = targetCameraRotation;
        }
        else
        {
            float targetGravityFactor;
            if (isGravityInverted)
            {
                targetGravityFactor = -1f;
            }
            else
            {
                targetGravityFactor = 1f;
            }
            
            currentGravityFactor = Mathf.Lerp(currentGravityFactor, targetGravityFactor, SmoothTransitionCurve(progress));
            
            float targetAngle;
            if (isGravityInverted)
            {
                targetAngle = 0f;
            }
            else
            {
                targetAngle = 180f;
            }

            Vector3 currentEuler = initialCameraRotation.eulerAngles;
            targetCameraRotation = Quaternion.Lerp(initialCameraRotation, Quaternion.Euler(targetAngle, currentEuler.y, currentEuler.z), SmoothTransitionCurve(progress));
        
            playerModel.localRotation = targetCameraRotation;
        }
    }

    private float SmoothTransitionCurve(float t)
    {
        // smooth step function: 3t^2 - 2t^3
        return t * t * (3f - 2f * t);
    }
    
    private void UpdateCameraRotation()
    {
        if (targetCameraRotation == Quaternion.identity)
        {
            targetCameraRotation = cameraTransform.localRotation;
        }
    
        float speed;
        if (isTransitioning)
        {
            speed = cameraInversionSpeed * 1.5f;
        }
        else
        {
            speed = cameraInversionSpeed;
        }
        
        cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, targetCameraRotation, speed * Time.unscaledDeltaTime);
    }

    private bool CheckGrounded()
    {
        float offsetFromCenter = controller.height / 2;
        Vector3 origin = transform.position;
        int gravityDirection;
        
        if (currentGravityFactor < 0)
        {
            gravityDirection = -1;
        }
        else
        {
            gravityDirection = 1;
        }
        
        if (gravityDirection < 0)
        {
            origin.y += offsetFromCenter - groundCheckDistance * 0.5f;
        }
        else
        {
            origin.y -= offsetFromCenter - groundCheckDistance * 0.5f;
        }

        Vector3 direction;
        if (gravityDirection < 0)
        {
            direction = Vector3.up;
        }
        else
        {
            direction = Vector3.down;
        }
        
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
        return foundGround;
    }

    private void CheckFallDeath()
    {
        bool hasFallenTooFar = false;

        if (currentGravityFactor < 0)
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
            currentGravityFactor = 1f;
            playerModel.localRotation = Quaternion.identity;
            targetCameraRotation = Quaternion.identity;
        }
        controller.enabled = true;
        isTransitioning = false;
        Time.timeScale = defaultTimeScale;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsGravityInverted()
    {
        return isGravityInverted;
    }

    public bool IsTransitioningGravity()
    {
        return isTransitioning;
    }

    public void SetCheckpoint(Vector3 position)
    {
        lastValidCheckpoint = position;
    }

    private void OnDisable()
    {
        Time.timeScale = defaultTimeScale;
    }
}