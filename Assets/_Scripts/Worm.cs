using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Worm : MonoBehaviour
{
    [SerializeField] WormSegment headPrefab;
    [SerializeField] WormSegment bodyPrefab;
    [SerializeField] private int length = 4;
    [SerializeField] private Vector2Int[] bodyPositions;
    
    public float moveDuration = 0.15f; // Smooth duration

    private bool isMoving = false;
    
    [SerializeField] [DisableIf("segments")] private List<WormSegment> segments = new List<WormSegment>();
    
    #region Gizmos
    [FoldoutGroup("Gizmos")]
    [SerializeField] private Color gizmoHeadColor = Color.green;
    [FoldoutGroup("Gizmos")]
    [SerializeField] private Color gizmoBodyColor = Color.green;
    [FoldoutGroup("Gizmos")]
    [SerializeField] [Range(0f, 1f)] private float gizmoSize = 0.5f;
    #endregion
    
    
    [Button("Create Worm")]
    public void CreateWorm()
    {
        ClearWorm();

        if (bodyPositions == null || bodyPositions.Length == 0)
        {
            bodyPositions = new Vector2Int[length];
            for (int i = 0; i < bodyPositions.Length; i++)
            {
                bodyPositions[i] = new Vector2Int(i, 0);
            }
        }

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

            segment.Setup(this, i, i < segmentsToCreate/2);
            segments.Add(segment);

            // Mark tile as occupied
            var tile = GridManager.instance.GetTileAtPosition(new Vector3(position.x, 0f,position.z));
            tile?.SetOccupied(true);
        }
        
        //Set head rotation
        if (segments.Count > 1)
        {
            Vector3 directionToSecondSegment = segments[0].LPos - segments[1].LPos;
            Quaternion headRotation = Quaternion.LookRotation(-directionToSecondSegment);
            segments[0].transform.localEulerAngles = headRotation.eulerAngles + new Vector3(0, 180, 0);
        }
    }

    private void ClearWorm()
    {
        segments.Clear();
    }
    
    public WormSegment HeadSeg => segments.Count > 0 ? segments[0] : null;
    public WormSegment TailSeg => segments.Count > 0 ? segments[^1] : null;
    

    public void MoveWormFromHead(Vector3 direction)
    {
        Vector3 headPos = segments[0].Pos;
        Vector3 targetPos = headPos + direction;

        Tile targetTile = GridManager.instance.GetTileAtPosition(targetPos);
        if (targetTile != null && !targetTile.IsOccupied)
        {
            MoveWorm(targetPos, fromHead: true);
        }
    }

    public void MoveWormFromTail(Vector3 direction)
    {
        Vector3 tailPos = segments[^1].Pos;
        Vector3 targetPos = tailPos + direction;

        Tile targetTile = GridManager.instance.GetTileAtPosition(targetPos);
        if (targetTile != null && !targetTile.IsOccupied)
        {
            MoveWorm(targetPos, fromHead: false);
        }
    }
    public void MoveWorm(Vector3 newPos, bool fromHead)
    {
        if (isMoving) return;
        StartCoroutine(MoveCoroutine(newPos, fromHead));
    }


    private IEnumerator MoveCoroutine(Vector3 newPos, bool fromHead)
    {
        isMoving = true;

        Vector3Int gridPos = Vector3Int.RoundToInt(newPos);

        Tile targetTile = GridManager.instance.GetTileAtPosition(gridPos);
        if (targetTile == null || targetTile.IsOccupied)
        {
            isMoving = false;
            yield break;
        }

        // Mark the new tile as occupied
        targetTile.IsOccupied = true;

        // Determine which segment will vacate its tile
        WormSegment exitingSegment = fromHead ? segments[^1] : segments[0];
        Tile exitingTile =
            GridManager.instance.GetTileAtPosition(Vector3Int.RoundToInt(exitingSegment.Pos));
        
        float duration = moveDuration;
        float elapsed = 0f;

        // Shift segment positions and start rotation tweens
        Vector3[] startPositions = new Vector3[segments.Count];
        for (int i = 0; i < segments.Count; i++)
        {
            startPositions[i] = segments[i].Pos;
            
            if (fromHead)
            {
                segments[i].transform.DOLookAt(i == 0 ? newPos : segments[i - 1].Pos, duration * 0.95f);
            }
            else
            {
                segments[i].transform.DOLookAt(i == 0 ? exitingTile.transform.position : segments[i - 1].Pos, duration * 0.95f);
            }
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
                        segments[i].transform.position = Vector3.Lerp(startPositions[i], newPos, t);
                    else
                        segments[i].transform.position = Vector3.Lerp(startPositions[i], startPositions[i - 1], t);
                }
                else // move from tail
                {
                    if (i == segments.Count - 1)
                        segments[i].transform.position = Vector3.Lerp(startPositions[i], newPos, t);
                    else
                        segments[i].transform.position = Vector3.Lerp(startPositions[i], startPositions[i + 1], t);
                }
            }

            yield return null;
        }

        // Final snapping
        if (fromHead)
        {
            segments[0].transform.position = newPos;
            for (int i = 1; i < segments.Count; i++)
                segments[i].transform.position = startPositions[i - 1];
        }
        else
        {
            segments[^1].transform.position = newPos;
            for (int i = 0; i < segments.Count - 1; i++)
                segments[i].transform.position = startPositions[i + 1];
        }

        // Free the old tile
        if (exitingTile != null)
            exitingTile.IsOccupied = false;

        isMoving = false;
    }
    
    public bool IsMoving() => isMoving;

    private void OnDrawGizmos()
    {
        //draw sphere at each position
        for (int i = 0; i < bodyPositions.Length; i++)
        {
            Gizmos.color = i == 0 ? gizmoHeadColor : gizmoBodyColor;
            Gizmos.DrawSphere(new Vector3(bodyPositions[i].x, 0.5f, bodyPositions[i].y), gizmoSize);
        }
    }

    public void TryMove(Vector3 inputDir, bool isCloseToHead)
    {
        if (isCloseToHead)
        {
            if (!IsMoving() && inputDir == HeadSeg.transform.forward)
            {
                Vector3 newPos = HeadSeg.Pos + inputDir;
                Tile tile = GridManager.instance.GetTileAtPosition(newPos);
                if(tile)
                    StartCoroutine(MoveCoroutine(newPos, true));
            }
        }
        else
        {
            
        }
    }
}