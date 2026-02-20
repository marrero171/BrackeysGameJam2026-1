using UnityEngine;

public class BoardManagerTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private BoardData[] testBoards;

    private void Start()
    {
        if (testBoards == null || testBoards.Length == 0)
        {
            Debug.LogWarning("[BoardManagerTest] No test boards assigned");
            return;
        }

        TestBoardManager();
    }

    private void TestBoardManager()
    {
        Debug.Log("=== BoardManager Test Started ===");

        if (BoardManager.Instance == null)
        {
            Debug.LogError("[BoardManagerTest] BoardManager instance not found!");
            return;
        }

        Debug.Log($"[BoardManagerTest] Loading {testBoards.Length} boards...");
        BoardManager.Instance.LoadBoards(testBoards);

        Debug.Log($"[BoardManagerTest] Total boards loaded: {BoardManager.Instance.BoardCount}");
        Debug.Log($"[BoardManagerTest] Active board index: {BoardManager.Instance.ActiveBoardIndex}");

        if (BoardManager.Instance.ActiveBoard != null)
        {
            Debug.Log($"[BoardManagerTest] Active board name: {BoardManager.Instance.ActiveBoard.BoardData.name}");
            Debug.Log($"[BoardManagerTest] Active board visible: {BoardManager.Instance.ActiveBoard.IsVisible}");
        }

        if (BoardManager.Instance.BoardCount > 1)
        {
            Debug.Log("[BoardManagerTest] Testing board switching...");
            
            BoardManager.Instance.SetActiveBoard(1);
            Debug.Log($"[BoardManagerTest] Switched to board 1: {BoardManager.Instance.ActiveBoard.BoardData.name}");

            BoardManager.Instance.SetActiveBoard(0);
            Debug.Log($"[BoardManagerTest] Switched back to board 0: {BoardManager.Instance.ActiveBoard.BoardData.name}");
        }

        Debug.Log("=== BoardManager Test Completed ===");
    }

    private void Update()
    {
        if (BoardManager.Instance == null || BoardManager.Instance.BoardCount <= 1)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            BoardManager.Instance.SwitchToNextBoard();
            Debug.Log($"[BoardManagerTest] Switched to next board: {BoardManager.Instance.ActiveBoardIndex}");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            BoardManager.Instance.SwitchToPreviousBoard();
            Debug.Log($"[BoardManagerTest] Switched to previous board: {BoardManager.Instance.ActiveBoardIndex}");
        }
    }
}
