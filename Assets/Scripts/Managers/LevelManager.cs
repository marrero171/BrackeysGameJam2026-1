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
    public GameObject GoalTileObject { get; private set; }
    public Vector2Int StartingTilePosition => _currentLevelData?.startingTile.position ?? Vector2Int.zero;
    public Vector2Int GoalTilePosition => _currentLevelData?.goalTile.position ?? Vector2Int.zero;

    public delegate void LevelLoadedHandler(int levelIndex);
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

        LoadLevel(startingLevelIndex);
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

        int startingBoardIndex = levelData.startingTile.boardIndex;
        boardManager.SetActiveBoard(startingBoardIndex);

        Debug.Log($"[LevelManager] Loaded level {levelIndex} ('{levelData.levelId}'), starting at board {startingBoardIndex}");
        
        DetectSpecialTiles();
        
        OnLevelLoaded?.Invoke(levelIndex);
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

        int nextIndex = _currentLevelIndex + 1;
        Debug.Log($"[LevelManager] Loading next level {nextIndex}");
        LoadLevel(nextIndex);
    }

    public void LoadPreviousLevel()
    {
        if (!HasPreviousLevel)
        {
            Debug.LogWarning("[LevelManager] No previous level available!");
            return;
        }

        int prevIndex = _currentLevelIndex - 1;
        Debug.Log($"[LevelManager] Loading previous level {prevIndex}");
        LoadLevel(prevIndex);
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
            IGameState currentState = GameStateMachine.Instance.CurrentState;
            
            if (currentState is State_Setup)
            {
                Debug.Log("[LevelManager] Board changed during Setup - NOT transitioning to Setup again");
            }
            else if (currentState is State_Playing || currentState is State_Transitioning)
            {
                Debug.Log("[LevelManager] Board changed during gameplay - transitioning to Setup");
                GameStateMachine.Instance.TransitionTo<State_Setup>();
            }
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

        if (_currentLevelData == null)
        {
            Debug.LogWarning("[LevelManager] Cannot detect special tiles: no level data");
            return;
        }

        TileGrid activeTileGrid = boardManager.ActiveBoard.TileGrid;
        int activeBoardIndex = boardManager.ActiveBoardIndex;

        if (_currentLevelData.startingTile.boardIndex == activeBoardIndex)
        {
            StartingTileObject = activeTileGrid.GetTile(_currentLevelData.startingTile.position);
            
            if (StartingTileObject != null)
            {
                Debug.Log($"[LevelManager] Starting tile detected at {_currentLevelData.startingTile.position}");
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Starting tile not found at {_currentLevelData.startingTile.position}");
            }
        }

        if (_currentLevelData.goalTile.boardIndex == activeBoardIndex)
        {
            GoalTileObject = activeTileGrid.GetTile(_currentLevelData.goalTile.position);
            
            if (GoalTileObject != null)
            {
                Debug.Log($"[LevelManager] Goal tile detected at {_currentLevelData.goalTile.position}");
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Goal tile not found at {_currentLevelData.goalTile.position}");
            }
        }

        OnSpecialTilesDetected?.Invoke(StartingTileObject, GoalTileObject);
    }

    public bool IsStartingTileInCurrentBoard()
    {
        return _currentLevelData != null && 
               _currentLevelData.startingTile.boardIndex == CurrentBoardIndex;
    }

    public bool IsGoalTileInCurrentBoard()
    {
        return _currentLevelData != null && 
               _currentLevelData.goalTile.boardIndex == CurrentBoardIndex;
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
