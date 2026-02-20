using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private LevelData levelData;
    [SerializeField] private BoardManager boardManager;

    public LevelData CurrentLevelData => levelData;
    public int CurrentBoardIndex => boardManager != null ? boardManager.ActiveBoardIndex : -1;
    public BoardData CurrentBoard => boardManager?.ActiveBoard?.BoardData;
    
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
        if (boardManager == null)
        {
            boardManager = BoardManager.Instance;
        }

        if (boardManager != null)
        {
            boardManager.OnBoardChanged += OnBoardChanged;
        }

        LoadLevel();
    }

    private void OnDestroy()
    {
        if (boardManager != null)
        {
            boardManager.OnBoardChanged -= OnBoardChanged;
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

        if (boardManager == null)
        {
            Debug.LogError("[LevelManager] BoardManager not found!");
            return;
        }

        boardManager.LoadBoards(levelData.boards);

        int startingBoardIndex = levelData.startingTile.boardIndex;
        boardManager.SetActiveBoard(startingBoardIndex);

        Debug.Log($"[LevelManager] Loading level '{levelData.levelId}', starting at board {startingBoardIndex}");
        
        DetectSpecialTiles();
        
        OnLevelLoaded?.Invoke();
    }

    public void LoadBoard(int boardIndex)
    {
        if (boardManager == null)
        {
            Debug.LogError("[LevelManager] BoardManager not found!");
            return;
        }

        if (!boardManager.CanSwitchToBoard(boardIndex))
        {
            Debug.LogError($"[LevelManager] Cannot switch to board {boardIndex}");
            return;
        }

        boardManager.SetActiveBoard(boardIndex);
    }

    private void OnBoardChanged(int previousIndex, int newIndex)
    {
        Debug.Log($"[LevelManager] Board changed from {previousIndex} to {newIndex}");
        DetectSpecialTiles();
        
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.TransitionTo<State_Setup>();
        }
    }

    private void DetectSpecialTiles()
    {
        StartingTileObject = null;
        GoalTileObject = null;

        if (boardManager == null || boardManager.ActiveBoard == null)
        {
            Debug.LogWarning("[LevelManager] Cannot detect special tiles: no active board");
            return;
        }

        TileGrid activeTileGrid = boardManager.ActiveBoard.TileGrid;
        int activeBoardIndex = boardManager.ActiveBoardIndex;

        if (levelData.startingTile.boardIndex == activeBoardIndex)
        {
            StartingTileObject = activeTileGrid.GetTile(levelData.startingTile.position);
            
            if (StartingTileObject != null)
            {
                Debug.Log($"[LevelManager] Starting tile detected at {levelData.startingTile.position}");
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Starting tile not found at {levelData.startingTile.position}");
            }
        }

        if (levelData.goalTile.boardIndex == activeBoardIndex)
        {
            GoalTileObject = activeTileGrid.GetTile(levelData.goalTile.position);
            
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
        return levelData.startingTile.boardIndex == CurrentBoardIndex;
    }

    public bool IsGoalTileInCurrentBoard()
    {
        return levelData.goalTile.boardIndex == CurrentBoardIndex;
    }

    public GameObject GetTileAt(Vector2Int position)
    {
        if (boardManager?.ActiveBoard?.TileGrid == null)
            return null;

        return boardManager.ActiveBoard.TileGrid.GetTile(position);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        if (boardManager?.ActiveBoard?.TileGrid == null)
            return Vector3.zero;

        return boardManager.ActiveBoard.TileGrid.GridToWorldPosition(gridPosition);
    }
}
