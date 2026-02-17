using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Data", menuName = "Tile System/Tile Data")]
public class TileData : ScriptableObject
{
    public TileType tileType;
    public bool isLocked;
    public bool isWalkable = true;
    public GameObject prefab;
}
