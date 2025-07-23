using UnityEngine;

public class TouchManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private WormSegment selectedSegment = null;
    private Vector2 touchStartPos;
    private float moveTimer = 0f;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            BeginTouch(Input.mousePosition);
        else if (Input.GetMouseButton(0))
            ContinueTouch(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            EndTouch();
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                BeginTouch(touch.position);
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                ContinueTouch(touch.position);
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                EndTouch();
        }
#endif
    }

    void BeginTouch(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.SphereCast(ray, GameConfigs.instance.touchSphereRadius, out RaycastHit hit, 100f, GameConfigs.instance.wormSegmentLayer))
        {
            selectedSegment = hit.collider.GetComponent<WormSegment>();
            touchStartPos = screenPos;
            moveTimer = 0f;
        }
    }

    void ContinueTouch(Vector2 currentScreenPos)
    {
        if (selectedSegment == null || selectedSegment.worm.IsMoving()) return;

        moveTimer += Time.deltaTime;

        if (moveTimer < GameConfigs.instance.moveCooldown)
            return;

        Vector2 dragDelta = currentScreenPos - touchStartPos;

        if (dragDelta.magnitude < GameConfigs.instance.gridMoveThreshold)
            return;

        Vector3 inputDir;
        if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
            inputDir = dragDelta.x > 0 ? Vector3.right : Vector3.left;
        else
            inputDir = dragDelta.y > 0 ? Vector3.forward : Vector3.back;

        selectedSegment.worm.TryMove(inputDir, selectedSegment.IsCloseToHead);

        // Reset drag state for continuous swiping
        touchStartPos = currentScreenPos;
        moveTimer = 0f;
    }

    void EndTouch()
    {
        selectedSegment = null;
        moveTimer = 0f;
    }
}
