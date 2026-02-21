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

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ForceResume();
        }

        if (_uiManager != null)
        {
            _uiManager.ShowWinPanel();
        }
    }

    public void Exit()
    {
        Debug.Log("[State_Win] Exited");

        if (_uiManager != null)
        {
            _uiManager.HideWinPanel();
        }
    }

    public void Tick()
    {
    }
}
