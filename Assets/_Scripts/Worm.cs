using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
 
public class Worm : MonoBehaviour
{
    [SerializeField] WormSegment headPrefab;
    [SerializeField] WormSegment bodyPrefab;
    
    [FoldoutGroup("Movement")][ReadOnly] private bool isMoving = false;
    
    [SerializeField] private Vector2Int[] bodyPositions;
    [SerializeField] [DisableIf("segments")] private List<WormSegment> segments = new List<WormSegment>();
    
    private int length = 4;
    
    public WormSegment HeadSeg => segments.Count > 0 ? segments[0] : null;
    public WormSegment TailSeg => segments.Count > 0 ? segments[^1] : null;
 
    public bool IsMoving() => isMoving;
    
    private List<Vector3> movePrevPositions = new List<Vector3>();
    private List<Vector3> moveNewPositions = new List<Vector3>();
 
    [SerializeField] private GameConfigs gameConfigs;
    
    [Button("Create Worm")]
    public void CreateWorm()
    {
        ClearWorm();
        lineRenderer = GetComponent<LineRenderer>();
        
        
        if (bodyPositions == null || bodyPositions.Length == 0)
        {
            Debug.LogError("Worm body positions not set");
            return;
        }
        
        length = bodyPositions.Length;
        
        int segmentsToCreate = Mathf.Min(length, bodyPositions.Length);
 
        for (int i = 0; i < segmentsToCreate; i++)
        {
            Vector3 position = new Vector3(bodyPositions[i].x, 0f, bodyPositions[i].y);
            WormSegment segment;
            
            if (i == 0)
            {
                segment = Instantiate(headPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                segment = Instantiate(bodyPrefab, position, Quaternion.identity, transform);
            }
            
            movePrevPositions.Add(position);
            
            segment.Setup(this, i, i < segmentsToCreate/2);
            segments.Add(segment);
 
            // Mark tile as occupied
            var tile = GridManager.instance.GetTileAtPosition(new Vector3(position.x, 0f,position.z));
            tile?.SetOccupied(true);
        }
        
        Vector3 direction = HeadSeg.Pos - segments[1].Pos;
        HeadSeg.transform.LookAt(HeadSeg.Pos + direction);
        for (int i = 1; i < segments.Count; i++)
        {
            segments[i].transform.LookAt(segments[i - 1].Pos);
        }
        lineRenderer.positionCount = segments.Count;
        UpdateLineRend();
    }
 
    private void ClearWorm()
    {
        segments.Clear();
        movePrevPositions.Clear();
    }
    
    private IEnumerator MoveCoroutine(Vector3 newPos, bool fromHead, bool forward)
    {
        moveTryPos = newPos;
        isMoving = true;
 
        Vector3Int gridPos = Vector3Int.RoundToInt(newPos);
 
        Tile targetTile = GridManager.instance.GetTileAtPosition(gridPos);
        if (targetTile == null || targetTile.IsOccupied)
        {
            isMoving = false;
            yield break;
        }
        
        float duration = gameConfigs.moveDuration;
        float elapsed = 0f;
        
        //Store previous positions
        movePrevPositions.Clear();
        foreach (WormSegment segment in segments)
            movePrevPositions.Add(segment.Pos);
        
        //store new positions
        moveNewPositions.Clear();
        for (int i = 0; i < segments.Count; i++)
        {
            if (fromHead)
                moveNewPositions.Add(i==0 ? newPos : movePrevPositions[i - 1]);
            else // move from tail
                moveNewPositions.Add(i == segments.Count - 1 ? newPos : movePrevPositions[i + 1]);
        }
 
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
 
            for (int i = 0; i < segments.Count; i++)
            {
                if (fromHead)
                {
                    if (i == 0)
                    {
                        segments[i].transform.position = Vector3.Lerp(movePrevPositions[i], newPos, t);
                        segments[i].transform.LookAt(newPos);
                    }
                    else
                    {
                        segments[i].transform.position = Vector3.Lerp(movePrevPositions[i], movePrevPositions[i - 1], t);
                        segments[i].transform.LookAt(segments[i - 1].Pos);
                    }
                }
                else // move from tail
                {
                    if (i == segments.Count - 1)
                    {
                        segments[i].transform.position = Vector3.Lerp(movePrevPositions[i], newPos, t);
                        segments[i].transform.LookAt(segments[i - 1].Pos);
                    }
                    else
                    {
                        segments[i].transform.position = Vector3.Lerp(movePrevPositions[i], movePrevPositions[i + 1], t);
                        segments[i].transform.LookAt(i==0 ? HeadSeg.Pos+(HeadSeg.Pos - segments[1].Pos) : segments[i + 1].Pos);
                    }
                }
            }
 
            yield return null;
        }
 
        // Final snapping
        if (fromHead)
        {
            segments[0].transform.position = newPos;
            for (int i = 1; i < segments.Count; i++)
                segments[i].transform.position = movePrevPositions[i - 1];
        }
        else
        {
            segments[^1].transform.position = newPos;
            for (int i = 0; i < segments.Count - 1; i++)
                segments[i].transform.position = movePrevPositions[i + 1];
        }
 
        foreach (var prevPos in movePrevPositions)
        {
            GridManager.instance.GetTileAtPosition(prevPos).IsOccupied = false;
        }
        foreach (var newSegPos in segments)
        {
            GridManager.instance.GetTileAtPosition(newSegPos.Pos).IsOccupied = true;
        }
 
        isMoving = false;
    }
 
