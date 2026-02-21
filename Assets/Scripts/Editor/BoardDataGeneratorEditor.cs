using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(BoardDataGenerator))]
public class BoardDataGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoardDataGenerator generator =
            (BoardDataGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Board Data"))
        {
            Generate(generator);
        }

        if (GUILayout.Button("Load Board Into Scene"))
        {
            LoadBoard(generator);
        }

        if (GUILayout.Button("Clear Board From Scene"))
        {
            ClearBoard(generator);
        }
    }

    private void Generate(BoardDataGenerator generator)
    {
        if (generator.targetBoardData == null)
        {
            Debug.LogError("No BoardData assigned.");
            return;
        }

        Transform root =
            generator.tileRoot != null
            ? generator.tileRoot
            : generator.transform;

        TileBase[] tiles =
            root.GetComponentsInChildren<TileBase>();

        if (tiles.Length == 0)
        {
            Debug.LogWarning("No tiles found.");
            return;
        }

        Dictionary<Vector2Int, TileInstanceData> grid =
            new();

        Vector2 offset = generator.worldOffset;

        foreach (var tile in tiles)
        {
            Vector3 pos = tile.transform.position;

            // --- FIX TILEMAP OFFSET ---
            float x = (pos.x - offset.x) / generator.cellSize;
            float z = (pos.z - offset.y) / generator.cellSize;

            Vector2Int gridPos =
                new Vector2Int(
                    Mathf.RoundToInt(x),
                    Mathf.RoundToInt(z)
                );

            int rotation =
                Mathf.RoundToInt(
                    tile.transform.eulerAngles.y / 90f
                ) % 4;

            TileInstanceData instance =
                new TileInstanceData()
                {
                    tileData = tile.tileData,
                    gridPosition = gridPos,
                    rotation = rotation
                };

            grid[gridPos] = instance;
        }

        // --- AUTO GRID SIZE ---
        int minX = grid.Keys.Min(p => p.x);
        int maxX = grid.Keys.Max(p => p.x);
        int minY = grid.Keys.Min(p => p.y);
        int maxY = grid.Keys.Max(p => p.y);

        Vector2Int size =
            new Vector2Int(
                maxX - minX + 1,
                maxY - minY + 1
            );

        generator.targetBoardData.gridSize = size;
        generator.targetBoardData.worldOrigin = new Vector2Int(minX, minY);

        // normalize positions so array starts at 0,0
        List<TileInstanceData> finalTiles = new();

        foreach (var kvp in grid)
        {
            TileInstanceData data = kvp.Value;

            data.gridPosition -=
                new Vector2Int(minX, minY);

            finalTiles.Add(data);
        }

        generator.targetBoardData.tiles =
            finalTiles.ToArray();

        EditorUtility.SetDirty(generator.targetBoardData);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Board generated. Tiles: {finalTiles.Count}"
        );
    }

    private void LoadBoard(BoardDataGenerator generator)
    {
        if (generator.targetBoardData == null)
        {
            Debug.LogError("No BoardData assigned.");
            return;
        }

        Transform root =
            generator.tileRoot != null
            ? generator.tileRoot
            : generator.transform;


        List<GameObject> toDelete = new();

        foreach (Transform child in root)
            toDelete.Add(child.gameObject);

        foreach (GameObject obj in toDelete)
            Object.DestroyImmediate(obj);


        BoardData boardData = generator.targetBoardData;

        if (boardData.tiles == null ||
            boardData.tiles.Length == 0)
        {
            Debug.LogWarning("BoardData has no tiles.");
            return;
        }

        float cellSize = generator.cellSize;
        Vector2 offset = generator.worldOffset;

        foreach (TileInstanceData tile in boardData.tiles)
        {
            if (tile.tileData == null)
                continue;

            GameObject prefab = tile.tileData.prefab;

            GameObject tileGO;

            if (prefab != null)
            {
                tileGO =
                    (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            }
            else
            {
                tileGO = new GameObject(tile.tileData.name);
            }

            tileGO.transform.SetParent(root);



            Vector3 worldPos = new Vector3(
                (tile.gridPosition.x + boardData.worldOrigin.x) * cellSize + offset.x,0f,
                 (tile.gridPosition.y + boardData.worldOrigin.y) * cellSize + offset.y);

            tileGO.transform.position = worldPos;
            tileGO.transform.rotation =
                Quaternion.Euler(0, tile.rotation, 0);

            // Ensure TileBase exists
            TileBase tileBase =
                tileGO.GetComponent<TileBase>();

            if (tileBase == null)
                tileBase = tileGO.AddComponent<TileBase>();

            tileBase.tileData = tile.tileData;
            tileBase.gridPosition = tile.gridPosition;
        }

        Debug.Log(
            $"Loaded {boardData.tiles.Length} tiles into scene.");
    }

    private void ClearBoard(BoardDataGenerator generator)
    {
        Transform root =
            generator.tileRoot != null
            ? generator.tileRoot
            : generator.transform;

        List<GameObject> toDelete = new();

        foreach (Transform child in root)
            toDelete.Add(child.gameObject);

        foreach (GameObject obj in toDelete)
            Object.DestroyImmediate(obj);
    }
}