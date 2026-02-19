using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Data", menuName = "Tile System/Tile Data")]
public class TileData : ScriptableObject
{
    [Header("Tile Properties")]
    public TileType tileType;
    public bool isLocked;
    public bool isWalkable = true;
    public GameObject prefab;

    [Header("Visual")]
    public Color highlightColor = new Color(1f, 0.8f, 0.3f, 1f);
    
    [Header("Teleport Settings")]
    [Tooltip("ID to pair teleport tiles. Tiles with same ID are connected (0 = not a teleport)")]
    public int teleportID = 0;
    [Tooltip("Direction the character exits when teleporting to this tile (relative to tile rotation)")]
    public Vector2Int exitDirection = Vector2Int.right;
    
    public bool IsSelectable()
    {
        return tileType != TileType.StartingTile && 
               tileType != TileType.GoalTile && 
               tileType != TileType.Locked;
    }
    
    public bool IsMovable()
    {
        return !isLocked && 
               tileType != TileType.StartingTile && 
               tileType != TileType.GoalTile;
    }
}
