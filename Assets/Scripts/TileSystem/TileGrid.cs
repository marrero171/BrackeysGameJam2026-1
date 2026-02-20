using UnityEngine;
using System.Collections.Generic;

public class TileGrid : MonoBehaviour
{
    [SerializeField] private BoardData boardData;
    [SerializeField] private float tileSpacing = 1f;
    
    public delegate void TilesInstantiatedHandler();
    public event TilesInstantiatedHandler OnTilesInstantiated;
    
    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        // Note: InstantiateTiles() is now called by Board.Initialize()
        // This prevents double instantiation when using BoardManager
    }
    
    public void SetBoardData(BoardData newBoardData)
    {
        boardData = newBoardData;
    }

    public void InstantiateTiles()
    {
        ClearTiles();

        if (boardData == null || boardData.tiles == null)
        {
            Debug.LogWarning("[TileGrid] No board data or tiles to instantiate");
            return;
        }

        foreach (TileInstanceData tileInstance in boardData.tiles)
        {
            if (tileInstance.tileData == null || tileInstance.tileData.prefab == null)
            {
                continue;
            }

            Vector3 worldPosition = GridToWorldPosition(tileInstance.gridPosition);
            Quaternion rotation = Quaternion.Euler(0, tileInstance.rotation, 0);
            
            GameObject tileObject = Instantiate(tileInstance.tileData.prefab, worldPosition, rotation, transform);
            tileObject.name = $"Tile_{tileInstance.gridPosition.x}_{tileInstance.gridPosition.y}";
            
            TileBase tileBase = tileObject.GetComponent<TileBase>();
            if (tileBase == null)
            {
                tileBase = tileObject.AddComponent<TileBase>();
            }
            
            tileBase.Initialize(tileInstance.tileData, tileInstance.gridPosition);
            
            ApplyMaterialToTile(tileObject, tileInstance.tileData);
            
            if (tileInstance.tileData.tileType == TileType.Empty)
            {
                MeshRenderer renderer = tileObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
            
            tiles[tileInstance.gridPosition] = tileObject;
        }

        OnTilesInstantiated?.Invoke();
    }

    public GameObject GetTile(Vector2Int gridPosition)
    {
        if (tiles.TryGetValue(gridPosition, out GameObject tile))
        {
            return tile;
        }
        return null;
    }

    public TileData GetTileData(Vector2Int gridPosition)
    {
        GameObject tile = GetTile(gridPosition);
        if (tile != null)
        {
            TileBase tileBase = tile.GetComponent<TileBase>();
            if (tileBase != null)
            {
                return tileBase.tileData;
            }
        }
        return null;
    }

    public Dictionary<Vector2Int, GameObject> GetAllTiles()
    {
        return tiles;
    }

    public void SwapTiles(Vector2Int positionA, Vector2Int positionB)
    {
        GameObject tileA = GetTile(positionA);
        GameObject tileB = GetTile(positionB);

        if (tileA == null || tileB == null)
        {
            return;
        }

        Vector3 worldPosA = GridToWorldPosition(positionA);
        Vector3 worldPosB = GridToWorldPosition(positionB);

        tileA.transform.position = worldPosB;
        tileB.transform.position = worldPosA;

        tiles[positionA] = tileB;
        tileB.GetComponent<TileBase>().gridPosition = positionA;
        tiles[positionB] = tileA;
        tileA.GetComponent<TileBase>().gridPosition = positionB;
    }

    public void TrySwap(Vector2Int positionA)
    {
        GameObject tileA = GetTile(positionA);
        TileBase tileBaseA = tileA.GetComponent<TileBase>();

        if (tileBaseA.tileData.isLocked) { return; }

        Vector2Int[] positionsToCheck = new Vector2Int[] { 
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left };

        foreach (Vector2Int posOffset in positionsToCheck) {
            Vector2Int positionB = positionA + posOffset;
            GameObject tileB = GetTile(positionB);
            if (tileB == null) { continue; }

            TileBase tileBaseB = tileB.GetComponent<TileBase>();
            if (tileBaseB.tileData.tileType == TileType.Empty)
            {
                SwapTiles(positionA, positionB);
                break;
            }
        }
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * tileSpacing, 0, gridPosition.y * tileSpacing);
    }

    private void ApplyMaterialToTile(GameObject tileObject, TileData tileData)
    {
        if (tileData.material != null)
        {
            MeshRenderer[] renderers = tileObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.sharedMaterial = tileData.material;
            }
        }
    }

    public void ClearTiles()
    {
        foreach (GameObject tile in tiles.Values)
        {
            if (tile != null)
            {
                Destroy(tile);
            }
        }
        tiles.Clear();
    }

    private void OnDestroy()
    {
        ClearTiles();
    }
}
