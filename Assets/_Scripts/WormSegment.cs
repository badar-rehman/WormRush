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
        // Only allow dragging if this is the head segment
        if (this != worm.GetHeadSegment()) return;
        
        isDragging = true;
        lastPosition = transform.position;
        
        // Create a plane to drag on
        dragPlane = new Plane(Vector3.up, transform.position);
        
        // Calculate the offset between the click position and the object's position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (dragPlane.Raycast(ray, out distance))
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
            // Snap to grid
            newPosition = new Vector3(Mathf.Round(newPosition.x), 0.5f, Mathf.Round(newPosition.z));
            
            // Only move if the position has changed
            if (newPosition != transform.position)
            {
                // Calculate movement direction
                direction = (newPosition - transform.position).normalized;
                
                // Move the worm
                worm.MoveWorm(newPosition);
            }
        }
    }

    public void OnMouseUp()
    {
        isDragging = false;
    }
}
