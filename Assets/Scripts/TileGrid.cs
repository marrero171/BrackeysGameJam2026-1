using UnityEngine;
using System.Collections.Generic;

public class TileGrid : MonoBehaviour
{
    [SerializeField] private BoardData boardData;
    [SerializeField] private float tileSpacing = 1f;
    
    [Header("Tile Materials")]
    [SerializeField] private Material emptyMaterial;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material lockedMaterial;
    [SerializeField] private Material portalMaterial;
    [SerializeField] private Material teleportMaterial;
    [SerializeField] private Material rotate90Material;
    [SerializeField] private Material rotate90LeftMaterial;
    [SerializeField] private Material rotate90RightMaterial;
    [SerializeField] private Material rotate180Material;
    [SerializeField] private Material jumpForwardMaterial;
    [SerializeField] private Material jumpVerticalMaterial;
    [SerializeField] private Material speedUpMaterial;
    [SerializeField] private Material blockMaterial;
    [SerializeField] private Material triggerMaterial;
    [SerializeField] private Material doorMaterial;
    [SerializeField] private Material startingTileMaterial;
    [SerializeField] private Material goalTileMaterial;
    
    public delegate void TilesInstantiatedHandler();
    public event TilesInstantiatedHandler OnTilesInstantiated;
    
    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        if (boardData != null)
        {
            InstantiateTiles();
        }
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
            if (tileBase != null)
            {
                tileBase.Initialize(tileInstance.tileData, tileInstance.gridPosition);
            }
            
            ApplyMaterialToTile(tileObject, tileInstance.tileData.tileType);
            
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
        
        Debug.Log("[TileGrid] Tiles instantiation complete, notifying listeners");
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
        tileB.GetComponent<TileComponent>().gridPosition = positionA;
        tiles[positionB] = tileA;
        tileA.GetComponent<TileComponent>().gridPosition = positionB;
    }

    public void TrySwap(Vector2Int positionA)
    {
        GameObject tileA = GetTile(positionA);
        TileComponent tileComponentA = tileA.GetComponent<TileComponent>();

        if (tileComponentA.tileData.isLocked) { return; }

        Vector2Int[] positionsToCheck = new Vector2Int[] { 
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left };

        foreach (Vector2Int posOffset in positionsToCheck) {
            Vector2Int positionB = positionA + posOffset;
            GameObject tileB = GetTile(positionB);
            if (tileB == null) { continue; }

            TileComponent tileComponentB = tileB.GetComponent<TileComponent>();
            if (tileComponentB.tileData.tileType == TileType.Empty)
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

    private void ApplyMaterialToTile(GameObject tileObject, TileType tileType)
    {
        Material material = GetMaterialForType(tileType);
        
        if (material != null)
        {
            MeshRenderer[] renderers = tileObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.sharedMaterial = material;
            }
        }
    }

    private Material GetMaterialForType(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Empty: return emptyMaterial;
            case TileType.Normal: return normalMaterial;
            case TileType.Locked: return lockedMaterial;
            case TileType.Portal: return portalMaterial;
            case TileType.Teleport: return teleportMaterial;
            case TileType.Rotate90: return rotate90Material;
            case TileType.Rotate90Left: return rotate90LeftMaterial;
            case TileType.Rotate90Right: return rotate90RightMaterial;
            case TileType.Rotate180: return rotate180Material;
            case TileType.JumpForward: return jumpForwardMaterial;
            case TileType.JumpVertical: return jumpVerticalMaterial;
            case TileType.SpeedUp: return speedUpMaterial;
            case TileType.Block: return blockMaterial;
            case TileType.Trigger: return triggerMaterial;
            case TileType.Door: return doorMaterial;
            case TileType.StartingTile: return startingTileMaterial;
            case TileType.GoalTile: return goalTileMaterial;
            default: return normalMaterial;
        }
    }

    private void ClearTiles()
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
