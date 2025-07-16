using UnityEngine;
using UnityEngine.EventSystems;

public class WormSegment : MonoBehaviour
{
    public Worm worm;
    private bool isDragging = false;
    private Vector3 offset;
    private Plane dragPlane;
    private Camera mainCamera;
    private Vector3 lastPosition;
    private Vector3 direction;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnMouseDown()
    {
        isDragging = true;
        lastPosition = transform.position;
        
        // Create a plane to drag on
        dragPlane = new Plane(Vector3.up, transform.position);
        
        // Calculate the offset between the click position and the object's position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            offset = transform.position - ray.GetPoint(distance);
        }
    }

    public void OnMouseDrag()
    {
        if (!isDragging) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 newPosition = ray.GetPoint(distance) + offset;
            newPosition = new Vector3(Mathf.Round(newPosition.x), 0.5f, Mathf.Round(newPosition.z));

            Vector3 currentPos = transform.position;
            Vector3 delta = newPosition - currentPos;

            // Only allow cardinal direction moves
            if (Mathf.Abs(delta.x) + Mathf.Abs(delta.z) != 1) return;

            // Only move if the tile is not occupied
            Tile targetTile = GridManager.instance.GetTileAtPosition(new Vector3(newPosition.x, 0f, newPosition.z));
            if (targetTile == null || targetTile.IsOccupied) return;

            worm.MoveWorm(newPosition);
        }
    }


    public void OnMouseUp()
    {
        isDragging = false;
    }
}
