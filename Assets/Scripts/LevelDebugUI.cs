using UnityEngine;
using UnityEngine.UI;

public class LevelDebugUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text levelInfoText;
    [SerializeField] private Text tilesInfoText;

    private void OnEnable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded += UpdateUI;
            LevelManager.Instance.OnStartingTileDetected += OnTilesDetected;
        }
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded -= UpdateUI;
            LevelManager.Instance.OnStartingTileDetected -= OnTilesDetected;
        }
    }

    private void Start()
    {
        UpdateUI(0);
    }

    private void UpdateUI(int levelIndex)
    {
        if (LevelManager.Instance == null || levelInfoText == null)
        {
            return;
        }

        LevelData levelData = LevelManager.Instance.CurrentLevelData;
        if (levelData == null)
        {
            levelInfoText.text = "No Level Loaded";
            return;
        }

        string info = $"Level: {levelIndex} ({levelData.levelId})\n";
        info += $"Board: {LevelManager.Instance.CurrentBoardIndex}/{levelData.boards.Length - 1}\n";

        levelInfoText.text = info;
    }

    private void OnTilesDetected(GameObject startTile)
    {
        if (tilesInfoText == null) return;

        TileBase startingTileBase = startTile != null ? startTile.GetComponent<TileBase>() : null;

        string info = "Special Tiles:\n";
        info += $"Start: {(startTile != null ? $"✓ Found at {startingTileBase?.gridPosition}" : "✗ Not found")}";

        tilesInfoText.text = info;
        Debug.Log($"[LevelDebugUI] Tiles detected - Start: {startTile?.name}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextBoard();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            LoadPreviousBoard();
        }
    }

    private void LoadNextBoard()
    {
        if (LevelManager.Instance == null)
        {
            return;
        }

        int nextBoard = LevelManager.Instance.CurrentBoardIndex + 1;
        if (nextBoard < LevelManager.Instance.CurrentLevelData.boards.Length)
        {
            LevelManager.Instance.LoadBoard(nextBoard);
            Debug.Log($"[LevelDebugUI] Loaded board {nextBoard}");
        }
        else
        {
            Debug.Log("[LevelDebugUI] Already at last board");
        }
    }

    private void LoadPreviousBoard()
    {
        if (LevelManager.Instance == null)
        {
            return;
        }

        int prevBoard = LevelManager.Instance.CurrentBoardIndex - 1;
        if (prevBoard >= 0)
        {
            LevelManager.Instance.LoadBoard(prevBoard);
            Debug.Log($"[LevelDebugUI] Loaded board {prevBoard}");
        }
        else
        {
            Debug.Log("[LevelDebugUI] Already at first board");
        }
    }
}
