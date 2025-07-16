using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
 
public class GridManager : MonoBehaviour {
    
    public static GridManager instance;
    
    [SerializeField] private int _width, _height;
 
    [SerializeField] private Tile _tilePrefab;
 
    [SerializeField] private Transform _cam;
 
    private Dictionary<Vector3, Tile> _tiles;

    private void Awake()
    {
        instance = this;
    }

    void Start() 
    {
        GenerateGrid();
    }
 
    [Button("Generate Grid")]
    void GenerateGrid() 
    {
        _tiles = new Dictionary<Vector3, Tile>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, 0, y), Quaternion.identity,transform);
                spawnedTile.name = $"Tile {x} {y}";
 
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
 
 
                _tiles[new Vector3(x, 0, y)] = spawnedTile;
            }
        }

        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, 8f, -0.89f); //-(float)_height / 2 - 0.5f);
    }
 
    public Tile GetTileAtPosition(Vector2 pos) 
    {
        if (_tiles.TryGetValue(pos, out var tile)) return tile;
        return null;
    }

    #region Gizmos
    [FoldoutGroup("Gizmos")]
    [SerializeField] private Color gizmoTileColor = Color.green;
    [FoldoutGroup("Gizmos")]
    [SerializeField] private Color gizmoOutlineColor = Color.green;
    [FoldoutGroup("Gizmos")]
    [SerializeField] private Vector3 gizmoSize;
    [FoldoutGroup("Gizmos")]
    [SerializeField] [Range(0f, 1f)] private float gizmoHeight;
    #endregion
    private void OnDrawGizmos()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Gizmos.color = gizmoTileColor;
                Gizmos.DrawCube(new Vector3(x, gizmoHeight, y), gizmoSize);
                Gizmos.color = gizmoOutlineColor;
                Gizmos.DrawWireCube(new Vector3(x, gizmoHeight, y), gizmoSize);
            }
        }
    }
}