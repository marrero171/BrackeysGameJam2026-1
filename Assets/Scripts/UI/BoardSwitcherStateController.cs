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
    }

    private void OnGameStateChanged(IGameState newState)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (boardSwitcherUI == null || GameStateMachine.Instance == null)
            return;

        IGameState currentState = GameStateMachine.Instance.CurrentState;

        if (currentState is State_Setup)
        {
            boardSwitcherUI.Show();
        }
        else
        {
            boardSwitcherUI.Hide();
        }
    }
}
