using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Testing : MonoBehaviour
{

    public Tile start;
    public Tile end;
    List<Tile> path = new List<Tile>();
    
    [Button("Test", ButtonSizes.Gigantic)]
    public void Test()
    {
        path = GridManager.instance.GetShortestPath(start, end);
    }

    public Color color = Color.red;
    public float size = 0.1f;
    private void OnDrawGizmos()
    {
        if(path.Count == 0) return;
        for (int i = 0; i < path.Count; i++)
        {
            Gizmos.color = color;
            Gizmos.DrawCube(path[i].transform.position + Vector3.up, size * Vector3.one);
        }
    }
}