using UnityEngine;

public class MultiBoardIntegrationTest : MonoBehaviour
{
    [Header("Test Setup")]
    [SerializeField] private bool runTestOnStart = true;

    private void Start()
    {
        if (runTestOnStart)
        {
            Invoke(nameof(RunIntegrationTest), 0.5f);
        }
    }

    private void RunIntegrationTest()
    {
        Debug.Log("=== Multi-Board Integration Test Started ===");

        if (LevelManager.Instance == null)
        {
            Debug.LogError("[IntegrationTest] LevelManager not found!");
            return;
        }

        if (BoardManager.Instance == null)
        {
            Debug.LogError("[IntegrationTest] BoardManager not found!");
            return;
        }

        Debug.Log($"[IntegrationTest] LevelManager: ✓");
        Debug.Log($"[IntegrationTest] BoardManager: ✓");

        Debug.Log($"[IntegrationTest] Current Level: {LevelManager.Instance.CurrentLevelData?.levelId}");
        Debug.Log($"[IntegrationTest] Total Boards: {BoardManager.Instance.BoardCount}");
        Debug.Log($"[IntegrationTest] Active Board Index: {BoardManager.Instance.ActiveBoardIndex}");

        if (BoardManager.Instance.ActiveBoard != null)
        {
            Debug.Log($"[IntegrationTest] Active Board: {BoardManager.Instance.ActiveBoard.BoardData.name}");
            Debug.Log($"[IntegrationTest] Active Board Visible: {BoardManager.Instance.ActiveBoard.IsVisible}");
        }

        GameObject startingTile = LevelManager.Instance.StartingTileObject;
        TileBase startingTileBase = startingTile != null ? startingTile.GetComponent<TileBase>() : null;
        Debug.Log($"[IntegrationTest] Starting Tile Position: {(startingTileBase != null ? startingTileBase.gridPosition.ToString() : "not found")}");
        Debug.Log($"[IntegrationTest] Starting Tile Object: {(startingTile != null ? "Found" : "Not found")}");

        TestBoardSwitching();

        Debug.Log("=== Multi-Board Integration Test Completed ===");
    }

    private void TestBoardSwitching()
    {
        if (BoardManager.Instance.BoardCount <= 1)
        {
            Debug.Log("[IntegrationTest] Only one board loaded, skipping switch test");
            return;
        }

        Debug.Log("[IntegrationTest] Testing board switching...");

        int initialBoard = BoardManager.Instance.ActiveBoardIndex;
        Debug.Log($"  Initial board: {initialBoard}");

        LevelManager.Instance.LoadBoard(1);
        Debug.Log($"  After LoadBoard(1): Active board = {BoardManager.Instance.ActiveBoardIndex}");

        LevelManager.Instance.LoadBoard(0);
        Debug.Log($"  After LoadBoard(0): Active board = {BoardManager.Instance.ActiveBoardIndex}");

        Debug.Log("[IntegrationTest] Board switching test completed");
    }

    private void Update()
    {
        if (BoardManager.Instance == null || BoardManager.Instance.BoardCount <= 1)
            return;

        if (Input.GetKeyDown(KeyCode.N))
        {
            BoardManager.Instance.SwitchToNextBoard();
            Debug.Log($"[IntegrationTest] Switched to next board: {BoardManager.Instance.ActiveBoardIndex}");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            BoardManager.Instance.SwitchToPreviousBoard();
            Debug.Log($"[IntegrationTest] Switched to previous board: {BoardManager.Instance.ActiveBoardIndex}");
        }
    }
}
