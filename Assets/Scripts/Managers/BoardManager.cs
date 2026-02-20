using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Board Configuration")]
    [SerializeField] private Transform boardsContainer;

    private List<Board> _boards = new List<Board>();
    private int _activeBoardIndex = -1;

    public Board ActiveBoard => _activeBoardIndex >= 0 && _activeBoardIndex < _boards.Count 
        ? _boards[_activeBoardIndex] 
        : null;

    public int ActiveBoardIndex => _activeBoardIndex;
    public int BoardCount => _boards.Count;

    public delegate void BoardChangedHandler(int previousIndex, int newIndex);
    public event BoardChangedHandler OnBoardChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (boardsContainer == null)
        {
            boardsContainer = transform;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadBoards(BoardData[] boardsData)
    {
        ClearAllBoards();

        if (boardsData == null || boardsData.Length == 0)
        {
            Debug.LogWarning("[BoardManager] No boards data provided to load");
            return;
        }

        for (int i = 0; i < boardsData.Length; i++)
        {
            if (boardsData[i] == null)
            {
                Debug.LogWarning($"[BoardManager] BoardData at index {i} is null, skipping");
                continue;
            }

            CreateBoard(boardsData[i], i);
        }

        Debug.Log($"[BoardManager] Loaded {_boards.Count} boards");

        if (_boards.Count > 0)
        {
            SetActiveBoard(0, false);
        }
    }

    private void CreateBoard(BoardData boardData, int index)
    {
        GameObject boardObject = new GameObject($"Board_{index}");
        boardObject.transform.SetParent(boardsContainer);
        boardObject.transform.localPosition = Vector3.zero;

        Board board = boardObject.AddComponent<Board>();
        board.Initialize(boardData, index);

        _boards.Add(board);

        Debug.Log($"[BoardManager] Created board {index}: {boardData.name}");
    }

    public void SetActiveBoard(int boardIndex, bool notifyListeners = true)
    {
        if (boardIndex < 0 || boardIndex >= _boards.Count)
        {
            Debug.LogError($"[BoardManager] Board index {boardIndex} out of range (0-{_boards.Count - 1})");
            return;
        }

        int previousIndex = _activeBoardIndex;

        if (ActiveBoard != null)
        {
            ActiveBoard.Hide();
        }

        _activeBoardIndex = boardIndex;
        ActiveBoard.Show();

        Debug.Log($"[BoardManager] Switched to board {boardIndex}: {ActiveBoard.BoardData.name}");

        if (notifyListeners && previousIndex != boardIndex)
        {
            OnBoardChanged?.Invoke(previousIndex, boardIndex);
        }
    }

    public Board GetBoard(int index)
    {
        if (index >= 0 && index < _boards.Count)
        {
            return _boards[index];
        }
        return null;
    }

    public List<Board> GetAllBoards()
    {
        return new List<Board>(_boards);
    }

    public void ClearAllBoards()
    {
        foreach (Board board in _boards)
        {
            if (board != null)
            {
                board.Clear();
                Destroy(board.gameObject);
            }
        }

        _boards.Clear();
        _activeBoardIndex = -1;

        Debug.Log("[BoardManager] All boards cleared");
    }

    public bool CanSwitchToBoard(int boardIndex)
    {
        return boardIndex >= 0 && boardIndex < _boards.Count;
    }

    public void SwitchToNextBoard()
    {
        if (_boards.Count <= 1) return;

        int nextIndex = (_activeBoardIndex + 1) % _boards.Count;
        SetActiveBoard(nextIndex);
    }

    public void SwitchToPreviousBoard()
    {
        if (_boards.Count <= 1) return;

        int prevIndex = (_activeBoardIndex - 1 + _boards.Count) % _boards.Count;
        SetActiveBoard(prevIndex);
    }
}
