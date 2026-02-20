using UnityEngine;

public enum TileEffectVisual
{
    Default,
    Teleport,
    Jump,
    Dash,
    None
}

public struct TileEffectContext
{
    public TileGrid tileGrid;

    public Vector2Int position;
    public Vector2Int direction;

    public TileData tileData;

    public TileEffectVisual visualEffect;
}
