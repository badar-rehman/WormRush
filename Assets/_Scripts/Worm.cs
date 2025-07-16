using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Worm : MonoBehaviour
{
    [SerializeField] WormSegment headPrefab;
    [SerializeField] WormSegment bodyPrefab;
    [SerializeField] private int length = 4;
    [SerializeField] private Vector2Int[] bodyPositions;
    
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
            Vector3 position = new Vector3(bodyPositions[i].x, 0.5f, bodyPositions[i].y);
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
    
    public void MoveWorm(Vector3 newHeadPosition)
    {
        if (segments.Count == 0) return;

        Vector3 newHeadGridPos = new Vector3(newHeadPosition.x, 0f,newHeadPosition.z);
        Tile newTile = GridManager.instance.GetTileAtPosition(newHeadGridPos);

        if (newTile == null || newTile.IsOccupied) return; // Don't move to occupied or invalid tiles

        // Store old tile references
        List<Tile> oldTiles = new();
        foreach (var seg in segments)
        {
            var tile = GridManager.instance.GetTileAtPosition(new Vector3(seg.transform.position.x, 0f, seg.transform.position.z));
            oldTiles.Add(tile);
        }

        // Update segment positions
        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].transform.position = segments[i - 1].transform.position;
        }

        segments[0].transform.position = newHeadPosition;

        // Mark new tiles
        for (int i = 0; i < segments.Count; i++)
        {
            Vector3 pos = new Vector3(segments[i].transform.position.x, 0f, segments[i].transform.position.z);
            var tile = GridManager.instance.GetTileAtPosition(pos);
            tile?.SetOccupied(true);
        }

        // Clear old tile occupancy
        for (int i = 1; i < oldTiles.Count; i++)
        {
            oldTiles[i]?.SetOccupied(false);
        }

        // Update bodyPositions
        for (int i = 0; i < segments.Count; i++)
        {
            if (i < bodyPositions.Length)
            {
                bodyPositions[i] = new Vector2Int(
                    Mathf.RoundToInt(segments[i].transform.position.x),
                    Mathf.RoundToInt(segments[i].transform.position.z)
                );
            }
        }
    }
    
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