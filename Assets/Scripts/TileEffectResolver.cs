using UnityEngine;

public enum TileEffectResult
{
    Continue,
    Win,
    Fail,
    Teleport
}

public static class TileEffectResolver
{
    public static TileEffectResult Resolve(TileType tileType, ref Vector2Int currentDirection, Vector2Int currentPosition)
    {
        switch (tileType)
        {
            case TileType.Normal:
                Debug.Log($"[TileEffectResolver] Normal tile at {currentPosition} - Continue");
                return TileEffectResult.Continue;

            case TileType.GoalTile:
                Debug.Log($"[TileEffectResolver] Goal tile at {currentPosition} - Win!");
                return TileEffectResult.Win;

            case TileType.Block:
                Debug.Log($"[TileEffectResolver] Block tile at {currentPosition} - Fail");
                return TileEffectResult.Fail;

            case TileType.Empty:
                Debug.Log($"[TileEffectResolver] Empty tile at {currentPosition} - Fail");
                return TileEffectResult.Fail;

            case TileType.Rotate90:
                Debug.Log($"[TileEffectResolver] Rotate90 tile at {currentPosition} - Rotating direction");
                currentDirection = RotateDirection90(currentDirection);
                return TileEffectResult.Continue;

            case TileType.StartingTile:
                Debug.Log($"[TileEffectResolver] Starting tile at {currentPosition} - Continue");
                return TileEffectResult.Continue;

            case TileType.Locked:
                Debug.LogWarning($"[TileEffectResolver] TileType 'Locked' not yet implemented - treating as Normal");
                return TileEffectResult.Continue;

            case TileType.Portal:
                Debug.LogWarning($"[TileEffectResolver] TileType 'Portal' not yet implemented - treating as Normal");
                return TileEffectResult.Continue;

            case TileType.Teleport:
                Debug.Log($"[TileEffectResolver] Teleport tile at {currentPosition} - Teleporting!");
                return TileEffectResult.Teleport;

            case TileType.Rotate90Left:
                Debug.Log($"[TileEffectResolver] Rotate90Left tile at {currentPosition} - Rotating left (anticlockwise)");
                currentDirection = RotateDirectionLeft(currentDirection);
                return TileEffectResult.Continue;

            case TileType.Rotate90Right:
                Debug.Log($"[TileEffectResolver] Rotate90Right tile at {currentPosition} - Rotating right (clockwise)");
                currentDirection = RotateDirectionRight(currentDirection);
                return TileEffectResult.Continue;

            case TileType.Rotate180:
                Debug.Log($"[TileEffectResolver] Rotate180 tile at {currentPosition} - Rotating 180°");
                currentDirection = RotateDirection180(currentDirection);
                return TileEffectResult.Continue;

            case TileType.JumpForward:
                Debug.LogWarning($"[TileEffectResolver] TileType 'JumpForward' not yet implemented - treating as Normal");
                return TileEffectResult.Continue;

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
                Debug.LogWarning($"[TileEffectResolver] TileType '{tileType}' not recognized - treating as Normal");
                return TileEffectResult.Continue;
        }
    }

    private static Vector2Int RotateDirection90(Vector2Int direction)
    {
        return new Vector2Int(-direction.y, direction.x);
    }

    private static Vector2Int RotateDirectionLeft(Vector2Int direction)
    {
        // Anticlockwise: (x, y) → (-y, x)
        // (1,0) → (0,1), (0,1) → (-1,0), (-1,0) → (0,-1), (0,-1) → (1,0)
        return new Vector2Int(-direction.y, direction.x);
    }

    private static Vector2Int RotateDirectionRight(Vector2Int direction)
    {
        // Clockwise: (x, y) → (y, -x)
        // (1,0) → (0,-1), (0,-1) → (-1,0), (-1,0) → (0,1), (0,1) → (1,0)
        return new Vector2Int(direction.y, -direction.x);
    }

    private static Vector2Int RotateDirection180(Vector2Int direction)
    {
        // 180°: (x, y) → (-x, -y)
        // Invierte completamente la dirección
        return new Vector2Int(-direction.x, -direction.y);
    }

    public static bool FindTeleportPair(TileGrid tileGrid, int teleportID, Vector2Int currentPosition, out Vector2Int pairPosition, out Vector2Int exitDirection)
    {
        pairPosition = Vector2Int.zero;
        exitDirection = Vector2Int.right;

        if (teleportID == 0)
        {
            Debug.LogWarning("[TileEffectResolver] Teleport ID is 0, no pairing possible");
            return false;
        }

        // Buscar todos los tiles en el grid
        var allTiles = tileGrid.GetAllTiles();
        
        foreach (var kvp in allTiles)
        {
            Vector2Int pos = kvp.Key;
            GameObject tileObj = kvp.Value;

            // Ignorar el tile actual
            if (pos == currentPosition)
                continue;

            // Verificar si es un teleport con el mismo ID
            TileComponent tileComp = tileObj.GetComponent<TileComponent>();
            if (tileComp != null && tileComp.tileData != null)
            {
                if (tileComp.tileData.tileType == TileType.Teleport && tileComp.tileData.teleportID == teleportID)
                {
                    pairPosition = pos;
                    exitDirection = tileComp.tileData.exitDirection;
                    Debug.Log($"[TileEffectResolver] Found teleport pair at {pairPosition} with exit direction {exitDirection}");
                    return true;
                }
            }
        }

        Debug.LogWarning($"[TileEffectResolver] No teleport pair found for ID {teleportID}");
        return false;
    }
}
