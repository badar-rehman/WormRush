using UnityEngine;

public class TouchManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    
    private WormSegment selectedSegment = null;
    private Vector2 touchStartPos;
    private float moveTimer = 0f;

    [SerializeField] private GameConfigs gameConfigs;
    
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

    // void BeginTouch(Vector2 screenPos)
    // {
    //     Ray ray = mainCamera.ScreenPointToRay(screenPos);
    //     if (Physics.SphereCast(ray, gameConfigs.touchSphereRadius, out RaycastHit hit, 100f, gameConfigs.wormSegmentLayer))
    //     {
    //         selectedSegment = hit.collider.GetComponent<WormSegment>();
    //         touchStartPos = screenPos;
    //         moveTimer = 0f;
    //     }
    // }
    public LayerMask planeLayerMask;
    void BeginTouch(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
       // Debug.Log("BeginTouch " + screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, planeLayerMask))
        {
           // Debug.Log("Water hit " + hit.point + " " + hit.collider.gameObject.name);
            var pos = hit.point + Vector3.up * 0.5f;
            
            var cols = Physics.OverlapSphere(pos, gameConfigs.touchSphereRadius, gameConfigs.wormSegmentLayer);
          //  Debug.Log("Cols " + cols.Length);
            if (cols.Length > 0)
            {
                selectedSegment = cols[0].GetComponent<WormSegment>();
                touchStartPos = screenPos;
                moveTimer = 0f;
            }
        }
    }

    private Tile selectedTile;
    public LayerMask tileLayerMask;
    void ContinueTouch(Vector2 screenPos)
    {
        if (selectedSegment == null || selectedSegment.worm.IsMoving()) return;

        Tile currentTile = null;
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayerMask))
        {
            currentTile = hit.collider.GetComponent<Tile>();
        }

        if (currentTile && currentTile != selectedTile && !currentTile.IsOccupied)
        {
            selectedTile = currentTile;
            if (selectedSegment.worm.MoveToTile(selectedTile))
            {
                selectedTile = currentTile;
            }
        }
    }

    // void ContinueTouch(Vector2 currentScreenPos)
    // {
    //     if (selectedSegment == null || selectedSegment.worm.IsMoving()) return;
    //
    //     moveTimer += Time.deltaTime;
    //
    //     if (moveTimer < gameConfigs.moveCooldown)
    //         return;
    //
    //     Vector2 dragDelta = currentScreenPos - touchStartPos;
    //
    //     if (dragDelta.magnitude < gameConfigs.gridMoveThreshold)
    //         return;
    //
    //     Vector3 inputDir;
    //     if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
    //         inputDir = dragDelta.x > 0 ? Vector3.right : Vector3.left;
    //     else
    //         inputDir = dragDelta.y > 0 ? Vector3.forward : Vector3.back;
    //
    //     selectedSegment.worm.TryMove(inputDir, selectedSegment.IsCloseToHead);
    //
    //     // Reset drag state for continuous swiping
    //     touchStartPos = currentScreenPos;
    //     moveTimer = 0f;
    // }

    void EndTouch()
    {
        selectedSegment = null;
        selectedTile = null;
        moveTimer = 0f;
    }
}
