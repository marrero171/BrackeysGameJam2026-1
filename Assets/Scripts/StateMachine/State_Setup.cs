using UnityEngine;

public class State_Setup : IGameState
{
    private readonly GameStateMachine _stateMachine;
    private readonly InputManager _inputManager;
    private readonly GameUIManager _uiManager;
    private readonly CharacterMover _characterMover;

    public State_Setup(GameStateMachine stateMachine, InputManager inputManager, GameUIManager uiManager, CharacterMover characterMover)
    {
        _stateMachine = stateMachine;
        _inputManager = inputManager;
        _uiManager = uiManager;
        _characterMover = characterMover;
    }

    public void Enter()
    {
        Debug.Log("[State_Setup] Entered");

        if (_characterMover != null)
        {
            _characterMover.SpawnCharacter();
        }

        if (_inputManager != null)
        {
            _inputManager.SetInputEnabled(true);
        }

        if (_uiManager != null)
        {
            _uiManager.ShowPlayButton();
            _uiManager.HideWinPanel();
            _uiManager.HideFailPanel();
            _uiManager.HideGameMenu();
        }
    }

    public void Exit()
    {
        Debug.Log("[State_Setup] Exited");

        if (_uiManager != null)
        {
            _uiManager.HidePlayButton();
        }
    }

    public void Tick()
    {
    }
}
