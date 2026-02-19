using UnityEngine;

public class TileMoveListener : MonoBehaviour
{
    [SerializeField] private TileSlideController slideController;

    private void OnEnable()
    {
        if (slideController != null)
        {
            slideController.OnTileMoved += HandleTileMoved;
        }
    }

    private void OnDisable()
    {
        if (slideController != null)
        {
            slideController.OnTileMoved -= HandleTileMoved;
        }
    }

    private void HandleTileMoved(Vector2Int fromPosition, Vector2Int toPosition)
    {
        Debug.Log($"Tile moved from {fromPosition} to {toPosition}");
    }
}
