using UnityEngine;

using UnityEngine;

public class GravityTransitionState : PlayerState
{
    public GravityTransitionState(PlayerController player, PlayerStateMachine stateMachine) 
        : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        player.IsTransitioning = true;
        player.GravityTransitionTimer = 0f;
        player.GravityInversionCooldownTimer = player.GravityInversionCooldown;
        player.InitialCameraRotation = player.PlayerModel.localRotation;
        
        Time.timeScale = player.DefaultTimeScale * player.TimeSlowFactor;
    }

    public override void Exit()
    {
        Time.timeScale = player.DefaultTimeScale;
    }

    public override void Update()
    {
        player.GravityTransitionTimer += Time.unscaledDeltaTime;
        float progress = player.GravityTransitionTimer / player.GravityTransitionDuration;

        if (progress >= 1.0f)
        {
            CompleteTransition();
        }
        else
        {
            UpdateTransition(progress);
        }
        
        player.Controller.Move(player.Velocity * Time.deltaTime);
    }

    private void CompleteTransition()
    {
        player.IsTransitioning = false;
        player.IsGravityInverted = !player.IsGravityInverted;
        player.CurrentGravityFactor = player.IsGravityInverted ? -1f : 1f;
        
        Time.timeScale = player.DefaultTimeScale;
        
        player.Velocity = Vector3.zero;
        player.Velocity = new Vector3(0, 0.2f * player.CurrentGravityFactor, 0);
        
        player.JumpCount = 0;
        
        if (player.CheckGrounded())
        {
            stateMachine.ChangeState(player.GetGroundedState());
        }
        else
        {
            stateMachine.ChangeState(player.GetAirborneState());
        }
    }

    private void UpdateTransition(float progress)
    {
        float targetGravityFactor = player.IsGravityInverted ? 1f : -1f;
        player.CurrentGravityFactor = Mathf.LerpAngle(player.CurrentGravityFactor, targetGravityFactor, SmoothTransitionCurve(progress));

        float targetAngle = player.IsGravityInverted ? 0f : 180f;
        if (!player.IsGravityInverted)
        {
            Quaternion targetRotation = Quaternion.Euler(targetAngle, player.InitialCameraRotation.eulerAngles.y, 0f);
            player.TargetCameraRotation = Quaternion.Slerp(player.InitialCameraRotation, targetRotation, SmoothTransitionCurve(progress));
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(targetAngle, -player.InitialCameraRotation.eulerAngles.y, 0f);
            player.TargetCameraRotation = Quaternion.Slerp(player.InitialCameraRotation, targetRotation, SmoothTransitionCurve(progress));
        }

        player.PlayerModel.localRotation = player.TargetCameraRotation;
    }

    private float SmoothTransitionCurve(float t)
    {
        // smooth step function: 3t^2 - 2t^3
        //used for smoothing out animations
        return t * t * (3f - 2f * t);
    }
}
