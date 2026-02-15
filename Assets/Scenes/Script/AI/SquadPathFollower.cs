/*using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Composant à ajouter au Squad pour suivre un chemin A*
/// </summary>
public class SquadPathFollower : MonoBehaviour
{
    [Header("References")]
    public Squad squad;
    public AStarPathfinder pathfinder;
    
    [Header("Path Following Settings")]
    public float waypointReachedDistance = 2f; // Distance pour considérer un waypoint atteint
    public float pathUpdateInterval = 1f; // Recalculer le chemin tous les X secondes
    
    [Header("Path Visualization")]
    public bool showPath = true;
    public Color pathColor = Color.cyan;
    
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private float lastPathUpdateTime;
    private Vector3 targetDestination;
    private bool isFollowingPath = false;

    public Vector3 currentWaypoint { get { return GetCurrentWaypoint(); } }
    
    void Start()
    {
        if (squad == null)
        {
            squad = GetComponent<Squad>();
        }
        
        if (pathfinder == null)
        {
            pathfinder = FindFirstObjectByType<AStarPathfinder>();
        }
    }
    
    void Update()
    {
        if (!isFollowingPath) return;
        
        // Recalculer le chemin périodiquement
        if (Time.time - lastPathUpdateTime > pathUpdateInterval)
        {
            UpdatePath();
        }
        
        // Suivre le chemin
        FollowPath();
    }
    
    /// <summary>
    /// Démarre le suivi d'un chemin vers une destination
    /// </summary>
    public void MoveSquadToDestination(Vector3 destination)
    {
        targetDestination = destination;
        UpdatePath();
        isFollowingPath = true;
    }
    
    /// <summary>
    /// Arrête le suivi du chemin
    /// </summary>
    public void StopFollowing()
    {
        isFollowingPath = false;
        currentPath.Clear();
        currentWaypointIndex = 0;
    }
    
    /// <summary>
    /// Recalcule le chemin A*
    /// </summary>
    void UpdatePath()
    {
        if (pathfinder == null) return;
        
        Vector3 squadCenter = squad.GetSquadCenter();
        currentPath = pathfinder.FindPath(squadCenter, targetDestination);
        currentWaypointIndex = 0;
        lastPathUpdateTime = Time.time;
        
        if (currentPath.Count == 0)
        {
            Debug.LogWarning($"{squad.squadName} : Aucun chemin trouvé vers la destination !");
            isFollowingPath = false;
        }
    }
    
    /// <summary>
    /// Logique de suivi du chemin
    /// </summary>
    void FollowPath()
    {
        if (currentPath.Count == 0) return;
        
        // Waypoint actuel
        Vector3 currentWaypoint = currentPath[currentWaypointIndex];
        Vector3 squadCenter = squad.GetSquadCenter();
        
        // Vérifier si on a atteint le waypoint
        float distanceToWaypoint = Vector3.Distance(squadCenter, currentWaypoint);
        
        if (distanceToWaypoint < waypointReachedDistance)
        {
            currentWaypointIndex++;
            
            // Fin du chemin atteint
            if (currentWaypointIndex >= currentPath.Count)
            {
                Debug.Log($"{squad.squadName} : Destination atteinte !");
                isFollowingPath = false;
                OnPathCompleted();
            }
        }
        
        // Diriger la squad vers le waypoint actuel
        //return currentWaypoint;
        //MoveSquadTowardsWaypoint(currentWaypoint);
    }
    
    /// <summary>
    /// Déplace la squad vers un waypoint spécifique
    /// </summary>
    public Vector3 MoveSquadTowardsWaypoint(Vector3 waypoint)
    {
        // Pour l'instant, on utilise juste le flocking existant
        // en définissant le waypoint comme "target moyen" pour tous les soldats
        
        // Option 1 : Tous les soldats vont vers le même waypoint
        // (simple mais peut créer des regroupements)
        /*foreach (var soldier in squad.soldiers)
        {
            if (soldier != null && soldier.currentState == SoldierState.Moving)
            {
                // On crée un "virtual target" au lieu d'utiliser le cover directement
                // soldier.SetTarget(waypoint); // Impossible car SetTarget attend un Transform
                
                // À la place, on modifie directement leur comportement d'arrivée
                //Vector3 arriveForce = soldier.Arrive(waypoint) * soldier.arriveWeight;
                //soldier.ApplyForce(arriveForce);
                return waypoint; // On peut aussi stocker ce waypoint dans le soldier pour qu'il l'utilise dans son Update

            }
        }

        return waypoint;
        
        // Option 2 : Formation autour du waypoint (plus avancé)
        // DistributeSquadAroundWaypoint(waypoint);
    }
    
    /// <summary>
    /// Appelé quand le chemin est complété
    /// </summary>
    void OnPathCompleted()
    {
        // Ici tu peux déclencher la logique de cover
        // Par exemple, assigner les covers aux soldats
        
        Debug.Log($"{squad.squadName} : Chemin complété, recherche de covers...");
    }
    
    /// <summary>
    /// Vérifie si la squad suit actuellement un chemin
    /// </summary>
    public bool IsFollowingPath()
    {
        return isFollowingPath;
    }
    
    /// <summary>
    /// Obtient le waypoint actuel
    /// </summary>
    public Vector3 GetCurrentWaypoint()
    {
        if (currentPath.Count > 0 && currentWaypointIndex < currentPath.Count)
        {
            return currentPath[currentWaypointIndex];
        }
        return squad.GetSquadCenter();
    }
    
    // Visualisation
    void OnDrawGizmos()
    {
        if (!showPath || currentPath == null || currentPath.Count == 0) return;
        
        Gizmos.color = pathColor;
        
        // Dessiner les lignes du chemin
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Gizmos.DrawLine(currentPath[i] + Vector3.up * 0.5f, currentPath[i + 1] + Vector3.up * 0.5f);
        }
        
        // Dessiner les waypoints
        foreach (var waypoint in currentPath)
        {
            Gizmos.DrawWireSphere(waypoint, 0.5f);
        }
        
        // Mettre en évidence le waypoint actuel
        if (currentWaypointIndex < currentPath.Count)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentPath[currentWaypointIndex], 0.7f);
        }
    }
}*/