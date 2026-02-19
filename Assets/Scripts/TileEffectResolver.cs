using UnityEngine;

public enum TileEffectResult
{
    Continue,
    Win,
    Fail
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
                Debug.LogWarning($"[TileEffectResolver] TileType 'Teleport' not yet implemented - treating as Normal");
                return TileEffectResult.Continue;

            case TileType.Rotate180:
                Debug.LogWarning($"[TileEffectResolver] TileType 'Rotate180' not yet implemented - treating as Normal");
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
}