    public void TryMove(Vector3 inputDir, bool isCloseToHead)
    {
        if (isCloseToHead) //dragging from head
        {
            float dot = Mathf.RoundToInt(Vector3.Dot(inputDir, HeadSeg.transform.forward));
            
            if (!IsMoving() && dot >= 0 && dot <= 1) //if draging from head and moving head in head direction
            {
                Vector3 newPos = HeadSeg.Pos + inputDir;
                Tile tile = GridManager.instance.GetTileAtPosition(newPos);
                
                if(tile)
                    StartCoroutine(MoveCoroutine(newPos, true, true));
            }
            else if (!IsMoving() && dot < 0 && dot >= -1) //if draging from head and moving head in opposite direction
            {
                if(GridManager.instance.GetUnOccupiedTileAtPosition(TailSeg.Pos - TailSeg.transform.forward)) //if there is a tile in the direciton of the input
                    StartCoroutine(MoveCoroutine(TailSeg.Pos - TailSeg.transform.forward, false, false)); 
                else if(GridManager.instance.GetUnOccupiedTileAtPosition(TailSeg.Pos + TailSeg.transform.right)) //if there is a tile at right of the input
                    StartCoroutine(MoveCoroutine(TailSeg.Pos + TailSeg.transform.right, false, false));
                else if(GridManager.instance.GetUnOccupiedTileAtPosition(TailSeg.Pos - TailSeg.transform.right)) //if there is a tile at left of the input
                    StartCoroutine(MoveCoroutine(TailSeg.Pos - TailSeg.transform.right, false, false));
            }
        }
        else //dragging from tail
        {
            float dot = Mathf.RoundToInt(Vector3.Dot(inputDir, -TailSeg.transform.forward));
            if (!IsMoving() && dot >= 0 && dot <= 1) //if draging from tail and moving tail in tail direction
            {
                Vector3 newPos = TailSeg.Pos + inputDir;
                Tile tile = GridManager.instance.GetTileAtPosition(newPos);
                
                if(tile)
                    StartCoroutine(MoveCoroutine(newPos, false, true));
            }
            else if (!IsMoving() && dot < 0 && dot >= -1) //if draging from tail and moving tail in opposite direction towards body
            {
                if(GridManager.instance.GetUnOccupiedTileAtPosition(HeadSeg.Pos + HeadSeg.transform.forward)) //if there is a tile in the direciton of the input
                    StartCoroutine(MoveCoroutine(HeadSeg.Pos + HeadSeg.transform.forward, true, false)); 
                else if(GridManager.instance.GetUnOccupiedTileAtPosition(HeadSeg.Pos + HeadSeg.transform.right)) //if there is a tile at right of the input
                    StartCoroutine(MoveCoroutine(HeadSeg.Pos + HeadSeg.transform.right, true, false));
                else if(GridManager.instance.GetUnOccupiedTileAtPosition(HeadSeg.Pos - HeadSeg.transform.right)) //if there is a tile at left of the input
                    StartCoroutine(MoveCoroutine(HeadSeg.Pos - HeadSeg.transform.right, true, false));
            }
        }
    }
 
