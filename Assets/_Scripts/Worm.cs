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
    
    #region Gizmos
    [FoldoutGroup("Gizmos")]
    [SerializeField] private Color gizmoHeadColor = Color.green;
    [FoldoutGroup("Gizmos")]
    [SerializeField] private Color gizmoBodyColor = Color.green;
    [FoldoutGroup("Gizmos")]
    [SerializeField] [Range(0f, 1f)] private float gizmoSize = 0.5f;
    #endregion
    
    [SerializeField] [DisableIf("segments")] private List<WormSegment> segments = new List<WormSegment>();
    
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

            segment.worm = this;
            segments.Add(segment);

            // Mark tile as occupied
            var tile = GridManager.instance.GetTileAtPosition(new Vector3(position.x, 0f,position.z));
            tile?.SetOccupied(true);
        }
        
        //Set head rotation
        if (segments.Count > 1)
        {
            Vector3 directionToSecondSegment = segments[0].transform.localPosition - segments[1].transform.localPosition;
            Quaternion headRotation = Quaternion.LookRotation(-directionToSecondSegment);
            segments[0].transform.localEulerAngles = headRotation.eulerAngles + new Vector3(0, 180, 0);
        }
    }

    private void ClearWorm()
    {
        segments.Clear();
    }
    
    public WormSegment GetHeadSegment()
    {
        return segments.Count > 0 ? segments[0] : null;
    }
    public WormSegment GetTailSegment()
    {
        return segments.Count > 0 ? segments[^1] : null;
    }
    
    public void MoveWorm(Vector3 newHeadPosition)
    {
        if (isMoving) return;

        StartCoroutine(MoveRoutine(newHeadPosition));
    }

    private IEnumerator MoveRoutine(Vector3 newHeadPosition)
    {
        isMoving = true;

        // Save previous positions
        List<Vector3> oldPositions = new List<Vector3>();
        foreach (var segment in segments)
        {
            oldPositions.Add(segment.transform.position);
        }

        // Update tile states before animation
        Tile oldTailTile = GridManager.instance.GetTileAtPosition(oldPositions[^1]);
        if (oldTailTile != null)
            oldTailTile.IsOccupied = false;

        Tile newHeadTile = GridManager.instance.GetTileAtPosition(newHeadPosition);
        if (newHeadTile != null)
            newHeadTile.IsOccupied = true;

        // Update rotation
        Vector3 moveDir = newHeadPosition - oldPositions[0];
        if (moveDir != Vector3.zero)
        {
            segments[0].transform.DOLookAt(segments[0].transform.position + moveDir, moveDuration);
        }
        
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            segments[0].transform.position = Vector3.Lerp(oldPositions[0], newHeadPosition, t);

            for (int i = 1; i < segments.Count; i++)
            {
                segments[i].transform.position = Vector3.Lerp(oldPositions[i], oldPositions[i - 1], t);
            }

            yield return null;
        }

        // Snap to final positions
        segments[0].transform.position = newHeadPosition;
        for (int i = 1; i < segments.Count; i++)
        {
            segments[i].transform.position = oldPositions[i - 1];
        }
        
        //set tiles state
        foreach (var segment in segments)
        {
            Tile tile = GridManager.instance.GetTileAtPosition(segment.transform.position);
            if (tile != null)
                tile.IsOccupied = true;
        }

        isMoving = false;
        
        // Update internal bodyPositions array for gizmos/debugging
        for (int i = 0; i < segments.Count; i++)
        {
            if (i < bodyPositions.Length)
            {
                Vector3 segPos = segments[i].transform.position;
                bodyPositions[i] = new Vector2Int(
                    Mathf.RoundToInt(segPos.x),
                    Mathf.RoundToInt(segPos.z)
                );
            }
        }
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
}