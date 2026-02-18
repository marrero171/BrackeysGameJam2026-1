using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera currentCamera;
    public TileGrid currentTileGrid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse position
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = currentCamera.ScreenPointToRay(mousePosition);


            bool somethingIsHit = Physics.Raycast(ray, out RaycastHit raycastHit);

            if (somethingIsHit)
            {
                // Get tile component and try swapping
                Debug.Log(raycastHit.transform.name);
                if (raycastHit.transform.TryGetComponent<TileComponent>(out var tileComponent))
                {
                    currentTileGrid.TrySwap(tileComponent.gridPosition);
                }
            }
        }
    }
}
