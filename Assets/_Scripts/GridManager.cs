using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
 
public class GridManager : MonoBehaviour {
    
    public static GridManager instance;
    
    [SerializeField] private int _width, _height;
    [SerializeField] private int numOfObstacles;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private Transform _cam;
    
    private Dictionary<Vector3, Tile> _tiles;
    private Tile[,] tileArray;
    
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

    private void Awake()
    {
        instance = this;
    }
 
    [Button("Generate Grid")]
    public void GenerateGrid() 
    {
        _tiles = new Dictionary<Vector3, Tile>();
        tileArray = new Tile[_width, _height];
        
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, 0, y), Quaternion.identity,transform);
                spawnedTile.name = $"Tile {x} {y}";
                
                spawnedTile.Init(x,y);
                
                _tiles[new Vector3(x, 0, y)] = spawnedTile;
                tileArray[x, y] = spawnedTile;
            }
        }

        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, 16f, 3.28f); //-(float)_height / 2 - 0.5f);
    }

    public void SpawnObstacles()
    {
        if (_obstaclePrefab == null)
        {
            Debug.LogError("Obstacle prefab is not assigned in GridManager!");
            return;
        }

        // Get all unoccupied tiles
        List<Vector3> unoccupiedTiles = new List<Vector3>();
        foreach (var tile in _tiles.Values)
        {
            if (!tile.IsOccupied)
            unoccupiedTiles.Add(tile.transform.position);
        }

        // Shuffle the list to get random tiles
        for (int i = 0; i < unoccupiedTiles.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, unoccupiedTiles.Count);
            (unoccupiedTiles[i], unoccupiedTiles[randomIndex]) = (unoccupiedTiles[randomIndex], unoccupiedTiles[i]);
        }

        // Spawn up to 3 obstacles or the number of available unoccupied tiles, whichever is smaller
        int obstaclesToSpawn = Mathf.Min(numOfObstacles, unoccupiedTiles.Count);
        
        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            Vector3 spawnPos = unoccupiedTiles[i];
            Tile tile = GetTileAtPosition(spawnPos);
            if (tile != null && !tile.IsOccupied)
            {
                Instantiate(_obstaclePrefab, spawnPos, Quaternion.identity, transform);
                tile.SetOccupied(true);
            }
        }
    }

    public Tile GetTileAtPosition(Vector3 pos) 
    {
        if (_tiles.TryGetValue(Vector3Int.RoundToInt(pos), out var tile))
        {
            return tile;
        }
        else
        {
            Debug.Log("TileNotFound at " + pos);
            return null;
        }
    }

    public Tile GetUnOccupiedTileAtPosition(Vector3 pos)
    {
        if (_tiles.TryGetValue(Vector3Int.RoundToInt(pos), out var tile))
        {
            if (tile.IsOccupied)
                return null;

            return tile;
        }

        Debug.Log("TileNotFound at " + pos);
        return null;
    }

    public Tile GetClosestTileFromPositionOutsideTheGrid()
    {
        Tile closestTile = null;
        float closestTileDistance = float.MaxValue;
        //itterate over all tiles and find the closest one
        foreach (var tile in _tiles.Values)
        {
            if (tile.IsOccupied)
                continue;
            
            float distance = Vector3.Distance(tile.transform.position, _cam.position);
            if (closestTile == null || distance < closestTileDistance)
            {
                closestTile = tile;
                closestTileDistance = distance;
            }
        }
        
        return closestTile;
    }

    
    #region Pathfinding
    public List<Tile> GetShortestPath(Tile startTile, Tile endTile)
    {
        if (startTile == null || endTile == null || startTile.IsOccupied || endTile.IsOccupied)
            return null;

        // Data structures for A*
        HashSet<Tile> closedSet = new HashSet<Tile>();
        PriorityQueue<Tile> openSet = new PriorityQueue<Tile>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();

        Dictionary<Tile, float> gScore = new Dictionary<Tile, float>();
        Dictionary<Tile, float> fScore = new Dictionary<Tile, float>();

        openSet.Enqueue(startTile, 0);
        gScore[startTile] = 0;
        fScore[startTile] = Heuristic(startTile, endTile);

        while (openSet.Count > 0)
        {
            Tile current = openSet.Dequeue();

            if (current == endTile)
                return ReconstructPath(cameFrom, current);

            closedSet.Add(current);

            foreach (Tile neighbor in GetNeighbours(current))
            {
                if (neighbor.IsOccupied || closedSet.Contains(neighbor))
                    continue;

                float tentativeG = gScore[current] + 1; // All moves cost 1

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, endTile);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return null; // No path found
    }    
    
    private float Heuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
    }

    private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> path = new List<Tile>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    private List<Tile> GetNeighbours(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        int[,] directions = new int[,]
        {
            { 1, 0 },
            { -1, 0 },
            { 0, 1 },
            { 0, -1 }
        };

        for (int i = 0; i < 4; i++)
        {
            int nx = tile.x + directions[i, 0];
            int ny = tile.y + directions[i, 1];

            if (nx >= 0 && ny >= 0 && nx < _width && ny < _height)
            {
                Tile neighbor = tileArray[nx, ny];
                if (neighbor != null)
                    neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    
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