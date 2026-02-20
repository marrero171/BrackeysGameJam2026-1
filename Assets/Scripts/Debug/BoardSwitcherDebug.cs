using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardSwitcherDebug : MonoBehaviour
{
    [Header("Check Status")]
    [SerializeField] private BoardSwitcherUI boardSwitcherUI;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI boardInfoText;

    [Header("Manual Test")]
    [SerializeField] private KeyCode testKey = KeyCode.T;

    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            RunDiagnostics();
        }
    }

    [ContextMenu("Run Diagnostics")]
    public void RunDiagnostics()
    {
        Debug.Log("=== BOARD SWITCHER DIAGNOSTICS ===");

        if (boardSwitcherUI == null)
        {
            boardSwitcherUI = GetComponent<BoardSwitcherUI>();
        }

        Debug.Log($"BoardSwitcherUI Component: {(boardSwitcherUI != null ? "✓ Found" : "✗ Missing")}");
        Debug.Log($"This GameObject Active: {gameObject.activeInHierarchy}");
        Debug.Log($"This GameObject ActiveSelf: {gameObject.activeSelf}");

        if (previousButton != null)
        {
            Debug.Log($"Previous Button:");
            Debug.Log($"  - GameObject: {previousButton.gameObject.name}");
            Debug.Log($"  - Active: {previousButton.gameObject.activeInHierarchy}");
            Debug.Log($"  - Interactable: {previousButton.interactable}");
            Debug.Log($"  - Enabled: {previousButton.enabled}");
        }
        else
        {
            Debug.LogWarning("Previous Button reference is NULL!");
        }

        if (nextButton != null)
        {
            Debug.Log($"Next Button:");
            Debug.Log($"  - GameObject: {nextButton.gameObject.name}");
            Debug.Log($"  - Active: {nextButton.gameObject.activeInHierarchy}");
            Debug.Log($"  - Interactable: {nextButton.interactable}");
            Debug.Log($"  - Enabled: {nextButton.enabled}");
        }
        else
        {
            Debug.LogWarning("Next Button reference is NULL!");
        }

        if (boardInfoText != null)
        {
            Debug.Log($"Board Info Text:");
            Debug.Log($"  - Text: '{boardInfoText.text}'");
            Debug.Log($"  - Active: {boardInfoText.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogWarning("Board Info Text reference is NULL!");
        }

        if (BoardManager.Instance != null)
        {
            Debug.Log($"BoardManager:");
            Debug.Log($"  - Board Count: {BoardManager.Instance.BoardCount}");
            Debug.Log($"  - Active Board Index: {BoardManager.Instance.ActiveBoardIndex}");
            if (BoardManager.Instance.ActiveBoard != null)
            {
                Debug.Log($"  - Active Board Name: {BoardManager.Instance.ActiveBoard.BoardData.name}");
            }
        }
        else
        {
            Debug.LogError("BoardManager Instance is NULL!");
        }

        if (GameStateMachine.Instance != null)
        {
            Debug.Log($"GameStateMachine:");
            Debug.Log($"  - Current State: {GameStateMachine.Instance.CurrentState?.GetType().Name}");
        }

        Debug.Log("=== END DIAGNOSTICS ===");
    }
}
