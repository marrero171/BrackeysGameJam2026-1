using UnityEngine;

public class TileBase : MonoBehaviour
{
    [Header("Tile Data")]
    public TileData tileData;
    public Vector2Int gridPosition;

    [Header("Visual")]
    private GameObject _highlightObject;
    private MeshRenderer _highlightRenderer;
    private Material _highlightMaterial;
    private bool _isHighlighted = false;

    protected virtual void Awake()
    {
        _highlightObject = transform.Find("Highlight")?.gameObject;
        
        if (_highlightObject != null)
        {
            _highlightRenderer = _highlightObject.GetComponent<MeshRenderer>();
            if (_highlightRenderer != null)
            {
                _highlightMaterial = _highlightRenderer.material;
            }
            _highlightObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"[TileBase] Awake: NO Highlight child found on {gameObject.name}");
        }
    }

    public void Initialize(TileData data, Vector2Int position)
    {
        tileData = data;
        gridPosition = position;

        if (tileData != null && _highlightMaterial != null)
        {
            Color highlightColor = tileData.highlightColor;
            highlightColor.a = 0.6f;
            _highlightMaterial.SetColor("_Color", highlightColor);
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (_highlightObject == null)
        {
            Debug.LogError($"[TileBase] SetHighlighted FAILED: No Highlight child on {gameObject.name}");
            return;
        }

        _isHighlighted = highlighted;
        _highlightObject.SetActive(highlighted);
    }

    public bool IsHighlighted()
    {
        return _isHighlighted;
    }

    protected virtual void OnDestroy()
    {
        if (_highlightMaterial != null)
        {
            Destroy(_highlightMaterial);
        }
    }
}
