using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private LayerMask ignoreMask;

    [SerializeField] private int speed;
    [SerializeField] private int jumpMax;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private int gravity;


    [SerializeField] private Transform camTransform;


    private Vector3 moveDir;
    private Vector3 playerVel;

    private int jumpCount;

    void Start()
    {
    }

    void Update()
    {
     Movement();   
        
    }

    void Movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        moveDir = (Input.GetAxis("Horizontal") * camTransform.right) + (Input.GetAxis("Vertical") * camTransform.forward);
        Jump();
        controller.Move(moveDir * speed * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }



}
