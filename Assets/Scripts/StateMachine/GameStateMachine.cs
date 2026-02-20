using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance { get; private set; }

    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private CharacterMover characterMover;
    [SerializeField] private GameUIManager uiManager;

    private Dictionary<Type, IGameState> _states = new Dictionary<Type, IGameState>();
    private IGameState _currentState;

    public IGameState CurrentState => _currentState;

    public delegate void StateChangedHandler(IGameState newState);
    public event StateChangedHandler OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeStates();
    }

    private void Start()
    {
        TransitionTo<State_Setup>();
    }

    private void Update()
    {
        _currentState?.Tick();
    }

    private void InitializeStates()
    {
        _states[typeof(State_Setup)] = new State_Setup(this, inputManager, uiManager, characterMover);
        _states[typeof(State_Playing)] = new State_Playing(this, inputManager, characterMover);
        _states[typeof(State_Transitioning)] = new State_Transitioning(this);
        _states[typeof(State_Win)] = new State_Win(this, uiManager);
        _states[typeof(State_Fail)] = new State_Fail(this, uiManager);

        Debug.Log("[GameStateMachine] All states initialized");
    }

    public void TransitionTo<T>() where T : IGameState
    {
        Type stateType = typeof(T);

        if (!_states.ContainsKey(stateType))
        {
            Debug.LogError($"[GameStateMachine] State {stateType.Name} not found!");
            return;
        }

        _currentState?.Exit();
        _currentState = _states[stateType];
        _currentState.Enter();

        Debug.Log($"[GameStateMachine] Transitioned to {stateType.Name}");

        OnStateChanged?.Invoke(_currentState);
    }

    public T GetState<T>() where T : IGameState
    {
        Type stateType = typeof(T);
        if (_states.ContainsKey(stateType))
        {
            return (T)_states[stateType];
        }
        return default(T);
    }
}
