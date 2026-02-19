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

        TileComponent[] tiles =
            root.GetComponentsInChildren<TileComponent>();

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
}
