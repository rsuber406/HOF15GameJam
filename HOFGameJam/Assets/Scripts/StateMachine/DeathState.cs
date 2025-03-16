using UnityEngine;

public class DeathState : PlayerState
{
    public DeathState(PlayerController player, PlayerStateMachine stateMachine) 
        : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        player.RespawnAtCheckpoint();
        
        stateMachine.ChangeState(player.GetGroundedState());
    }
}
