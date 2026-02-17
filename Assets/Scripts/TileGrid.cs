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
    [SerializeField] private Material rotate180Material;
    [SerializeField] private Material jumpForwardMaterial;
    [SerializeField] private Material jumpVerticalMaterial;
    [SerializeField] private Material speedUpMaterial;
    [SerializeField] private Material blockMaterial;
    [SerializeField] private Material triggerMaterial;
    [SerializeField] private Material doorMaterial;
    
    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        if (boardData != null)
        {
            InstantiateTiles();
        }
    }

    public void InstantiateTiles()
    {
        ClearTiles();

        if (boardData == null || boardData.tiles == null)
        {
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
            
            ApplyMaterialToTile(tileObject, tileInstance.tileData.tileType);
            
            tiles[tileInstance.gridPosition] = tileObject;
        }
    }

    public GameObject GetTile(Vector2Int gridPosition)
    {
        if (tiles.TryGetValue(gridPosition, out GameObject tile))
        {
            return tile;
        }
        return null;
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
        tiles[positionB] = tileA;
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPosition)
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
            case TileType.Rotate180: return rotate180Material;
            case TileType.JumpForward: return jumpForwardMaterial;
            case TileType.JumpVertical: return jumpVerticalMaterial;
            case TileType.SpeedUp: return speedUpMaterial;
            case TileType.Block: return blockMaterial;
            case TileType.Trigger: return triggerMaterial;
            case TileType.Door: return doorMaterial;
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
