using UnityEngine;

public enum TileEffectResult
{
    Continue,
    Win,
    Fail
}

public static class TileEffectResolver
{
    public static TileEffectResult Resolve(ref TileEffectContext context)
    {
        switch (context.tileData.tileType)
        {
            case TileType.Normal:
                Debug.Log($"[TileEffectResolver] Normal tile at {context.position} - Continue");
                return TileEffectResult.Continue;

            case TileType.GoalTile:
                Debug.Log($"[TileEffectResolver] Goal tile at {context.position} - Win!");
                return TileEffectResult.Win;

            case TileType.Block:
                Debug.Log($"[TileEffectResolver] Block tile at {context.position} - Fail");
                return TileEffectResult.Fail;

            case TileType.Empty:
                Debug.Log($"[TileEffectResolver] Empty tile at {context.position} - Fail");
                return TileEffectResult.Fail;

            case TileType.StartingTile:
                Debug.Log($"[TileEffectResolver] Starting tile at {context.position} - Continue");
                return TileEffectResult.Continue;

            case TileType.Locked:
                Debug.LogWarning($"[TileEffectResolver] TileType 'Locked' not yet implemented - treating as Normal");
                return TileEffectResult.Continue;

            case TileType.Portal:
                {
                    if (context.justUsedPortal)
                    {
                        Debug.Log($"[TileEffectResolver] Portal at {context.position} skipped (just arrived from another portal)");
                        context.justUsedPortal = false;
                        return TileEffectResult.Continue;
                    }

                    Debug.Log($"[TileEffectResolver] Portal triggered at board {context.currentBoardIndex}");

                    if (FindPortalPair(
                        context.tileData.teleportID,
                        context.position,
                        context.currentBoardIndex,
                        out int targetBoard,
                        out Vector2Int pairPos,
                        out Vector2Int exitDir))
                    {
                        context.targetBoardIndex = targetBoard;
                        context.position = pairPos;
                        context.direction = exitDir;
                        context.visualEffect = TileEffectVisual.Teleport;
                        context.switchedBoard = true;
                        context.justUsedPortal = true;

                        Debug.Log($"[TileEffectResolver] Portal pair found on board {targetBoard} at {pairPos}");
                        return TileEffectResult.Continue;
                    }

                    Debug.LogWarning("[TileEffectResolver] No portal pair found!");
                    return TileEffectResult.Fail;
                }

            case TileType.Teleport:
                {
                    if (context.justUsedPortal)
                    {
                        Debug.Log($"[TileEffectResolver] Teleport at {context.position} skipped (just arrived from another teleport)");
                        context.justUsedPortal = false;
                        return TileEffectResult.Continue;
                    }

                    Debug.Log($"[TileEffectResolver] Teleport triggered");

                    if (FindTeleportPair(
                        context.tileGrid,
                        context.tileData.teleportID,
                        context.position,
                        out Vector2Int pairPosition,
                        out Vector2Int exitDirection))
                    {
                        context.position = pairPosition;
                        context.direction = exitDirection;

                        context.visualEffect = TileEffectVisual.Teleport;
                        context.justUsedPortal = true;

                        return TileEffectResult.Continue;
                    }

                    return TileEffectResult.Fail;
                }
            case TileType.Rotate90Left:
                Debug.Log($"[TileEffectResolver] Rotate90Left tile at {context.position} - Rotating left (anticlockwise)");
                context.direction = RotateDirectionLeft(context.direction);
                return TileEffectResult.Continue;

            case TileType.Rotate90Right:
                Debug.Log($"[TileEffectResolver] Rotate90Right tile at {context.position} - Rotating right (clockwise)");
                context.direction = RotateDirectionRight(context.direction);
                return TileEffectResult.Continue;

            case TileType.Rotate180:
                Debug.Log($"[TileEffectResolver] Rotate180 tile at {context.position} - Rotating 180°");
                context.direction = RotateDirection180(context.direction);
                return TileEffectResult.Continue;

            case TileType.JumpForward:
                return HandleJumpForward(ref context);
                    

            case TileType.JumpVertical:
                Debug.LogWarning($"[TileEffectResolver] TileType 'JumpVertical' not yet implemented - treating as Normal");
                return TileEffectResult.Continue;

            case TileType.SpeedUp:
                Debug.LogWarning($"[TileEffectResolver] TileType 'SpeedUp' not yet implemented - treating as Normal");
                return TileEffectResult.Continue;

            case TileType.Trigger:
                Debug.LogWarning($"[TileEffectResolver] TileType 'Trigger' not yet implemented - treating as Normal");
                return TileEffectResult.Continue;

            case TileType.Door:
                Debug.LogWarning($"[TileEffectResolver] TileType 'Door' not yet implemented - treating as Normal");
                return TileEffectResult.Continue;

            default:
                Debug.LogWarning($"[TileEffectResolver] TileType '{context.tileData.tileType}' not recognized - treating as Normal");
                return TileEffectResult.Continue;
        }
    }
    private static Vector2Int RotateDirectionLeft(Vector2Int direction)
    {
        return new Vector2Int(-direction.y, direction.x);
    }

