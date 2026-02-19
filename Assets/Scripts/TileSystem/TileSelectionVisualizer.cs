using UnityEngine;

public class TileSelectionVisualizer : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

    private TileBase _selectedTile;

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnTileSelected += HandleTileSelected;
            Debug.Log("[TileSelectionVisualizer] Subscribed to OnTileSelected event");
        }
        else
        {
            Debug.LogError("[TileSelectionVisualizer] InputManager is NULL!");
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnTileSelected -= HandleTileSelected;
        }

        ClearSelection();
    }

    private void HandleTileSelected(Vector2Int tilePosition, GameObject tileObject)
    {
        Debug.Log($"[TileSelectionVisualizer] === EVENT RECEIVED === Tile: {tileObject?.name} at {tilePosition}");
        
        ClearSelection();

        if (tileObject != null)
        {
            _selectedTile = tileObject.GetComponent<TileBase>();
            
            if (_selectedTile != null)
            {
                Debug.Log($"[TileSelectionVisualizer] TileBase found, calling SetHighlighted(true)");
                _selectedTile.SetHighlighted(true);
            }
            else
            {
                Debug.LogError($"[TileSelectionVisualizer] NO TileBase component on {tileObject.name}!");
            }
        }
        else
        {
            Debug.LogWarning("[TileSelectionVisualizer] tileObject is NULL");
        }
    }

    private void ClearSelection()
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetHighlighted(false);
            _selectedTile = null;
        }
    }

    public TileBase GetSelectedTile()
    {
        return _selectedTile;
    }
}
