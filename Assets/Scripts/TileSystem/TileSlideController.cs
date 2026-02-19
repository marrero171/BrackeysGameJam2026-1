using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileSlideController : MonoBehaviour
{
    [SerializeField] private TileGrid tileGrid;
    [SerializeField] private float slideDuration = 0.15f;
    [SerializeField] private int maxUndoSteps = 20;

    public delegate void TileMovedHandler(Vector2Int fromPosition, Vector2Int toPosition);
    public event TileMovedHandler OnTileMoved;

    private List<Vector2Int> _emptyPositions = new List<Vector2Int>();
    private Stack<MoveRecord> _undoStack = new Stack<MoveRecord>();
    private bool _isAnimating = false;

    private struct MoveRecord
    {
        public Vector2Int tilePosition;
        public Vector2Int emptyPosition;
    }

    private void Start()
    {
        if (tileGrid != null)
        {
            tileGrid.OnTilesInstantiated += FindAllEmptyPositions;
        }
    }

    private void OnDestroy()
    {
        if (tileGrid != null)
        {
            tileGrid.OnTilesInstantiated -= FindAllEmptyPositions;
        }
    }

    public bool TrySlide(Vector2Int clickedTilePosition)
    {
        if (_isAnimating)
        {
            return false;
        }

        GameObject clickedTile = tileGrid.GetTile(clickedTilePosition);
        if (clickedTile == null)
        {
            return false;
        }

        TileComponent tileComponent = clickedTile.GetComponent<TileComponent>();
        if (tileComponent == null || tileComponent.tileData == null)
        {
            return false;
        }

        if (!tileComponent.tileData.IsMovable())
        {
            Debug.Log($"[TileSlideController] Tile at {clickedTilePosition} is not movable (type: {tileComponent.tileData.tileType})");
            return false;
        }

        Vector2Int adjacentEmpty = GetAdjacentEmptyPosition(clickedTilePosition);
        if (adjacentEmpty == new Vector2Int(-1, -1))
        {
            return false;
        }

        PushUndoRecord(clickedTilePosition, adjacentEmpty);

        StartCoroutine(AnimateSlide(clickedTilePosition, adjacentEmpty));

        return true;
    }

    public bool TrySlideInDirection(Vector2Int tilePosition, Vector2Int direction)
    {
        Debug.Log($"[TileSlideController] === TrySlideInDirection ===");
        Debug.Log($"  Tile Position: {tilePosition}");
        Debug.Log($"  Direction: {direction}");
        Debug.Log($"  Empty Positions: {string.Join(", ", _emptyPositions)}");
        
        if (_isAnimating)
        {
            Debug.Log($"  BLOCKED: Currently animating");
            return false;
        }

        GameObject tile = tileGrid.GetTile(tilePosition);
        if (tile == null)
        {
            Debug.Log($"  BLOCKED: Tile not found at {tilePosition}");
            return false;
        }

        TileComponent tileComponent = tile.GetComponent<TileComponent>();
        if (tileComponent == null || tileComponent.tileData == null)
        {
            Debug.Log($"  BLOCKED: No TileComponent or TileData");
            return false;
        }

        if (!tileComponent.tileData.IsMovable())
        {
            Debug.Log($"  BLOCKED: Tile is not movable (type: {tileComponent.tileData.tileType})");
            return false;
        }

        Vector2Int targetPosition = tilePosition + direction;
        Debug.Log($"  Target Position (tile + direction): {targetPosition}");

        if (!_emptyPositions.Contains(targetPosition))
        {
            Debug.Log($"  BLOCKED: Target {targetPosition} is not an empty position");
            return false;
        }

        Debug.Log($"  ✅ SUCCESS: Slide approved to empty at {targetPosition}!");
        PushUndoRecord(tilePosition, targetPosition);
        StartCoroutine(AnimateSlide(tilePosition, targetPosition));
        return true;
    }

    public void Undo()
    {
        if (_undoStack.Count == 0 || _isAnimating)
        {
            return;
        }

        MoveRecord lastMove = _undoStack.Pop();

        StartCoroutine(AnimateSlide(lastMove.emptyPosition, lastMove.tilePosition, true));
    }

    public void ClearUndoHistory()
    {
        _undoStack.Clear();
    }

    public int GetUndoCount()
    {
        return _undoStack.Count;
    }

    public List<Vector2Int> GetEmptyPositions()
    {
        return new List<Vector2Int>(_emptyPositions);
    }

    public Vector2Int GetEmptyPosition()
    {
        return _emptyPositions.Count > 0 ? _emptyPositions[0] : new Vector2Int(-1, -1);
    }

    private Vector2Int GetAdjacentEmptyPosition(Vector2Int position)
    {
        Vector2Int[] adjacentOffsets = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int offset in adjacentOffsets)
        {
            Vector2Int checkPos = position + offset;
            if (_emptyPositions.Contains(checkPos))
            {
                return checkPos;
            }
        }

        return new Vector2Int(-1, -1);
    }

    private void PushUndoRecord(Vector2Int tilePos, Vector2Int emptyPos)
    {
        MoveRecord record = new MoveRecord
        {
            tilePosition = tilePos,
            emptyPosition = emptyPos
        };

        _undoStack.Push(record);

        if (_undoStack.Count > maxUndoSteps)
        {
            Stack<MoveRecord> tempStack = new Stack<MoveRecord>();
            for (int i = 0; i < maxUndoSteps; i++)
            {
                tempStack.Push(_undoStack.Pop());
            }
            _undoStack.Clear();
            while (tempStack.Count > 0)
            {
                _undoStack.Push(tempStack.Pop());
            }
        }
    }

    private IEnumerator AnimateSlide(Vector2Int fromPosition, Vector2Int toPosition, bool isUndo = false)
    {
        _isAnimating = true;

        GameObject movingTile = tileGrid.GetTile(fromPosition);
        if (movingTile == null)
        {
            _isAnimating = false;
            yield break;
        }

        Vector3 startWorldPos = movingTile.transform.position;
        Vector3 targetWorldPos = tileGrid.GetTile(toPosition).transform.position;

        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / slideDuration);
            
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            movingTile.transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, smoothT);

            yield return null;
        }

        movingTile.transform.position = targetWorldPos;

        tileGrid.SwapTiles(fromPosition, toPosition);

        _emptyPositions.Remove(toPosition);
        _emptyPositions.Add(fromPosition);
        
        Debug.Log($"[TileSlideController] Updated empty positions: {string.Join(", ", _emptyPositions)}");

        _isAnimating = false;

        if (!isUndo)
        {
            OnTileMoved?.Invoke(fromPosition, toPosition);
        }
    }

    private void FindAllEmptyPositions()
    {
        _emptyPositions.Clear();
        Debug.Log("[TileSlideController] === Finding ALL Empty Positions ===");
        
        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                GameObject tile = tileGrid.GetTile(gridPos);

                if (tile != null)
                {
                    TileComponent tileComponent = tile.GetComponent<TileComponent>();
                    if (tileComponent != null && 
                        tileComponent.tileData != null && 
                        tileComponent.tileData.tileType == TileType.Empty)
                    {
                        _emptyPositions.Add(gridPos);
                        Debug.Log($"[TileSlideController] ✅ Empty tile found at {gridPos}");
                    }
                }
            }
        }

        if (_emptyPositions.Count == 0)
        {
            Debug.LogError("[TileSlideController] ❌ No Empty tiles found in grid!");
        }
        else
        {
            Debug.Log($"[TileSlideController] Total empty tiles found: {_emptyPositions.Count}");
        }
    }
}
