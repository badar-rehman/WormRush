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

    // Round the new head position to match grid
    newHeadPosition = new Vector3(
        Mathf.Round(newHeadPosition.x),
        Mathf.Round(newHeadPosition.y),
        Mathf.Round(newHeadPosition.z)
    );

    // Check if the target tile exists and is unoccupied
    Tile targetTile = GridManager.instance.GetTileAtPosition(newHeadPosition);
    if (targetTile == null || targetTile.IsOccupied) return;

    // Store old tile references (before movement)
    List<Tile> oldTiles = new();
    foreach (var seg in segments)
    {
        Vector3 segPos = seg.transform.position;
        Tile tile = GridManager.instance.GetTileAtPosition(segPos);
        oldTiles.Add(tile);
    }

    // Store current head position before moving
    Vector3 previousHeadPosition = segments[0].transform.position;

    // Move segments: tail follows the one in front
    for (int i = segments.Count - 1; i > 0; i--)
    {
        segments[i].transform.position = segments[i - 1].transform.position;
    }

    // Move the head to the new position
    segments[0].transform.position = newHeadPosition;

    // Rotate the head to face the direction of movement
    Vector3 moveDirection = newHeadPosition - previousHeadPosition;
    if (moveDirection != Vector3.zero)
    {
        Quaternion rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        segments[0].transform.rotation = rotation;
    }

    // Mark new tile positions as occupied
    for (int i = 0; i < segments.Count; i++)
    {
        Vector3 segPos = segments[i].transform.position;
        Tile tile = GridManager.instance.GetTileAtPosition(segPos);
        tile?.SetOccupied(true);
    }

    // Unmark old tail tile (only the last segment leaves its tile)
    if (oldTiles.Count > 0)
    {
        oldTiles[^1]?.SetOccupied(false);
    }

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