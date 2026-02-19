using UnityEngine;

[System.Serializable]
public struct BoardReference
{
    public int boardIndex;
    public Vector2Int position;
}

[CreateAssetMenu(fileName = "New Level Data", menuName = "Tile System/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelId;
    public BoardData[] boards;
    
    [Header("Level Start and Goal")]
    public BoardReference startingTile;
    public Vector3 characterStartDirection = Vector3.forward;
    public BoardReference goalTile;
}