    public float moveSpeed = 1f;
    private List<List<Vector3>> bodyPathsList;
    public Tween moveTween = null;
    [Button("MoveToTile")]
    public bool MoveToTile(Tile tile)
    {
        if (moveTween != null && moveTween.IsPlaying())
        {
            return false;
        }
        
        var startTile = GridManager.instance.GetTileAtPosition(HeadSeg.Pos);
        
        GridManager.instance.GetTileAtPosition(movePrevPositions[0]).IsOccupied = false;
        
        List<Tile> path = GridManager.instance.GetShortestPath(startTile, tile);
        
        List<Vector3> tilePosPath = new List<Vector3>();
        
        if (path != null)
        {
            if (path.Count <= 0) return false;
            
          //  Debug.Log("Path: " + path.Count);
            
            foreach(var _tile in path)
                tilePosPath.Add(_tile.transform.position);
            
            //bodyPathsList.Clear();
            bodyPathsList = new List<List<Vector3>>();
            
            bodyPathsList.Add(tilePosPath);
            
            for (int i = 1; i < segments.Count; i++)
            {
                GridManager.instance.GetTileAtPosition(movePrevPositions[i]).IsOccupied = false;
                List<Vector3> segPosPath = new List<Vector3>();
                
                foreach (var poss in tilePosPath)
                    segPosPath.Add(poss);
                
                for (int j = 0; j < i; j++)
                {
                    segPosPath.Insert(0, segments[j].Pos);
                }
                
                for (int j = i; j > 0; j--)
                {
                    //remove last element
                    segPosPath.RemoveAt(segPosPath.Count - 1);
                }
                
                bodyPathsList.Add(segPosPath);
            }
            
            
            float duration = GetPathDistance(segments[0].Pos,bodyPathsList[0].ToArray()) / moveSpeed;
            movePrevPositions.Clear();
            for (int i = 0; i < segments.Count; i++)
            {
                var lookAt = i==0 ? null : segments[i - 1].transform;
                var seg = segments[i];
 
                Vector3[] pathArray = bodyPathsList[i].ToArray();
                // float duration = GetPathDistance(seg.Pos,pathArray) / moveSpeed;
                // Debug.Log("PathArray : " + pathArray.Length + " Duration: " + duration);
                GridManager.instance.GetTileAtPosition(seg.Pos).IsOccupied = false;
                GridManager.instance.GetTileAtPosition(pathArray[^1]).IsOccupied = true;
                movePrevPositions.Add(pathArray[^1]);
                if (i > 0)
                {
                    seg.transform.DOPath(pathArray, duration, gizmoColor: Color.magenta).SetEase(Ease.Linear)
                        .OnUpdate(() =>
                        {
                            if (lookAt != null)
                                seg.transform.LookAt(lookAt);
                        });
                }
                else
                {
                    moveTween = seg.transform.DOPath(pathArray, duration, gizmoColor: Color.magenta).SetEase(Ease.Linear)
                        .SetLookAt(0.1f).OnComplete(() =>
                        {
                            moveTween = null;
                        });
                }
            }
            return true;
        }
        else
        {
          //  Debug.Log("Path not found");
            GridManager.instance.GetTileAtPosition(movePrevPositions[0]).IsOccupied = true;
            return false;
        }
    }
 
