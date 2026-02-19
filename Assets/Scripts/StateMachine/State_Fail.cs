using UnityEngine;

public class State_Fail : IGameState
{
    private readonly GameStateMachine _stateMachine;
    private readonly GameUIManager _uiManager;

    public State_Fail(GameStateMachine stateMachine, GameUIManager uiManager)
    {
        _stateMachine = stateMachine;
        _uiManager = uiManager;
    }

    public void Enter()
    {
        Debug.Log("[State_Fail] Entered - Player Failed!");

        if (_uiManager != null)
        {
            _uiManager.ShowFailText();
        }
    }

    public void Exit()
    {
        Debug.Log("[State_Fail] Exited");

        if (_uiManager != null)
        {
            _uiManager.HideFailText();
        }
    }

    public void Tick()
    {
    }
}
