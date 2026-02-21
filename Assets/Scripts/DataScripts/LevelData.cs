using UnityEngine;

[System.Serializable]

[CreateAssetMenu(fileName = "New Level Data", menuName = "Tile System/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelId;
    public BoardData[] boards;
    public Vector3 characterStartDirection = Vector3.forward;
}
