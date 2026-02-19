using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private LevelData levelData;
    [SerializeField] private TileGrid tileGrid;

    [Header("Current State")]
    [SerializeField] private int currentBoardIndex = 0;

    public LevelData CurrentLevelData => levelData;
    public int CurrentBoardIndex => currentBoardIndex;
    public BoardData CurrentBoard => levelData?.boards[currentBoardIndex];
    
    public GameObject StartingTileObject { get; private set; }
    public GameObject GoalTileObject { get; private set; }
    public Vector2Int StartingTilePosition => levelData?.startingTile.position ?? Vector2Int.zero;
    public Vector2Int GoalTilePosition => levelData?.goalTile.position ?? Vector2Int.zero;

    public delegate void LevelLoadedHandler();
    public event LevelLoadedHandler OnLevelLoaded;

    public delegate void SpecialTilesDetectedHandler(GameObject startingTile, GameObject goalTile);
    public event SpecialTilesDetectedHandler OnSpecialTilesDetected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (tileGrid != null)
        {
            tileGrid.OnTilesInstantiated += OnTilesReady;
        }

        LoadLevel();
    }

    private void OnDestroy()
    {
        if (tileGrid != null)
        {
            tileGrid.OnTilesInstantiated -= OnTilesReady;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadLevel()
    {
        if (levelData == null)
        {
            Debug.LogError("[LevelManager] No LevelData assigned!");
            return;
        }

        if (levelData.boards == null || levelData.boards.Length == 0)
        {
            Debug.LogError("[LevelManager] LevelData has no boards!");
            return;
        }

        currentBoardIndex = levelData.startingTile.boardIndex;
        
        if (tileGrid != null)
        {
            tileGrid.SetBoardData(CurrentBoard);
            tileGrid.InstantiateTiles();
        }

        Debug.Log($"[LevelManager] Loading level '{levelData.levelId}', starting at board {currentBoardIndex}");
        
        OnLevelLoaded?.Invoke();
    }

    public void LoadBoard(int boardIndex)
    {
        if (levelData == null || levelData.boards == null)
        {
            Debug.LogError("[LevelManager] Cannot load board: no level data!");
            return;
        }

        if (boardIndex < 0 || boardIndex >= levelData.boards.Length)
        {
            Debug.LogError($"[LevelManager] Board index {boardIndex} out of range!");
            return;
        }

        currentBoardIndex = boardIndex;
        
        if (tileGrid != null)
        {
            tileGrid.SetBoardData(CurrentBoard);
            tileGrid.InstantiateTiles();
        }

        Debug.Log($"[LevelManager] Loaded board {boardIndex}");
    }

    private void OnTilesReady()
    {
        DetectSpecialTiles();
    }

    private void DetectSpecialTiles()
    {
        StartingTileObject = null;
        GoalTileObject = null;

        if (levelData.startingTile.boardIndex == currentBoardIndex)
        {
            StartingTileObject = tileGrid.GetTile(levelData.startingTile.position);
            
            if (StartingTileObject != null)
            {
                Debug.Log($"[LevelManager] Starting tile detected at {levelData.startingTile.position}");
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Starting tile not found at {levelData.startingTile.position}");
            }
        }

        if (levelData.goalTile.boardIndex == currentBoardIndex)
        {
            GoalTileObject = tileGrid.GetTile(levelData.goalTile.position);
            
            if (GoalTileObject != null)
            {
                Debug.Log($"[LevelManager] Goal tile detected at {levelData.goalTile.position}");
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Goal tile not found at {levelData.goalTile.position}");
            }
        }

        OnSpecialTilesDetected?.Invoke(StartingTileObject, GoalTileObject);
    }

    public bool IsStartingTileInCurrentBoard()
    {
        return levelData.startingTile.boardIndex == currentBoardIndex;
    }

    public bool IsGoalTileInCurrentBoard()
    {
        return levelData.goalTile.boardIndex == currentBoardIndex;
    }

    public GameObject GetTileAt(Vector2Int position)
    {
        return tileGrid?.GetTile(position);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return tileGrid?.GridToWorldPosition(gridPosition) ?? Vector3.zero;
    }
}
