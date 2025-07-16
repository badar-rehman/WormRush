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
        
        // Initialize bodyPositions if it's null or empty
        if (bodyPositions == null || bodyPositions.Length == 0)
        {
            bodyPositions = new Vector2Int[length];
            for (int i = 0; i < bodyPositions.Length; i++)
            {
                bodyPositions[i] = new Vector2Int(i, 0);
            }
        }
        
        // Make sure we don't exceed the bodyPositions array
        int segmentsToCreate = Mathf.Min(length, bodyPositions.Length);
        
        for (int i = 0; i < segmentsToCreate; i++)
        {
            Vector3 position = new Vector3(bodyPositions[i].x, 0.5f, bodyPositions[i].y);
            if (i == 0)
            {
                var head = Instantiate(headPrefab, position, Quaternion.identity, transform);
                head.worm = this;
                segments.Add(head);
            }
            else
            {
                var body = Instantiate(bodyPrefab, position, Quaternion.identity, transform);
                body.worm = this;
                segments.Add(body);
            }
        }

        if (segments.Count > 1)
        {
            //set head rotation to face away from second segment
            Vector3 directionToSecondSegment = segments[0].transform.localPosition - segments[1].transform.localPosition;
            Quaternion headRotation = Quaternion.LookRotation(-directionToSecondSegment);
            Debug.Log(headRotation.eulerAngles);
            Debug.Log(segments[0].transform.localEulerAngles);
            segments[0].transform.localEulerAngles = headRotation.eulerAngles + new Vector3(0, 180, 0);
            Debug.Log(segments[0].transform.localEulerAngles);
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
        
        // Store the previous positions
        Vector3[] previousPositions = new Vector3[segments.Count];
        for (int i = 0; i < segments.Count; i++)
        {
            previousPositions[i] = segments[i].transform.position;
        }
        
        // Move the head to the new position
        segments[0].transform.position = newHeadPosition;
        
        // Move each body segment to the previous position of the segment in front of it
        for (int i = 1; i < segments.Count; i++)
        {
            segments[i].transform.position = previousPositions[i - 1];
        }
        
        // Update the body positions for gizmos
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