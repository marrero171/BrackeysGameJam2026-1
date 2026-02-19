using UnityEngine;

public class State_Transitioning : IGameState
{
    private readonly GameStateMachine _stateMachine;

    public State_Transitioning(GameStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log("[State_Transitioning] Entered - Character transitioning between boards");
    }

    public void Exit()
    {
        Debug.Log("[State_Transitioning] Exited");
    }

    public void Tick()
    {
    }
}
