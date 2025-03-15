using UnityEngine;

public class AirborneState : PlayerState
{
    public AirborneState(PlayerController player, PlayerStateMachine stateMachine) 
        : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        player.IsGrounded = false;
    }

    public override void HandleInput()
    {
        if (player.CanInvert && Input.GetKeyDown(player.GravityInvertKey) && 
            !player.IsTransitioning && player.GravityInversionCooldownTimer <= 0)
        {
            stateMachine.ChangeState(player.GetGravityTransitionState());
            return;
        }
        if (Input.GetButtonDown("Jump") && player.JumpCount < player.MaxJumps)
        {
            float jumpVelocity = Mathf.Sqrt(2 * player.GravityStrength * player.JumpHeight);
            player.Velocity = new Vector3(player.Velocity.x, jumpVelocity * player.CurrentGravityFactor, player.Velocity.z);
            player.JumpCount++;
            player.JumpCooldown = 0.1f;
        }
    }

    public override void Update()
    {
        bool isGrounded = (player.JumpCooldown <= 0) && player.CheckGrounded();
        
        if (isGrounded)
        {
            stateMachine.ChangeState(player.GetGroundedState());
            return;
        }
        
        HandleMovement();
        
        ApplyGravity();
        
        player.Controller.Move(player.Velocity * Time.deltaTime);
        
        CheckFallDeath();
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 cameraForward = Vector3.Scale(player.CameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = player.CameraTransform.right;
        Vector3 moveDirection = (horizontalInput * cameraRight) + (verticalInput * cameraForward);
        
        if (moveDirection.magnitude > 0.1f)
        {
            player.Controller.Move(moveDirection.normalized * player.MoveSpeed * Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        player.Velocity -= new Vector3(0, player.GravityStrength * player.CurrentGravityFactor * Time.deltaTime, 0);
        
        float maxFallSpeed = 20f;
        player.Velocity = new Vector3(
            player.Velocity.x,
            Mathf.Clamp(player.Velocity.y, -maxFallSpeed * Mathf.Abs(player.CurrentGravityFactor), maxFallSpeed * Mathf.Abs(player.CurrentGravityFactor)),
            player.Velocity.z
        );
    }

    private void CheckFallDeath()
    {
        bool hasFallenTooFar = false;

        if (player.CurrentGravityFactor < 0)
        {
            hasFallenTooFar = player.transform.position.y >= player.FallDeathY;
        }
        else
        {
            hasFallenTooFar = player.transform.position.y <= -player.FallDeathY;
        }

        if (hasFallenTooFar)
        {
            stateMachine.ChangeState(player.GetDeathState());
        }
    }
}
