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

    private TileGrid ActiveTileGrid => BoardManager.Instance?.ActiveBoard?.TileGrid;


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

        Vector3 spawnWorldPos = ActiveTileGrid.GridToWorldPosition(_currentGridPosition);
        spawnWorldPos.y += heightOffset;

        if (characterPrefab != null)
        {
            _characterInstance = Instantiate(characterPrefab, spawnWorldPos, Quaternion.identity, transform);
            Debug.Log($"[CharacterMover] Character spawned from prefab at {_currentGridPosition}");
        }
        else
        {
            transform.position = spawnWorldPos;
            _characterInstance = gameObject;
            Debug.Log($"[CharacterMover] Character positioned (no prefab) at {_currentGridPosition}");
        }

        Debug.Log($"[CharacterMover] Initialized at board {_currentBoardIndex}, position {_currentGridPosition}, direction: {_moveDirection}");
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
            _isMoving = false;
            Debug.Log("[CharacterMover] Stopped moving");
        }
    }


    private IEnumerator DefaultMoveVisual(
    Transform movingTransform,
    Vector2Int targetGridPos)
    {
        Vector3 startPos = movingTransform.position;

        Vector3 targetPos =
            ActiveTileGrid.GridToWorldPosition(targetGridPos);
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
            ActiveTileGrid.GridToWorldPosition(targetGridPos);
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
            ActiveTileGrid.GridToWorldPosition(targetGridPos);
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

            TileData nextTileData = ActiveTileGrid.GetTileData(nextPosition);
            if (nextTileData == null)
            {
                OnMoveFailed?.Invoke();
                _isMoving = false;
                yield break;
            }

            if (!nextTileData.isWalkable)
            {
                OnMoveFailed?.Invoke();
                _isMoving = false;
                yield break;
            }

            yield return StartCoroutine(MoveStep(nextPosition, nextTileData));

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


    private IEnumerator MoveStep(Vector2Int targetGridPos, TileData tileData)
    {
        Transform movingTransform = _characterInstance != null ? _characterInstance.transform : transform;

        Vector3 startPos = movingTransform.position;
        Vector3 targetPos = ActiveTileGrid.GridToWorldPosition(targetGridPos);
        targetPos.y += heightOffset;
        float elapsedTime = 0f;

        while (elapsedTime < stepDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / stepDuration);
            movingTransform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        movingTransform.position = targetPos;
        _currentGridPosition = targetGridPos;

        TileEffectResult result = TileEffectResolver.Resolve(
            tileData,
            ActiveTileGrid,
            ref _moveDirection,
            _currentGridPosition
        );

        switch (result)
        {
            case TileEffectResult.Win:
                OnGoalReached?.Invoke();
                _isMoving = false;
                break;

            case TileEffectResult.Fail:
                OnMoveFailed?.Invoke();
                _isMoving = false;
                break;

            case TileEffectResult.Teleport:
                if (TileEffectResolver.FindTeleportPair(ActiveTileGrid, tileData.teleportID, _currentGridPosition, out Vector2Int pairPosition, out Vector2Int exitDirection))
                {
                    _currentGridPosition = pairPosition;
                    _moveDirection = exitDirection;
                    
                    Vector3 teleportPos = ActiveTileGrid.GridToWorldPosition(pairPosition);
                    teleportPos.y += heightOffset;
                    movingTransform.position = teleportPos;
                }
                else
                {
                    Debug.LogError($"[CharacterMover] Teleport failed - no pair found for ID {tileData.teleportID}");
                    OnMoveFailed?.Invoke();
                    _isMoving = false;
                }
                break;

            case TileEffectResult.Jump:
                yield return StartCoroutine(HandleJump(tileData, _currentGridPosition));
                break;

            case TileEffectResult.Continue:
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

    private IEnumerator HandleJump(TileData jumpTileData, Vector2Int currentPosition)
    {
        Transform movingTransform = _characterInstance != null ? _characterInstance.transform : transform;
        
        int jumpDistance = jumpTileData.jumpDistance;
        Vector2Int landingPosition = currentPosition + (_moveDirection * (jumpDistance + 1));
        
        Vector3 startPos = movingTransform.position;
        Vector3 endPos = ActiveTileGrid.GridToWorldPosition(landingPosition);
        endPos.y += heightOffset;
        
        float jumpHeight = 0.5f;
        float jumpDuration = stepDuration * 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / jumpDuration);
            
            Vector3 horizontalPos = Vector3.Lerp(startPos, endPos, t);
            
            float parabola = -4f * jumpHeight * t * t + 4f * jumpHeight * t;
            
            movingTransform.position = new Vector3(
                horizontalPos.x,
                startPos.y + parabola,
                horizontalPos.z
            );
            
            yield return null;
        }
        
        movingTransform.position = endPos;
        _currentGridPosition = landingPosition;
        
        TileData landingTileData = ActiveTileGrid.GetTileData(landingPosition);
        if (landingTileData != null)
        {
            TileEffectResult landingResult = TileEffectResolver.Resolve(
                landingTileData,
                ActiveTileGrid,
                ref _moveDirection,
                _currentGridPosition
            );
            
            if (landingResult == TileEffectResult.Fail || landingResult == TileEffectResult.Win)
            {
                yield return StartCoroutine(ProcessTileEffect(landingResult, landingTileData));
            }
        }
    }

    private IEnumerator ProcessTileEffect(TileEffectResult result, TileData tileData)
    {
        switch (result)
        {
            case TileEffectResult.Win:
                OnGoalReached?.Invoke();
                _isMoving = false;
                break;
                
            case TileEffectResult.Fail:
                OnMoveFailed?.Invoke();
                _isMoving = false;
                break;
                
            case TileEffectResult.Jump:
                yield return StartCoroutine(HandleJump(tileData, _currentGridPosition));
                break;
                
            case TileEffectResult.Teleport:
                if (TileEffectResolver.FindTeleportPair(ActiveTileGrid, tileData.teleportID, _currentGridPosition, out Vector2Int pairPosition, out Vector2Int exitDirection))
                {
                    _currentGridPosition = pairPosition;
                    _moveDirection = exitDirection;
                    
                    Transform movingTransform = _characterInstance != null ? _characterInstance.transform : transform;
                    Vector3 teleportPos = ActiveTileGrid.GridToWorldPosition(pairPosition);
                    teleportPos.y += heightOffset;
                    movingTransform.position = teleportPos;
                    
                    Debug.Log($"[CharacterMover] Teleported to {_currentGridPosition}, new direction: {_moveDirection}");
                }
                else
                {
                    OnMoveFailed?.Invoke();
                    _isMoving = false;
                }
                break;
                
            case TileEffectResult.Continue:
                break;
        }
    }
}
