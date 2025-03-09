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

    [SerializeField] Transform camTransform;
    //  [SerializeField] Animator animator;

    [SerializeField] Vector3 lastValidCheckpoint;

    Vector3 moveDir;
    Vector3 playerVel;

    int jumpCount;

    void Start()
    {
       // animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        movement();
        if (this.transform.position.y <= -100)
        {
            deathByFall();
        }
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 cameraForward = Vector3.Scale(camTransform.forward, new Vector3(1, 0, 1)).normalized;

        moveDir = (Input.GetAxis("Horizontal") * camTransform.right) + (Input.GetAxis("Vertical") * camTransform.forward);
        controller.Move(moveDir * speed * Time.deltaTime);

        if (moveDir.magnitude > 0.1f)
        {
          //  animator.SetBool("IsRunning", true);
        }
        else
        {
         //   animator.SetBool("IsRunning", false);
        }
        jump();

        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;


    }
    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
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
        if (other.tag == "Checkpoint")
        {
            lastValidCheckpoint = this.transform.position;
        }
    }

}