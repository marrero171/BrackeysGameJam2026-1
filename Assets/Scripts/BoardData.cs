using UnityEngine;

[CreateAssetMenu(fileName = "New Board Data", menuName = "Tile System/Board Data")]
public class BoardData : ScriptableObject
{
    public string boardId;
    public int boardIndex;
    public Vector2Int gridSize = new Vector2Int(10, 10);
    public TileInstanceData[] tiles;
    public Vector2Int characterStartPosition;
    public Vector3 characterStartDirection = Vector3.forward;
    public Vector2Int goalPosition;
}
