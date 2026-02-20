using UnityEngine;

public class BoardTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private BoardData testBoardData;

    private void Start()
    {
        if (testBoardData != null)
        {
            TestBoardCreation();
        }
        else
        {
            Debug.LogWarning("[BoardTest] No BoardData assigned for testing");
        }
    }

    private void TestBoardCreation()
    {
        GameObject boardObject = new GameObject("TestBoard");
        Board board = boardObject.AddComponent<Board>();
        
        board.Initialize(testBoardData, 0);
        
        Debug.Log($"[BoardTest] Board created successfully!");
        Debug.Log($"  - BoardIndex: {board.BoardIndex}");
        Debug.Log($"  - BoardData: {board.BoardData.name}");
        Debug.Log($"  - TileGrid: {(board.TileGrid != null ? "✓" : "✗")}");
        Debug.Log($"  - IsVisible: {board.IsVisible}");
        
        board.Show();
        Debug.Log($"  - After Show(), IsVisible: {board.IsVisible}");
        
        board.Hide();
        Debug.Log($"  - After Hide(), IsVisible: {board.IsVisible}");
    }
}
