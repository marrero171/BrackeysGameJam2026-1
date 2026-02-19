using UnityEngine;

public class State_Playing : IGameState
{
    private readonly GameStateMachine _stateMachine;
    private readonly InputManager _inputManager;
    private readonly CharacterMover _characterMover;

    public State_Playing(GameStateMachine stateMachine, InputManager inputManager, CharacterMover characterMover)
    {
        _stateMachine = stateMachine;
        _inputManager = inputManager;
        _characterMover = characterMover;
    }

    public void Enter()
    {
        Debug.Log("[State_Playing] Entered");

        if (_inputManager != null)
        {
            _inputManager.SetInputEnabled(false);
        }

        if (_characterMover != null)
        {
            _characterMover.StartMoving();
        }
    }

    public void Exit()
    {
        Debug.Log("[State_Playing] Exited");

        if (_characterMover != null)
        {
            _characterMover.StopMoving();
        }
    }

    public void Tick()
    {
    }
}
