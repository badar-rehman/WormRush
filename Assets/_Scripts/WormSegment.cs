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
    private Vector2 mouseStartPos;
    private float moveCooldown = 0.15f; // Time between allowed moves
    private float moveTimer = 0f;

    private float gridMoveThreshold = 50f; // pixels to trigger 1-tile move
    
    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnMouseDown()
    {
        isDragging = true;
        mouseStartPos = Input.mousePosition;
    }

    public void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector2 currentMousePos = Input.mousePosition;
        Vector2 dragDelta = currentMousePos - mouseStartPos;

        if (dragDelta.magnitude < gridMoveThreshold) return;

        Vector3 direction = Vector3.zero;

        // Determine main cardinal direction
        if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
        {
            direction = dragDelta.x > 0 ? Vector3.right : Vector3.left;
        }
        else
        {
            direction = dragDelta.y > 0 ? Vector3.forward : Vector3.back;
        }

        TryMoveInDirection(direction);

        // Reset mouse start for continuous drag movement
        mouseStartPos = currentMousePos;
    }

    public void OnMouseUp()
    {
        isDragging = false;
    }

    private void TryMoveInDirection(Vector3 direction)
    {
        var head = worm.GetHeadSegment();
        Vector3 headPos = head.transform.position;
        Vector3 targetPos = new Vector3(
            Mathf.Round(headPos.x + direction.x),
            0f,
            Mathf.Round(headPos.z + direction.z)
        );

        Tile targetTile = GridManager.instance.GetTileAtPosition(targetPos);
        if (targetTile != null && !targetTile.IsOccupied)
        {
            worm.MoveWorm(targetPos);
        }
    }
}
