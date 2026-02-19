using UnityEngine;

public class State_Win : IGameState
{
    private readonly GameStateMachine _stateMachine;
    private readonly GameUIManager _uiManager;

    public State_Win(GameStateMachine stateMachine, GameUIManager uiManager)
    {
        _stateMachine = stateMachine;
        _uiManager = uiManager;
    }

    public void Enter()
    {
        Debug.Log("[State_Win] Entered - Player Won!");

        if (_uiManager != null)
        {
            _uiManager.ShowWinText();
        }
    }

    public void Exit()
    {
        Debug.Log("[State_Win] Exited");

        if (_uiManager != null)
        {
            _uiManager.HideWinText();
        }
    }

    public void Tick()
    {
    }
}
