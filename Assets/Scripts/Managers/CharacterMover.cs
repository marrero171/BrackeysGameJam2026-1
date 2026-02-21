using System.Collections;
using UnityEngine;

public class CharacterMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject characterPrefab;

    [Header("Movement Settings")]
    [SerializeField] private float stepDuration = 0.3f;
    [SerializeField] private float heightOffset = 0.5f;
    [Header("Timing")]
    [SerializeField] private float stepPauseDuration = 0.05f;

    // Always uses the board the character is physically on, regardless of which
    // board the camera/player is currently viewing.
    private TileGrid CharacterTileGrid => BoardManager.Instance?.GetBoard(_currentBoardIndex)?.TileGrid;
    public delegate void GoalReachedHandler();
    public event GoalReachedHandler OnGoalReached;

    public delegate void MoveFailedHandler();
    public event MoveFailedHandler OnMoveFailed;

    private GameObject _characterInstance;
    private int _currentBoardIndex;
    private Vector2Int _currentGridPosition;
    private Vector2Int _moveDirection;
    private Coroutine _moveCoroutine;
    private bool _isMoving = false;

    private void Awake()
    {
        OnGoalReached += HandleGoalReached;
        OnMoveFailed += HandleMoveFailed;
    }

    private void OnDestroy()
    {
        OnGoalReached -= HandleGoalReached;
        OnMoveFailed -= HandleMoveFailed;
        if (BoardManager.Instance != null)
            BoardManager.Instance.OnBoardChanged -= OnBoardChanged;
    }

    private void Start()
    {
        if (BoardManager.Instance != null)
            BoardManager.Instance.OnBoardChanged += OnBoardChanged;
    }

    private void OnBoardChanged(int previousIndex, int newIndex)
    {
        if (_characterInstance == null) return;

        bool onActiveBoard = (newIndex == _currentBoardIndex);
        SetCharacterVisible(onActiveBoard);

        Debug.Log($"[CharacterMover] Board changed to {newIndex}. " +
                  $"Character on board {_currentBoardIndex} → " +
                  $"{(onActiveBoard ? "visible" : "hidden")}");
    }
    private void SetCharacterVisible(bool visible)
    {
        if (_characterInstance == null) return;

        foreach (Renderer r in _characterInstance.GetComponentsInChildren<Renderer>(true))
            r.enabled = visible;
    }

    public void SpawnCharacter()
    {
        StopMoving();
        DespawnCharacter();
        
        if (LevelManager.Instance == null || LevelManager.Instance.CurrentLevelData == null)
        {
            Debug.LogError("[CharacterMover] LevelManager or LevelData not found!");
            return;
        }

        LevelData levelData = LevelManager.Instance.CurrentLevelData;

        _currentBoardIndex = levelData.startingTile.boardIndex;
        _currentGridPosition = levelData.startingTile.position;
        _moveDirection = new Vector2Int(
            (int)levelData.characterStartDirection.x,
            (int)levelData.characterStartDirection.z
        );

        if (BoardManager.Instance == null)
        {
            Debug.LogError("[CharacterMover] BoardManager not found!");
            return;
        }

        if (BoardManager.Instance.ActiveBoardIndex != _currentBoardIndex)
        {
            Debug.LogWarning($"[CharacterMover] Active board ({BoardManager.Instance.ActiveBoardIndex}) doesn't match starting board ({_currentBoardIndex}). Switching...");
            BoardManager.Instance.SetActiveBoard(_currentBoardIndex, false);
        }

        Board targetBoard = BoardManager.Instance.GetBoard(_currentBoardIndex);
        if (targetBoard == null || targetBoard.TileGrid == null)
        {
            Debug.LogError($"[CharacterMover] Board {_currentBoardIndex} or its TileGrid not found!");
            return;
        }

        Vector3 spawnWorldPos = targetBoard.TileGrid.GridToWorldPosition(_currentGridPosition);
        spawnWorldPos.y += heightOffset;

        if (characterPrefab != null)
        {
            _characterInstance = Instantiate(characterPrefab, spawnWorldPos, Quaternion.identity, transform);
            Debug.Log($"[CharacterMover] Character spawned from prefab at board {_currentBoardIndex}, position {_currentGridPosition}");
        }
        else
        {
            transform.position = spawnWorldPos;
            _characterInstance = gameObject;
            Debug.Log($"[CharacterMover] Character positioned (no prefab) at board {_currentBoardIndex}, position {_currentGridPosition}");
        }

        bool onActiveBoard = (BoardManager.Instance.ActiveBoardIndex == _currentBoardIndex);
        SetCharacterVisible(onActiveBoard);

        Debug.Log($"[CharacterMover] Character fully initialized - Board: {_currentBoardIndex}, Position: {_currentGridPosition}, Direction: {_moveDirection}, Visible: {onActiveBoard}");
    }

    public void DespawnCharacter()
    {
        if (_characterInstance != null && _characterInstance != gameObject)
        {
            Destroy(_characterInstance);
            _characterInstance = null;
            Debug.Log("[CharacterMover] Character despawned");
        }
    }

    private void HandleGoalReached()
    {
        Debug.Log("[CharacterMover] Goal reached, transitioning to State_Win");
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.TransitionTo<State_Win>();
        }
    }

    private void HandleMoveFailed()
    {
        Debug.Log("[CharacterMover] Move failed, transitioning to State_Fail");
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.TransitionTo<State_Fail>();
        }
    }

    public void StartMoving()
    {
        if (_moveCoroutine == null)
        {
            _moveCoroutine = StartCoroutine(MoveRoutine());
            Debug.Log("[CharacterMover] Started moving");
        }
    }

    public void StopMoving()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        
        StopAllCoroutines();
        _isMoving = false;
        Debug.Log("[CharacterMover] Stopped moving - all coroutines stopped");
    }


    private IEnumerator DefaultMoveVisual(
    Transform movingTransform,
    Vector2Int targetGridPos)
    {
        Vector3 startPos = movingTransform.position;

        Vector3 targetPos =
            CharacterTileGrid.GridToWorldPosition(targetGridPos);
        targetPos.y += heightOffset;

        float elapsedTime = 0f;

        while (elapsedTime < stepDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / stepDuration);

            movingTransform.position =
                Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        movingTransform.position = targetPos;
    }

    private IEnumerator JumpVisual(
    Transform movingTransform,
    Vector2Int targetGridPos)
    {
        Vector3 startPos = movingTransform.position;

        Vector3 targetPos =
            CharacterTileGrid.GridToWorldPosition(targetGridPos);
        targetPos.y += heightOffset;

        float jumpHeight = 1.0f;
        float elapsedTime = 0f;

        while (elapsedTime < stepDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / stepDuration);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

            // Parabolic arc
            pos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            movingTransform.position = pos;

            yield return null;
        }

        movingTransform.position = targetPos;
    }

    private IEnumerator TeleportVisual(
    Transform movingTransform,
    Vector2Int targetGridPos)
    {
        Vector3 originalScale = movingTransform.localScale;
        Vector3 shrinkScale = Vector3.zero;

        Vector3 targetPos =
            CharacterTileGrid.GridToWorldPosition(targetGridPos);
        targetPos.y += heightOffset;

        float duration = stepDuration * 0.4f;
        float t = 0f;

        // Shrink
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            movingTransform.localScale =
                Vector3.Lerp(originalScale, shrinkScale, t);
            yield return null;
        }

        // Instant relocation
        movingTransform.position = targetPos;

        t = 0f;

        // Grow back
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            movingTransform.localScale =
                Vector3.Lerp(shrinkScale, originalScale, t);
            yield return null;
        }

        movingTransform.localScale = originalScale;
    }


    private IEnumerator MoveRoutine()
    {
        _isMoving = true;

        while (_isMoving)
        {
            Vector2Int nextPosition = _currentGridPosition + _moveDirection;

            GameObject nextTile = CharacterTileGrid.GetTile(nextPosition);
            if (nextTile == null)
            {
                Debug.Log("[CharacterMover] No tile ahead, movement failed");
                OnMoveFailed?.Invoke();
                _isMoving = false;
                yield break;
            }

            TileBase tileBase = nextTile.GetComponent<TileBase>();
            if (tileBase == null || tileBase.tileData == null)
            {
                Debug.Log("[CharacterMover] Invalid tile ahead, movement failed");
                OnMoveFailed?.Invoke();
                _isMoving = false;
                yield break;
            }

            if (!tileBase.tileData.isWalkable)
            {
                Debug.Log($"[CharacterMover] Blocked tile at {nextPosition} - Cannot walk there!");
                OnMoveFailed?.Invoke();
                _isMoving = false;
                yield break;
            }

            yield return StartCoroutine(MoveStep(nextPosition, tileBase.tileData));

            if (!_isMoving)
            {
                yield break;
            }

            yield return new WaitForSeconds(stepPauseDuration);

        }
    }

    private IEnumerator PlayVisual(
    TileEffectVisual visual,
    Transform movingTransform,
    Vector2Int targetGridPos)
    {
        switch (visual)
        {
            case TileEffectVisual.Teleport:
                yield return TeleportVisual(
                    movingTransform,
                    targetGridPos);
                break;

            case TileEffectVisual.Jump:
                yield return JumpVisual(
                    movingTransform,
                    targetGridPos);
                break;

            default:
                yield return DefaultMoveVisual(
                    movingTransform,
                    targetGridPos);
                break;
        }
    }


    private IEnumerator MoveStep(
    Vector2Int targetGridPos,
    TileData tileData)
    {
        Transform movingTransform =
            _characterInstance != null
            ? _characterInstance.transform
            : transform;

        yield return StartCoroutine(
            DefaultMoveVisual(
                movingTransform,
                targetGridPos));

        _currentGridPosition = targetGridPos;

        TileEffectResult result = TileEffectResult.Continue;

        bool resolving = true;
        bool justUsedPortal = false;

        while (resolving)
        {
            TileData currentTile =
                CharacterTileGrid.GetTileData(_currentGridPosition);

            TileEffectContext context = new TileEffectContext
            {
                tileGrid = CharacterTileGrid,
                currentBoardIndex = _currentBoardIndex,
                position = _currentGridPosition,
                direction = _moveDirection,
                tileData = currentTile,
                switchedBoard = false,
                targetBoardIndex = -1,
                justUsedPortal = justUsedPortal
            };

            Vector2Int previousPosition = context.position;
            int previousBoardIndex = _currentBoardIndex;

            result = TileEffectResolver.Resolve(ref context);

            justUsedPortal = context.justUsedPortal;

            Debug.Log(
                $"[MoveStep] Chain resolve -> " +
                $"from={previousPosition} " +
                $"to={context.position} " +
                $"board={context.currentBoardIndex} " +
                $"justUsedPortal={justUsedPortal} " +
                $"visual={context.visualEffect}");

            _moveDirection = context.direction;

            if (context.switchedBoard && context.targetBoardIndex != -1)
            {
                Debug.Log($"[CharacterMover] Portal used! Switching from board {_currentBoardIndex} to {context.targetBoardIndex}");
                
                _currentBoardIndex = context.targetBoardIndex;
                
                if (BoardManager.Instance != null)
                {
                    BoardManager.Instance.SetActiveBoard(_currentBoardIndex, false);
                }
                
                SetCharacterVisible(true);
            }

            if (context.visualEffect != TileEffectVisual.None &&
                (context.position != previousPosition || context.switchedBoard))
            {
                yield return StartCoroutine(
                    PlayVisual(
                        context.visualEffect,
                        movingTransform,
                        context.position));

                _currentGridPosition = context.position;
            }

            resolving = context.position != previousPosition;
        }

        Debug.Log($"[CharacterMover] Finished moving to {_currentGridPosition}");

        switch (result)
        {
            case TileEffectResult.Win:
                _isMoving = false;
                OnGoalReached?.Invoke();
                break;

            case TileEffectResult.Fail:
                _isMoving = false;
                OnMoveFailed?.Invoke();
                break;
        }
    }


    public Vector2Int GetCurrentPosition()
    {
        return _currentGridPosition;
    }

    public int GetCurrentBoardIndex()
    {
        return _currentBoardIndex;
    }

    public Vector2Int GetCurrentDirection()
    {
        return _moveDirection;
    }

    public void SetDirection(Vector2Int newDirection)
    {
        _moveDirection = newDirection;
        Debug.Log($"[CharacterMover] Direction changed to {_moveDirection}");
    }
}