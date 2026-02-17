using UnityEngine;
using System.Collections.Generic;

public class PathNode
{
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public bool isWalkable;

    public int gCost;
    public int hCost;
    public PathNode parent;
    
    public PathNode(Vector3 worldPos, int x, int y, bool walkable)
    {
        worldPosition = worldPos;
        gridX = x;
        gridY = y;
        isWalkable = walkable;
    }
    
    public int fCost 
    { 
        get { return gCost + hCost; } 
    }
}

public class PathFindingGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2 gridWorldSize = new Vector2(50, 100);
    public float nodeRadius = 0.5f;
    public LayerMask unwalkableMask;
    
    [Header("Grid Position")]
    public Transform gridCenter;
    
    private PathNode[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    
    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        
        CreateGrid();
    }
    
    void CreateGrid()
    {
        grid = new PathNode[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = gridCenter.position 
            - Vector3.right * gridWorldSize.x / 2 
            - Vector3.forward * gridWorldSize.y / 2;
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft 
                    + Vector3.right * (x * nodeDiameter + nodeRadius) 
                    + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);
                
                grid[x, y] = new PathNode(worldPoint, x, y, walkable);
            }
        }
        
        Debug.Log($"Grid créée : {gridSizeX}x{gridSizeY} = {gridSizeX * gridSizeY} nodes");
    }

    public List<PathNode> GetNeighbors(PathNode node, bool includeDiagonals = false)
    {
        List<PathNode> neighbors = new List<PathNode>();

        CheckAndAddNeighbor(neighbors, node.gridX - 1, node.gridY);
        CheckAndAddNeighbor(neighbors, node.gridX + 1, node.gridY);
        CheckAndAddNeighbor(neighbors, node.gridX, node.gridY + 1);
        CheckAndAddNeighbor(neighbors, node.gridX, node.gridY - 1);

        if (includeDiagonals)
        {
            CheckAndAddNeighbor(neighbors, node.gridX - 1, node.gridY + 1);
            CheckAndAddNeighbor(neighbors, node.gridX + 1, node.gridY + 1);
            CheckAndAddNeighbor(neighbors, node.gridX - 1, node.gridY - 1);
            CheckAndAddNeighbor(neighbors, node.gridX + 1, node.gridY - 1);
        }
        
        return neighbors;
    }
    
    void CheckAndAddNeighbor(List<PathNode> neighbors, int x, int y)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            neighbors.Add(grid[x, y]);
        }
    }

    public PathNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - (gridCenter.position 
            - Vector3.right * gridWorldSize.x / 2 
            - Vector3.forward * gridWorldSize.y / 2);
        
        int x = Mathf.Clamp(Mathf.RoundToInt(localPos.x / nodeDiameter), 0, gridSizeX - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(localPos.z / nodeDiameter), 0, gridSizeY - 1);
        
        return grid[x, y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(gridCenter.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        
        if (grid != null)
        {
            foreach (PathNode node in grid)
            {
                Gizmos.color = node.isWalkable ? Color.white : Color.red;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}