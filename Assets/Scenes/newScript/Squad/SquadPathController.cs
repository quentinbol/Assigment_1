using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère le pathfinding A* et le suivi du chemin pour une squad
/// Utilisé par SquadController pour orchestrer le mouvement
/// </summary>
public class SquadPathFollower : MonoBehaviour
{
    [Header("References")]
    public AStarPathfinder pathfinder;
    public Squad squad;
    
    [Header("Path Settings")]
    public float waypointReachedDistance = 2f;
    public float pathUpdateInterval = 1f;
    
    [Header("Visual Debug")]
    public bool showPath = true;
    public Color pathColor = Color.cyan;
    public Color waypointColor = Color.yellow;
    
    private List<Vector3> path = new List<Vector3>();
    [SerializeField]
    private int currentWaypointIndex = 0;
    [SerializeField]
    private bool isFollowingPath = false;
    private Vector3 destination;
    private float lastPathUpdateTime = 0f;
    
    // Propriétés publiques
    public bool IsFollowingPath() => isFollowingPath;
    public int CurrentWaypointIndex => currentWaypointIndex;
    public Vector3 CurrentWaypoint => path.Count > currentWaypointIndex ? path[currentWaypointIndex] : Vector3.zero;
    public List<Vector3> Path => path;
    
    void Awake()
    {
        if (pathfinder == null)
        {
            pathfinder = FindFirstObjectByType<AStarPathfinder>();
        }
        
        if (squad == null)
        {
            squad = GetComponent<Squad>();
        }
    }
    
    void Update()
    {
        if (!isFollowingPath) return;
        
        // Suivre le chemin
        FollowPath();
        
        // Mise à jour périodique du path (replanning)
        if (Time.time - lastPathUpdateTime > pathUpdateInterval)
        {
            UpdatePath();
            lastPathUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// Calcule et démarre le suivi d'un chemin vers une destination
    /// </summary>
    public void MoveSquadToDestination(Vector3 targetDestination)
    {
        destination = targetDestination;
        
        if (pathfinder == null)
        {
            Debug.LogError($"{squad.squadName} : Pathfinder introuvable !");
            return;
        }
        
        // Calculer le chemin A*
        Vector3 startPos = squad.GetSquadCenter();
        path = pathfinder.FindPath(startPos, destination);
        
        if (path.Count > 0)
        {
            isFollowingPath = true;
            currentWaypointIndex = 0;
            lastPathUpdateTime = Time.time;
            
            Debug.Log($"[{squad.squadName}] A* : Chemin trouvé avec {path.Count} waypoints");
        }
        else
        {
            Debug.LogWarning($"[{squad.squadName}] A* : Aucun chemin trouvé vers {destination}");
            isFollowingPath = false;
        }
    }
    
    /// <summary>
    /// Suit le chemin en avançant aux waypoints successifs
    /// </summary>
    private void FollowPath()
    {
        if (path.Count == 0 || currentWaypointIndex >= path.Count)
        {
            OnPathCompleted();
            return;
        }
        
        // Vérifier si on a atteint le waypoint actuel
        Vector3 squadCenter = squad.GetSquadCenter();
        float distanceToWaypoint = Vector3.Distance(squadCenter, path[currentWaypointIndex]);
        
        if (distanceToWaypoint < waypointReachedDistance)
        {
            // Passer au waypoint suivant
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= path.Count)
            {
                OnPathCompleted();
            }
            else
            {
                Debug.Log($"[{squad.squadName}] Waypoint {currentWaypointIndex}/{path.Count} atteint");
            }
        }
    }
    
    /// <summary>
    /// Recalcule le chemin périodiquement (replanning)
    /// </summary>
    private void UpdatePath()
    {
        if (!isFollowingPath) return;
        
        Vector3 startPos = squad.GetSquadCenter();
        List<Vector3> newPath = pathfinder.FindPath(startPos, destination);
        
        if (newPath.Count > 0)
        {
            path = newPath;
            currentWaypointIndex = 0;
        }
    }
    
    /// <summary>
    /// Appelé quand le chemin est terminé
    /// </summary>
    private void OnPathCompleted()
    {
        isFollowingPath = false;
        currentWaypointIndex = 0;
        
        Debug.Log($"[{squad.squadName}] Path terminé - destination atteinte");
        
        // Notifier le SquadController si présent
        SquadController controller = GetComponent<SquadController>();
        if (controller != null)
        {
            // Le controller peut déclencher l'assignation des covers ici
        }
    }
    
    /// <summary>
    /// Obtient le waypoint actuel vers lequel la squad doit se diriger
    /// </summary>
    public Vector3 GetCurrentWaypoint()
    {
        if (path.Count == 0 || currentWaypointIndex >= path.Count)
        {
            return squad.GetSquadCenter();
        }
        
        return path[currentWaypointIndex];
    }
    
    /// <summary>
    /// Arrête le suivi du chemin
    /// </summary>
    public void StopFollowing()
    {
        isFollowingPath = false;
        path.Clear();
        currentWaypointIndex = 0;
        
        Debug.Log($"[{squad.squadName}] Path following stopped");
    }
    
    /// <summary>
    /// Obtient le nombre de waypoints restants
    /// </summary>
    public int GetRemainingWaypoints()
    {
        if (!isFollowingPath || path.Count == 0) return 0;
        return path.Count - currentWaypointIndex;
    }
    
    /// <summary>
    /// Obtient le pourcentage de progression sur le chemin
    /// </summary>
    public float GetPathProgress()
    {
        if (path.Count == 0) return 1f;
        return (float)currentWaypointIndex / path.Count;
    }
    
    void OnDrawGizmos()
    {
        if (!showPath || path.Count == 0) return;
        
        // Dessiner le chemin complet
        Gizmos.color = pathColor;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }
        
        // Dessiner les waypoints
        foreach (Vector3 waypoint in path)
        {
            Gizmos.DrawWireSphere(waypoint, 0.5f);
        }
        
        // Dessiner le waypoint actuel en surbrillance
        if (isFollowingPath && currentWaypointIndex < path.Count)
        {
            Gizmos.color = waypointColor;
            Gizmos.DrawSphere(path[currentWaypointIndex], 0.8f);
            
            // Ligne vers le waypoint actuel depuis le centre de la squad
            Gizmos.color = Color.green;
            if (squad != null)
            {
                Gizmos.DrawLine(squad.GetSquadCenter(), path[currentWaypointIndex]);
            }
        }
        
        // Dessiner la destination finale
        if (isFollowingPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(destination, 1f);
        }
    }
}