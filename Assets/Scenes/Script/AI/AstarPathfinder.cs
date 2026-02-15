using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AStarPathfinder : MonoBehaviour
{
    private PathFindingGrid grid;
    
    void Awake()
    {
        grid = GetComponent<PathFindingGrid>();
    }
    
    /// <summary>
    /// Trouve un chemin entre startPos et targetPos
    /// </summary>
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        PathNode startNode = grid.NodeFromWorldPoint(startPos);
        PathNode targetNode = grid.NodeFromWorldPoint(targetPos);
        
        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            // Trouver le node avec le fCost le plus bas
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || 
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }
            
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            
            // On a trouvé le chemin !
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }
            
            // Explorer les voisins
            foreach (PathNode neighbor in grid.GetNeighbors(currentNode, includeDiagonals: false))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }
                
                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;
                    
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        
        // Pas de chemin trouvé
        Debug.LogWarning("A* : Aucun chemin trouvé !");
        return new List<Vector3>();
    }
    
    /// <summary>
    /// Reconstitue le chemin en remontant les parents
    /// </summary>
    List<Vector3> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        
        path.Reverse();
        
        // Simplifier le chemin (optionnel)
        List<Vector3> waypoints = SimplifyPath(path);
        
        Debug.Log($"A* : Chemin trouvé avec {waypoints.Count} waypoints");
        return waypoints;
    }
    
    /// <summary>
    /// Simplifie le chemin en ne gardant que les points de changement de direction
    /// </summary>
    List<Vector3> SimplifyPath(List<PathNode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(
                path[i - 1].gridX - path[i].gridX, 
                path[i - 1].gridY - path[i].gridY
            );
            
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i - 1].worldPosition);
            }
            
            directionOld = directionNew;
        }
        
        // Ajouter le dernier point
        if (path.Count > 0)
        {
            waypoints.Add(path[path.Count - 1].worldPosition);
        }
        
        return waypoints;
    }
    
    /// <summary>
    /// Distance entre deux nodes (heuristique de Manhattan ou Euclidienne)
    /// </summary>
    int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        // Manhattan distance (plus rapide, pas de diagonales)
        return dstX + dstY;
        
        // OU Euclidienne (si diagonales autorisées) :
        // return Mathf.RoundToInt(Mathf.Sqrt(dstX * dstX + dstY * dstY) * 10);
    }
}