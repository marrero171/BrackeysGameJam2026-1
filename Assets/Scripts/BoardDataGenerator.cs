using UnityEngine;

public class BoardDataGenerator : MonoBehaviour
{
    [Header("Output")]
    public BoardData targetBoardData;

    [Header("Grid Settings")]
    public float cellSize = 1f;

    [Tooltip("Tilemap usually offsets tiles by 0.5")]
    public Vector2 worldOffset = new Vector2(0.5f, 0.5f);

    [Header("Search")]
    public Transform tileRoot;
}
