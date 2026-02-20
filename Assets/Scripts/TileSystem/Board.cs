using UnityEngine;

public class Board : MonoBehaviour
{
    public int BoardIndex { get; private set; }
    public BoardData BoardData { get; private set; }
    public TileGrid TileGrid { get; private set; }

    private bool _isVisible = false;

    public void Initialize(BoardData boardData, int index)
    {
        BoardIndex = index;
        BoardData = boardData;

        TileGrid = GetComponent<TileGrid>();
        if (TileGrid == null)
        {
            TileGrid = gameObject.AddComponent<TileGrid>();
        }

        TileGrid.SetBoardData(boardData);
        TileGrid.InstantiateTiles();

        gameObject.name = $"Board_{index}_{boardData.name}";

        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _isVisible = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _isVisible = false;
    }

    public bool IsVisible => _isVisible;

    public void Clear()
    {
        if (TileGrid != null)
        {
            TileGrid.ClearTiles();
        }
    }
}