    private float GetPathDistance(Vector3 pos,Vector3[] pathArray)
    {
        if (pathArray.Length == 0) return 0;
        
        float distance = Vector3.Distance(pos, pathArray[0]);
        for (int i = 0; i < pathArray.Length - 1; i++)
        {
            distance += Vector3.Distance(pathArray[i], pathArray[i + 1]);
        }
        return distance;
    }
 
 
    private Vector3 moveTryPos;
    private void OnDrawGizmos()
    {
        //draw sphere at each position
        for (int i = 0; i < bodyPositions.Length; i++)
        {
            Gizmos.color = i == 0 ? gameConfigs.wormGizmoHeadColor : gameConfigs.wormGizmoBodyColor;
            
            if(segments.Count > 0)
                Gizmos.DrawSphere(segments[i].Pos + Vector3.up * 0.5f, gameConfigs.wormGizmoSize);
            else
                Gizmos.DrawSphere(new Vector3(bodyPositions[i].x, 0.5f, bodyPositions[i].y), gameConfigs.wormGizmoSize);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(moveTryPos + Vector3.up, gameConfigs.wormGizmoSize/2f);
    }


    public Tile targetTile;

    public void SetTargetTile(Tile tile)
    {
        if (targetTile == tile) return;
        Debug.Log("SetTargetTile " + tile.transform.position);
        targetTile = tile;
    }

    private Tile currentTargetTile;
    // private bool lerping = false;
    private List<Tile> headPath = new List<Tile>();
    private List<Tile> tailPath = new List<Tile>();
    List<Vector3> prevBodyPos  = new List<Vector3>();
    private Vector3 nextStepPos;
    public enum WormMoveState
    {
        Idle,
        Forward,
        Backward,
    }
    WormMoveState moveState = WormMoveState.Idle;
    public float rotSpeed = 10;
    private float totalStepDistance;
    private float coveredStepDistance;
    LineRenderer lineRenderer;
    void Update()
    {
        if (moveState == WormMoveState.Idle && targetTile != null && currentTargetTile != targetTile)
        {
            Debug.Log("SetCurrentTile " + targetTile.transform.position);
            currentTargetTile = targetTile;
            
            Tile headTile = GridManager.instance.GetTileAtPosition(HeadSeg.Pos);
            if (headTile != null)
            {
                headTile.IsOccupied = false;
                headPath = GridManager.instance.GetShortestPath(headTile, currentTargetTile);
                headTile.IsOccupied = true;
            }
            
            Tile tailTile = GridManager.instance.GetTileAtPosition(TailSeg.Pos);
            if (tailTile != null)
            {
                tailTile.IsOccupied = false;
                tailPath = GridManager.instance.GetShortestPath(tailTile, currentTargetTile);
                tailTile.IsOccupied = true;
            }

            for (int i = 0; i < segments.Count; i++)
            {
                prevBodyPos.Add(segments[i].Pos);
            }

            if (headPath != null && tailPath != null && headPath.Count > 0 && tailPath.Count > 0)
            {
                if (headPath.Count <= tailPath.Count)
                {
                    moveState = WormMoveState.Forward;
                    nextStepPos = headPath[0].transform.position;
                    totalStepDistance = Vector3.Distance(nextStepPos, HeadSeg.Pos);
                }
                else
                {
                    moveState = WormMoveState.Backward;
                    nextStepPos = tailPath[0].transform.position;
                    totalStepDistance = Vector3.Distance(nextStepPos, TailSeg.Pos);
                }
            }
            else if (headPath != null && headPath.Count > 0)
            {
                moveState = WormMoveState.Forward;
                nextStepPos = headPath[0].transform.position;
                totalStepDistance = Vector3.Distance(nextStepPos, HeadSeg.Pos);
            }
            else if (tailPath != null && tailPath.Count > 0)
            {
                moveState = WormMoveState.Backward;
                nextStepPos = tailPath[0].transform.position;
                totalStepDistance = Vector3.Distance(nextStepPos, TailSeg.Pos);
            }
        }

        if (moveState == WormMoveState.Forward)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                Vector3 movePos = i == 0 ? nextStepPos : prevBodyPos[i - 1];
                segments[i].Pos = 
                    Vector3.MoveTowards(segments[i].Pos, movePos, Time.deltaTime * moveSpeed);
                
                coveredStepDistance = totalStepDistance - Vector3.Distance(segments[i].Pos, movePos);
                
                // Calculate direction to next position
                Vector3 direction = i==0? (movePos - segments[i].Pos).normalized : (prevBodyPos[i - 1] - segments[i].Pos).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    segments[i].transform.rotation = Quaternion.Slerp(
                        segments[i].transform.rotation, 
                        targetRotation, 
                        coveredStepDistance/totalStepDistance
                    );
                }
            }
            if (Vector3.Distance(HeadSeg.Pos, nextStepPos) == 0f)
            {
                moveState = WormMoveState.Idle;
                headPath.Clear();
                headPath = null;
                currentTargetTile = null;
                GridManager.instance.GetTileAtPosition(HeadSeg.Pos).IsOccupied = true;
                GridManager.instance.GetTileAtPosition(prevBodyPos[^1]).IsOccupied = false;
                prevBodyPos.Clear();
            }
            if (HeadSeg.Pos == targetTile.transform.position)
            {
                targetTile = null;
            }
            UpdateLineRend();
        }
        else if (moveState == WormMoveState.Backward)
        {
            
            for (int i = 0; i < segments.Count; i++)
            {
                Vector3 movePos = i == segments.Count - 1 ? nextStepPos : prevBodyPos[i + 1];
                segments[i].Pos = 
                    Vector3.MoveTowards(segments[i].Pos, movePos, Time.deltaTime * moveSpeed);
                
                coveredStepDistance = totalStepDistance - Vector3.Distance(segments[i].Pos, movePos);
                
                // Calculate direction to next position
                Vector3 direction = i==0 ? -(movePos - segments[i].Pos).normalized : (segments[i].Pos - prevBodyPos[i - 1]).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    segments[i].transform.rotation = Quaternion.Slerp(
                        segments[i].transform.rotation, 
                        targetRotation, 
                        coveredStepDistance/totalStepDistance
                    );
                }
            }
            if (Vector3.Distance(TailSeg.Pos, nextStepPos) == 0f)
            {
                moveState = WormMoveState.Idle;
                tailPath.Clear();
                tailPath = null;
                currentTargetTile = null;
                GridManager.instance.GetTileAtPosition(TailSeg.Pos).IsOccupied = true;
                GridManager.instance.GetTileAtPosition(prevBodyPos[0]).IsOccupied = false;
                prevBodyPos.Clear();
            }
            if (TailSeg.Pos == targetTile.transform.position)
            {
                targetTile = null;
            }
            UpdateLineRend();
        }
    }

    private void UpdateLineRend()
    {
        for (int i = 0; i < segments.Count; i++)
            lineRenderer.SetPosition(i, segments[i].Pos);
    }
}