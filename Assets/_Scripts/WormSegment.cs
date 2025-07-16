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

        // Always base drag plane and offset on the head, not the clicked segment
        Transform head = worm.GetHeadSegment().transform;

        dragPlane = new Plane(Vector3.up, head.position);

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            offset = head.position - hitPoint;
        }
    }

    public void OnMouseDrag()
    {
        if (!isDragging) return;

        Transform head = worm.GetHeadSegment().transform;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance) + offset;
            Vector3 snappedPoint = new Vector3(Mathf.Round(hitPoint.x), 0f, Mathf.Round(hitPoint.z));

            Vector3 headPos = head.position;
            Vector3 delta = snappedPoint - headPos;

            // Only allow movement to a cardinal adjacent tile (no diagonal, 1 step)
            if (Mathf.Abs(delta.x) + Mathf.Abs(delta.z) != 1) return;

            // Check for unoccupied target tile
            Tile targetTile = GridManager.instance.GetTileAtPosition(snappedPoint);
            if (targetTile == null || targetTile.IsOccupied) return;

            worm.MoveWorm(snappedPoint);
        }
    }



    public void OnMouseUp()
    {
        isDragging = false;
    }
}
