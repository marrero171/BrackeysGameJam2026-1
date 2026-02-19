using System.Collections;
using UnityEngine;

public class CharacterMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private TileGrid tileGrid;

    [Header("Movement Settings")]
    [SerializeField] private float stepDuration = 0.3f;
    [SerializeField] private float heightOffset = 0.5f;

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

        Vector3 spawnWorldPos = tileGrid.GridToWorldPosition(_currentGridPosition);
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

    private IEnumerator MoveRoutine()
    {
        _isMoving = true;

        while (_isMoving)
        {
            Vector2Int nextPosition = _currentGridPosition + _moveDirection;

            GameObject nextTile = tileGrid.GetTile(nextPosition);
            if (nextTile == null)
            {
                Debug.Log("[CharacterMover] No tile ahead, movement failed");
                OnMoveFailed?.Invoke();
                _isMoving = false;
                yield break;
            }

            TileComponent tileComponent = nextTile.GetComponent<TileComponent>();
            if (tileComponent == null || tileComponent.tileData == null)
            {
                Debug.Log("[CharacterMover] Invalid tile ahead, movement failed");
                OnMoveFailed?.Invoke();
                _isMoving = false;
                yield break;
            }

            if (!tileComponent.tileData.isWalkable)
            {
                Debug.Log($"[CharacterMover] Blocked tile at {nextPosition} - Cannot walk there!");
                OnMoveFailed?.Invoke();
                _isMoving = false;
                yield break;
            }

            yield return StartCoroutine(MoveStep(nextPosition, tileComponent.tileData));

            if (!_isMoving)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator MoveStep(Vector2Int targetGridPos, TileData tileData)
    {
        Transform movingTransform = _characterInstance != null ? _characterInstance.transform : transform;

        Vector3 startPos = movingTransform.position;
        Vector3 targetPos = tileGrid.GridToWorldPosition(targetGridPos);
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

        Debug.Log($"[CharacterMover] Moved to {_currentGridPosition}");

        TileEffectResult result = TileEffectResolver.Resolve(
            tileData.tileType,
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
                if (TileEffectResolver.FindTeleportPair(tileGrid, tileData.teleportID, _currentGridPosition, out Vector2Int pairPosition, out Vector2Int exitDirection))
                {
                    Debug.Log($"[CharacterMover] Teleporting from {_currentGridPosition} to {pairPosition}");
                    
                    _currentGridPosition = pairPosition;
                    _moveDirection = exitDirection;
                    
                    Vector3 teleportPos = tileGrid.GridToWorldPosition(pairPosition);
                    teleportPos.y += heightOffset;
                    movingTransform.position = teleportPos;
                    
                    Debug.Log($"[CharacterMover] Teleported! New position: {_currentGridPosition}, New direction: {_moveDirection}");
                }
                else
                {
                    Debug.LogError($"[CharacterMover] Teleport failed - no pair found for ID {tileData.teleportID}");
                    OnMoveFailed?.Invoke();
                    _isMoving = false;
                }
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
}
