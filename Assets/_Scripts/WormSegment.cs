using UnityEngine;

public class WormSegment : MonoBehaviour
{
    public Worm worm;
    private bool isDragging = false;
    private Vector2 mouseStartPos;
    private float gridMoveThreshold = 50f; // Pixels to trigger move
    private float moveCooldown = 0.15f;
    private float moveTimer = 0f;
    private Camera mainCamera;
    
    public Vector3 Pos
    {
        get => transform.position;
        set => transform.position = value;
    }

    public Vector3 LPos
    {
        get => transform.localPosition;
        set => transform.localPosition = value;
    }
    
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

        moveTimer += Time.deltaTime;
        if (moveTimer < moveCooldown) return;

        Vector2 currentMousePos = Input.mousePosition;
        Vector2 dragDelta = currentMousePos - mouseStartPos;

        if (dragDelta.magnitude < gridMoveThreshold) return;

        Vector3 inputDir = Vector3.zero;

        // Determine input direction in world (X-Z plane)
        if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
            inputDir = dragDelta.x > 0 ? Vector3.right : Vector3.left;
        else
            inputDir = dragDelta.y > 0 ? Vector3.forward : Vector3.back;

        TryMoveBasedOnAlignment(inputDir);

        // Reset drag for smooth continuous dragging
        mouseStartPos = currentMousePos;
        moveTimer = 0f;
    }

    public void OnMouseUp()
    {
        isDragging = false;
        moveTimer = 0f;
    }

    private void TryMoveBasedOnAlignment(Vector3 inputDir)
    {
        if (worm == null || worm.IsMoving()) return;

        Vector3 headDir = worm.GetHeadDirection();
        Vector3 tailDir = worm.GetTailDirection();

        float headDot = Vector3.Dot(inputDir.normalized, headDir.normalized);
        float tailDot = Vector3.Dot(inputDir.normalized, tailDir.normalized);

        if (headDot > tailDot)
        {
            worm.MoveWormFromHead(inputDir);
        }
        else
        {
            worm.MoveWormFromTail(inputDir);
        }
    }
}