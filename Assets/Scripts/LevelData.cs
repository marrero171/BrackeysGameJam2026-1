using UnityEngine;

[CreateAssetMenu(fileName = "New Level Data", menuName = "Tile System/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelId;
    public BoardData[] boards;
}
