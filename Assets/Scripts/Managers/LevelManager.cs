using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Configuration")]
    [SerializeField] private LevelData[] levels;
    [SerializeField] private int startingLevelIndex = 0;

    [Header("References")]
    [SerializeField] private BoardManager boardManager;

    private int _currentLevelIndex = 0;
    private LevelData _currentLevelData;

    public LevelData CurrentLevelData => _currentLevelData;
    public int CurrentLevelIndex => _currentLevelIndex;
    public int TotalLevels => levels != null ? levels.Length : 0;
    public bool HasNextLevel => _currentLevelIndex < TotalLevels - 1;
    public bool HasPreviousLevel => _currentLevelIndex > 0;

    public int CurrentBoardIndex => boardManager != null ? boardManager.ActiveBoardIndex : -1;
    public BoardData CurrentBoard => boardManager?.ActiveBoard?.BoardData;

    public GameObject StartingTileObject { get; private set; }

    public delegate void LevelLoadedHandler(int levelIndex);
    public event LevelLoadedHandler OnLevelLoaded;

    public delegate void StartingTileDetectedHandler(GameObject startingTile);
    public event StartingTileDetectedHandler OnStartingTileDetected;

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
            boardManager = BoardManager.Instance;

        if (boardManager != null)
            boardManager.OnBoardChanged += OnBoardChanged;

        LoadLevel(startingLevelIndex);
    }

    private void OnDestroy()
    {
        if (boardManager != null)
            boardManager.OnBoardChanged -= OnBoardChanged;

        if (Instance == this)
            Instance = null;
    }

    public void LoadLevel(int levelIndex)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("[LevelManager] No levels assigned!");
            return;
        }

        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError($"[LevelManager] Level index {levelIndex} out of range (0-{levels.Length - 1})!");
            return;
        }

        LevelData levelData = levels[levelIndex];
        if (levelData == null)
        {
            Debug.LogError($"[LevelManager] LevelData at index {levelIndex} is null!");
            return;
        }

        if (levelData.boards == null || levelData.boards.Length == 0)
        {
            Debug.LogError($"[LevelManager] LevelData '{levelData.name}' has no boards!");
            return;
        }

        if (boardManager == null)
        {
            Debug.LogError("[LevelManager] BoardManager not found!");
            return;
        }

        _currentLevelIndex = levelIndex;
        _currentLevelData = levelData;

        boardManager.LoadBoards(levelData.boards);

        FindStartingTile();

        int startingBoardIndex = DetermineStartingBoardIndex();
        boardManager.SetActiveBoard(startingBoardIndex);

        Debug.Log($"[LevelManager] Loaded level {levelIndex} ('{levelData.levelId}'), starting at board {startingBoardIndex}");
        OnLevelLoaded?.Invoke(levelIndex);
    }

    private void FindStartingTile()
    {
        StartingTileObject = null;

        if (boardManager == null)
        {
            Debug.LogWarning("[LevelManager] Cannot find starting tile: BoardManager missing.");
            return;
        }

        for (int i = 0; i < boardManager.BoardCount; i++)
        {
            Board board = boardManager.GetBoard(i);
            if (board?.TileGrid == null) continue;

            GameObject found = FindStartingTileInGrid(board.TileGrid);
            Debug.Log($"[LevelManager] Board {i} TileGrid: {(board.TileGrid != null ? "exists" : "NULL")}, tile count: {board.TileGrid?.GetAllTiles().Count}");
            if (found != null)
            {
                StartingTileObject = found;
                TileBase tb = found.GetComponent<TileBase>();
                Debug.Log($"[LevelManager] Starting tile found on board {i} at grid position {tb?.gridPosition}.");
                OnStartingTileDetected?.Invoke(StartingTileObject);
                return;
            }
        }

        Debug.LogWarning("[LevelManager] No starting tile found across any board in this level.");
        OnStartingTileDetected?.Invoke(null);
    }

    private GameObject FindStartingTileInGrid(TileGrid tileGrid)
    {
        foreach (GameObject tileObj in tileGrid.GetAllTiles().Values)
        {
            if (tileObj == null) continue;

            TileBase tb = tileObj.GetComponent<TileBase>();
            Debug.Log($"Current tile type: {tb.tileData.tileType}");
            if (tb.tileData.tileType == TileType.StartingTile)
                return tileObj;
        }
        return null;
    }

    private int DetermineStartingBoardIndex()
    {
        if (StartingTileObject == null) return 0;

        Board owningBoard = StartingTileObject.GetComponentInParent<Board>();
        if (owningBoard != null)
        {
            return owningBoard.BoardIndex;
        }

        Debug.LogWarning("[LevelManager] Could not resolve owning board from starting tile; defaulting to 0.");
        return 0;
    }

    public void ReloadCurrentLevel()
    {
        Debug.Log($"[LevelManager] Reloading current level {_currentLevelIndex}");
        LoadLevel(_currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        if (!HasNextLevel)
        {
            Debug.LogWarning("[LevelManager] No next level available!");
            return;
        }
        LoadLevel(_currentLevelIndex + 1);
    }

    public void LoadPreviousLevel()
    {
        if (!HasPreviousLevel)
        {
            Debug.LogWarning("[LevelManager] No previous level available!");
            return;
        }
        LoadLevel(_currentLevelIndex - 1);
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

        if (GameStateMachine.Instance != null)
        {
            IGameState currentState = GameStateMachine.Instance.CurrentState;

            if (currentState is State_Setup)
            {
                Debug.Log("[LevelManager] Board changed during Setup - NOT re-triggering Setup.");
            }
            else if (currentState is State_Playing || currentState is State_Transitioning)
            {
                Debug.Log("[LevelManager] Board changed during gameplay - transitioning to Setup.");
                GameStateMachine.Instance.TransitionTo<State_Setup>();
            }
        }
    }

    public GameObject GetTileAt(Vector2Int position)
    {
        if (boardManager?.ActiveBoard?.TileGrid == null) return null;
        return boardManager.ActiveBoard.TileGrid.GetTile(position);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        if (boardManager?.ActiveBoard?.TileGrid == null) return Vector3.zero;
        return boardManager.ActiveBoard.TileGrid.GridToWorldPosition(gridPosition);
    }
}