using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TileSlideController slideController;

    [Header("Drag Settings")]
    [SerializeField] private float minDragDistance = 20f;
    [SerializeField] private LayerMask tileLayerMask = -1;

    public delegate void TileSelectedHandler(Vector2Int tilePosition, GameObject tileObject);
    public event TileSelectedHandler OnTileSelected;

    private Vector2 _mouseDownPosition;
    private Vector2 _currentMousePosition;
    private bool _isMouseDown = false;
    private GameObject _clickedTile;
    private Vector2Int _clickedTileGridPosition;
    private Vector2Int _lastSelectedTilePosition = new Vector2Int(-1, -1);
    private bool _inputEnabled = true;

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
        Debug.Log($"[InputManager] Input {(enabled ? "enabled" : "disabled")}");
    }

    private void Update()
    {
        HandleMouseInput();
        HandleUndoInput();
    }

    private void HandleMouseInput()
    {
        if (!_inputEnabled)
        {
            return;
        }

        // ✅ NUEVO: Check if mouse is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Mouse is over UI, don't process tile input
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _mouseDownPosition = Input.mousePosition;
            _currentMousePosition = _mouseDownPosition;
            _isMouseDown = true;

            RaycastTile(_mouseDownPosition, out _clickedTile, out _clickedTileGridPosition);
        }

        if (_isMouseDown)
        {
            _currentMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && _isMouseDown)
        {
            _isMouseDown = false;

            if (_clickedTile == null)
            {
                return;
            }

            Vector2 dragDelta = _currentMousePosition - _mouseDownPosition;
            float dragDistance = dragDelta.magnitude;

            if (dragDistance >= minDragDistance)
            {
                HandleDrag(dragDelta);
            }
            else
            {
                HandleClick();
            }

            _clickedTile = null;
        }
    }

    private void HandleUndoInput()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (slideController != null)
            {
                slideController.Undo();
            }
        }
    }

    private void HandleClick()
    {
        Debug.Log($"[InputManager] === CLICK DETECTED === Tile: {_clickedTile?.name} at {_clickedTileGridPosition}");
        _lastSelectedTilePosition = _clickedTileGridPosition;
        OnTileSelected?.Invoke(_clickedTileGridPosition, _clickedTile);
    }

    private void HandleDrag(Vector2 dragDelta)
    {
        Debug.Log($"[InputManager] === DRAG DETECTED === Tile: {_clickedTile?.name} at {_clickedTileGridPosition}");
        Debug.Log($"[InputManager] Last selected tile position: {_lastSelectedTilePosition}");
        Debug.Log($"[InputManager] Current clicked tile position: {_clickedTileGridPosition}");
        
        bool isAlreadySelected = (_clickedTileGridPosition == _lastSelectedTilePosition);
        Debug.Log($"[InputManager] Is already selected? {isAlreadySelected}");
        
        if (!isAlreadySelected)
        {
            _lastSelectedTilePosition = _clickedTileGridPosition;
            OnTileSelected?.Invoke(_clickedTileGridPosition, _clickedTile);
            Debug.Log($"[InputManager] Triggered OnTileSelected event");
        }
        else
        {
            Debug.Log($"[InputManager] Tile already selected, skipping selection event");
        }
        
        Vector2Int gridDirection = ScreenDragToIsometricDirection(dragDelta);
        Debug.Log($"[InputManager] Grid direction calculated: {gridDirection}");

        if (gridDirection == Vector2Int.zero)
        {
            Debug.Log($"[InputManager] Grid direction is zero, aborting slide");
            return;
        }

        Debug.Log($"[InputManager] Calling TrySlideInDirection({_clickedTileGridPosition}, {gridDirection})");
        bool slideSuccess = slideController.TrySlideInDirection(_clickedTileGridPosition, gridDirection);

        if (!slideSuccess)
        {
            Debug.Log($"[InputManager] Cannot slide tile {_clickedTileGridPosition} in direction {gridDirection}");
        }
        else
        {
            Debug.Log($"[InputManager] Slide succeeded!");
        }
    }

    private Vector2Int ScreenDragToIsometricDirection(Vector2 screenDrag)
    {
        Vector2 dragNormalized = screenDrag.normalized;
        
        float angle = Mathf.Atan2(dragNormalized.y, dragNormalized.x) * Mathf.Rad2Deg;
        
        Debug.Log($"[InputManager] Screen drag: {screenDrag}, Normalized: {dragNormalized}, Angle: {angle}°");
        
        
        if (angle >= 0f && angle < 90f)
        {
            Debug.Log($"[InputManager] Direction: RIGHT (+X)");
            return Vector2Int.right;
        }
        else if (angle >= 90f && angle <= 180f)
        {
            Debug.Log($"[InputManager] Direction: FORWARD (+Z)");
            return Vector2Int.up;
        }
        else if (angle >= -180f && angle < -90f)
        {
            Debug.Log($"[InputManager] Direction: LEFT (-X)");
            return Vector2Int.left;
        }
        else if (angle >= -90f && angle < 0f)
        {
            Debug.Log($"[InputManager] Direction: BACKWARD (-Z)");
            return Vector2Int.down;
        }
        else
        {
            Debug.Log($"[InputManager] Direction: Not Found");
            return Vector2Int.zero;
        }
    }

    private bool RaycastTile(Vector2 screenPosition, out GameObject tileObject, out Vector2Int gridPosition)
    {
        tileObject = null;
        gridPosition = Vector2Int.zero;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayerMask))
        {
            if (hit.transform.TryGetComponent<TileComponent>(out var tileComponent))
            {
                if (tileComponent.tileData != null && !tileComponent.tileData.IsSelectable())
                {
                    Debug.Log($"[InputManager] Tile at {tileComponent.gridPosition} is not selectable (type: {tileComponent.tileData.tileType})");
                    return false;
                }
                
                tileObject = hit.transform.gameObject;
                gridPosition = tileComponent.gridPosition;
                return true;
            }
        }

        return false;
    }
}
