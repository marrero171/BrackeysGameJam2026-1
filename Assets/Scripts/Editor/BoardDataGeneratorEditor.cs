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

        BoardDataGenerator generator = (BoardDataGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Load Current Board Into Scene"))
            LoadBoard(generator, useSolvedTiles: false);

        if (GUILayout.Button("Load Solved Board Into Scene"))
            LoadBoard(generator, useSolvedTiles: true);

        GUILayout.Space(10);

        if (GUILayout.Button("Save Current Tiles as Scrambled"))
            SaveCurrentAsScrambled(generator);

        if (GUILayout.Button("Save Current Tiles as Solved"))
            SaveCurrentAsSolved(generator);

        GUILayout.Space(10);

        if (GUILayout.Button("Scramble Non-Locked Tiles"))
            Scramble(generator);

        GUILayout.Space(10);

        if (GUILayout.Button("Clear Board From Scene"))
            ClearBoard(generator);
    }

    // Saves the current scene layout as the solved state
    private void SaveCurrentAsSolved(BoardDataGenerator generator)
    {
        TileInstanceData[] tilesInScene = CollectTilesFromScene(generator);
        if (tilesInScene == null || tilesInScene.Length == 0) return;

        generator.targetBoardData.solvedTiles = CloneTiles(tilesInScene);
        EditorUtility.SetDirty(generator.targetBoardData);
        AssetDatabase.SaveAssets();

        Debug.Log("Current tiles saved as SOLVED.");
    }

    // Saves the current scene layout as the play/scrambled state
    private void SaveCurrentAsScrambled(BoardDataGenerator generator)
    {
        TileInstanceData[] tilesInScene = CollectTilesFromScene(generator);
        if (tilesInScene == null || tilesInScene.Length == 0) return;

        generator.targetBoardData.tiles = CloneTiles(tilesInScene);
        EditorUtility.SetDirty(generator.targetBoardData);
        AssetDatabase.SaveAssets();

        Debug.Log("Current tiles saved as SCRAMBLED.");
    }

    // Collect all TileBase objects in scene and convert to TileInstanceData
    private TileInstanceData[] CollectTilesFromScene(BoardDataGenerator generator)
    {
        Transform root = generator.tileRoot != null ? generator.tileRoot : generator.transform;
        TileBase[] tiles = root.GetComponentsInChildren<TileBase>();
        if (tiles.Length == 0) return null;

        Vector2 offset = generator.worldOffset;
        Dictionary<Vector2Int, TileInstanceData> grid = new();

        foreach (var tile in tiles)
        {
            Vector3 pos = tile.transform.position;
            float x = (pos.x - offset.x) / generator.cellSize;
            float z = (pos.z - offset.y) / generator.cellSize;

            Vector2Int gridPos = new(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
            int rotation = Mathf.RoundToInt(tile.transform.eulerAngles.y / 90f) % 4;

            TileInstanceData instance = new TileInstanceData()
            {
                tileData = tile.tileData,
                gridPosition = gridPos,
                rotation = rotation
            };

            grid[gridPos] = instance;
        }

        int minX = grid.Keys.Min(p => p.x);
        int minY = grid.Keys.Min(p => p.y);

        List<TileInstanceData> finalTiles = new();
        foreach (var kvp in grid)
        {
            TileInstanceData data = kvp.Value;
            data.gridPosition -= new Vector2Int(minX, minY);
            finalTiles.Add(data);
        }

        return finalTiles.ToArray();
    }

    private void LoadBoard(BoardDataGenerator generator, bool useSolvedTiles)
    {
        ClearBoard(generator);
        if (generator.targetBoardData == null)
        {
            Debug.LogError("No BoardData assigned.");
            return;
        }

        Transform root = generator.tileRoot != null ? generator.tileRoot : generator.transform;

        // Clear existing tiles
        foreach (Transform child in root)
            Object.DestroyImmediate(child.gameObject);

        BoardData boardData = generator.targetBoardData;

        // Choose tiles
        TileInstanceData[] tilesToLoad = useSolvedTiles && boardData.solvedTiles != null
            ? boardData.solvedTiles
            : boardData.tiles;

        if (tilesToLoad == null || tilesToLoad.Length == 0)
        {
            Debug.LogWarning("BoardData has no tiles to load.");
            return;
        }

        float cellSize = generator.cellSize;
        Vector2 offset = generator.worldOffset;

        foreach (TileInstanceData tile in tilesToLoad)
        {
            if (tile.tileData == null)
                continue;

            GameObject prefab = tile.tileData.prefab;
            GameObject tileGO = prefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                : new GameObject(tile.tileData.name);

            tileGO.transform.SetParent(root);

            Vector3 worldPos = new Vector3(
                (tile.gridPosition.x + boardData.worldOrigin.x) * cellSize + offset.x,
                0f,
                (tile.gridPosition.y + boardData.worldOrigin.y) * cellSize + offset.y
            );

            tileGO.transform.position = worldPos;
            tileGO.transform.rotation = Quaternion.Euler(0, tile.rotation, 0);

            TileBase tileBase = tileGO.GetComponent<TileBase>() ?? tileGO.AddComponent<TileBase>();
            tileBase.tileData = tile.tileData;
            tileBase.gridPosition = tile.gridPosition;
        }

        Debug.Log($"Loaded {tilesToLoad.Length} tiles into scene.");
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

    private void Scramble(BoardDataGenerator generator)
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
            Debug.LogWarning("No tiles in scene.");
            return;
        }

        // ---------- COLLECT MOVABLE ----------
        List<TileBase> movable =
            tiles
            .Where(t => !t.tileData.isLocked)
            .ToList();

        if (movable.Count <= 1)
            return;

        List<Vector3> positions =
            movable.Select(t => t.transform.position).ToList();

        // Fisher–Yates shuffle
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (positions[i], positions[j]) =
                (positions[j], positions[i]);
        }

        Undo.RecordObjects(
            movable.Select(t => t.transform).ToArray(),
            "Scramble Tiles"
        );

        for (int i = 0; i < movable.Count; i++)
        {
            movable[i].transform.position = positions[i];
        }

        Debug.Log($"Scrambled {movable.Count} tiles.");
    }

    // Utility function
    private TileInstanceData[] CloneTiles(TileInstanceData[] source)
    {
        if (source == null) return null;
        return source.Select(t => new TileInstanceData()
        {
            tileData = t.tileData,
            gridPosition = t.gridPosition,
            rotation = t.rotation
        }).ToArray();
    }
}