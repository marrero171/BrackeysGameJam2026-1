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
    public int currentBoardIndex;

    public Vector2Int position;
    public Vector2Int direction;

    public TileData tileData;

    public TileEffectVisual visualEffect;
    public bool switchedBoard;
    public int targetBoardIndex;
    public bool justUsedPortal;
}