    private static Vector2Int RotateDirectionRight(Vector2Int direction)
    {
        return new Vector2Int(direction.y, -direction.x);
    }

    private static Vector2Int RotateDirection180(Vector2Int direction)
    {
        return new Vector2Int(-direction.x, -direction.y);
    }

    

    public static bool FindTeleportPair(
    TileGrid tileGrid,
    int teleportID,
    Vector2Int currentPosition,
    out Vector2Int pairPosition,
    out Vector2Int exitDirection)

    {
        pairPosition = Vector2Int.zero;
        exitDirection = Vector2Int.right;

        if (teleportID == 0)
        {
            Debug.LogWarning("[TileEffectResolver] Teleport ID is 0, no pairing possible");
            return false;
        }

        var allTiles = tileGrid.GetAllTiles();

        foreach (var kvp in allTiles)
        {
            Vector2Int pos = kvp.Key;
            GameObject tileObj = kvp.Value;

            if (pos == currentPosition)
                continue;

            TileBase tileBase = tileObj.GetComponent<TileBase>();
            if (tileBase != null && tileBase.tileData != null)
            {
                if (tileBase.tileData.tileType == TileType.Teleport && tileBase.tileData.teleportID == teleportID)
                {
                    pairPosition = pos;
                    exitDirection = tileBase.tileData.exitDirection;
                    Debug.Log($"[TileEffectResolver] Found teleport pair at {pairPosition} with exit direction {exitDirection}");
                    return true;
                }
            }
        }

        Debug.LogWarning($"[TileEffectResolver] No teleport pair found for ID {teleportID}");
        return false;
    }

    public static bool FindPortalPair(
    int teleportID,
    Vector2Int currentPosition,
    int currentBoardIndex,
    out int targetBoardIndex,
    out Vector2Int pairPosition,
    out Vector2Int exitDirection)
    {
        targetBoardIndex = -1;
        pairPosition = Vector2Int.zero;
        exitDirection = Vector2Int.right;

        if (teleportID == 0)
        {
            Debug.LogWarning("[TileEffectResolver] Portal ID is 0, no pairing possible");
            return false;
        }

        if (BoardManager.Instance == null)
        {
            Debug.LogError("[TileEffectResolver] BoardManager not found!");
            return false;
        }

        var allBoards = BoardManager.Instance.GetAllBoards();

        for (int i = 0; i < allBoards.Count; i++)
        {
            if (i == currentBoardIndex)
                continue;

            Board board = allBoards[i];
            if (board == null || board.TileGrid == null)
                continue;

            TileGrid tileGrid = board.TileGrid;
            var allTiles = tileGrid.GetAllTiles();

            foreach (var kvp in allTiles)
            {
                Vector2Int pos = kvp.Key;
                GameObject tileObj = kvp.Value;

                TileBase tileBase = tileObj.GetComponent<TileBase>();
                if (tileBase?.tileData != null)
                {
                    if (tileBase.tileData.tileType == TileType.Portal && 
                        tileBase.tileData.teleportID == teleportID)
                    {
                        targetBoardIndex = i;
                        pairPosition = pos;
                        exitDirection = tileBase.tileData.exitDirection;
                        Debug.Log($"[TileEffectResolver] Found portal pair on board {i} at {pairPosition} with exit direction {exitDirection}");
                        return true;
                    }
                }
            }
        }

        Debug.LogWarning($"[TileEffectResolver] No portal pair found for ID {teleportID}");
        return false;
    }

    private static TileEffectResult HandleJumpForward(
    ref TileEffectContext context)
    {
        Debug.Log("[TileEffectResolver] JumpForward triggered");

        Vector2Int landing =
            context.position +
            context.direction *
            (context.tileData.jumpDistance + 1);

        GameObject landingTile =
            context.tileGrid.GetTile(landing);

        if (landingTile == null)
            return TileEffectResult.Fail;

        TileBase tileBase =
            landingTile.GetComponent<TileBase>();

        if (!tileBase.tileData.isWalkable)
            return TileEffectResult.Fail;
        context.position = landing;
        context.visualEffect = TileEffectVisual.Jump;

        return TileEffectResult.Continue;
    }
}