using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreMask;

    [SerializeField] int speed;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] Vector3 lastValidCheckpoint;

    [SerializeField] private KeyCode gravityInvert = KeyCode.G;
    [SerializeField] Transform playerModel;
    [SerializeField] Transform camTransform;

    [SerializeField] private float cameraInversionSpeed;

    float groundCheckDistance = 0.3f;
    private bool isCustomGrounded;
    Vector3 moveDir;
    Vector3 playerVel;
    private bool isGravityInverted;
    private int currentGravityModifier = 1;
    Quaternion targetCamRotation;

    int jumpCount;

    void Start()
    {
        targetCamRotation = camTransform.localRotation;
    }

    void Update()
    {
        CheckGravityInversion();
        CheckGrounded();
        movement();
        UpdateCamera();

        if (this.transform.position.y <= -100 || this.transform.position.y >= 100)
        {
            deathByFall();
        }
    }

    void CheckGrounded()
    {
        
        
    }

    void UpdateCamera()
    {
        camTransform.localRotation = Quaternion.Slerp(
            camTransform.localRotation,
            targetCamRotation,
            cameraInversionSpeed * Time.deltaTime
        );
    }

    void movement()
    {
        if (isCustomGrounded)
        {
            jumpCount = 0;
            playerVel.y = 0;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 cameraForward = Vector3.Scale(camTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = camTransform.right;

        if (isGravityInverted)
        {
            horizontal *= -1;
        }

        moveDir = (horizontal * cameraRight) + (vertical * cameraForward);
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();

        playerVel.y -= gravity * currentGravityModifier * Time.deltaTime;
        controller.Move(playerVel * Time.deltaTime);
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax && isCustomGrounded)
        {
            jumpCount++;
            playerVel.y = jumpSpeed * (isGravityInverted ? -1 : 1);
            Debug.Log("Jump executed with velocity: " + playerVel.y);
        }
    }

    void deathByFall()
    {
        controller.enabled = false;
        this.transform.position = lastValidCheckpoint;
        controller.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            lastValidCheckpoint = this.transform.position;
        }
    }

    private void CheckGravityInversion()
    {
        if (Input.GetKeyDown(gravityInvert))
        {
            isGravityInverted = !isGravityInverted;
            currentGravityModifier = isGravityInverted ? -1 : 1;
            
            playerModel.localRotation = Quaternion.Euler(isGravityInverted ? 180f : 0f, 0f, 0f);
            
            
            playerVel = Vector3.zero;
            jumpCount = 0;
            playerVel.y = 0.2f * currentGravityModifier;
        }
    }
}
