using UnityEngine;
using System.Collections.Generic;

public class TileFactory : MonoBehaviour
{
    [System.Serializable]
    public class TileTypeMapping
    {
        public TileType tileType;
        public GameObject prefab;
        public Material material;
    }

    [SerializeField] private TileTypeMapping[] tileMappings;
    
    private Dictionary<TileType, TileTypeMapping> mappingLookup;

    private void Awake()
    {
        InitializeMappings();
    }

    private void InitializeMappings()
    {
        mappingLookup = new Dictionary<TileType, TileTypeMapping>();
        
        if (tileMappings != null)
        {
            foreach (TileTypeMapping mapping in tileMappings)
            {
                if (!mappingLookup.ContainsKey(mapping.tileType))
                {
                    mappingLookup[mapping.tileType] = mapping;
                }
            }
        }
    }

    public GameObject SpawnTile(TileType tileType, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (mappingLookup == null)
        {
            InitializeMappings();
        }

        if (!mappingLookup.TryGetValue(tileType, out TileTypeMapping mapping))
        {
            Debug.LogWarning($"No mapping found for TileType: {tileType}");
            return null;
        }

        if (mapping.prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for TileType: {tileType}");
            return null;
        }

        GameObject tileObject = Instantiate(mapping.prefab, position, rotation, parent);
        
        if (mapping.material != null)
        {
            ApplyMaterial(tileObject, mapping.material);
        }

        return tileObject;
    }

    public GameObject GetPrefabForType(TileType tileType)
    {
        if (mappingLookup == null)
        {
            InitializeMappings();
        }

        if (mappingLookup.TryGetValue(tileType, out TileTypeMapping mapping))
        {
            return mapping.prefab;
        }

        return null;
    }

    public Material GetMaterialForType(TileType tileType)
    {
        if (mappingLookup == null)
        {
            InitializeMappings();
        }

        if (mappingLookup.TryGetValue(tileType, out TileTypeMapping mapping))
        {
            return mapping.material;
        }

        return null;
    }

    private void ApplyMaterial(GameObject tileObject, Material material)
    {
        MeshRenderer[] renderers = tileObject.GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.sharedMaterial = material;
        }
    }
}
