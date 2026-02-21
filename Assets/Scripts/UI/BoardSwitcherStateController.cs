using UnityEngine;

public class BoardSwitcherStateController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private BoardSwitcherUI boardSwitcherUI;

    private void Start()
    {
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.OnStateChanged += OnGameStateChanged;
        }

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPauseStateChanged += OnPauseStateChanged;
        }

        if (boardSwitcherUI == null)
        {
            boardSwitcherUI = GetComponent<BoardSwitcherUI>();
        }

        UpdateVisibility();
    }

    private void OnDestroy()
    {
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.OnStateChanged -= OnGameStateChanged;
        }

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPauseStateChanged -= OnPauseStateChanged;
        }
    }

    private void OnGameStateChanged(IGameState newState)
    {
        UpdateVisibility();
    }

    private void OnPauseStateChanged(bool isPaused)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (boardSwitcherUI == null || GameStateMachine.Instance == null)
            return;

        IGameState currentState = GameStateMachine.Instance.CurrentState;

        bool shouldShow = currentState is State_Setup || 
                         (PauseManager.Instance != null && PauseManager.Instance.IsPaused);

        if (shouldShow)
        {
            boardSwitcherUI.Show();
        }
        else
        {
            boardSwitcherUI.Hide();
        }
    }
}
