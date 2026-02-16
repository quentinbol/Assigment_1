using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// SquadController compatible avec WaypointPathFollower
/// </summary>
public class SquadController : MonoBehaviour
{
    [Header("References")]
    public Squad squad;
    
    [Header("Path Follower (choisir UN seul)")]
    public WaypointPathFollower waypointPathFollower; // Nouveau système
    public SquadPathFollower squadPathFollower; // Ancien système (backup)
    
    [Header("Auto Cover")]
    public bool autoSeekCoverOnArrival = true;
    public float searchRadius = 15f;
    public float delayBeforeSeekCover = 0.5f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    [Header("Soldiers (Debug)")]
    [SerializeField] private List<SoldierAgent> soldiers = new List<SoldierAgent>();
    private bool hasReachedDestination = false;
    
    void Awake()
    {
        if (squad == null) squad = GetComponent<Squad>();
        
        // Auto-detect path follower
        if (waypointPathFollower == null)
            waypointPathFollower = GetComponent<WaypointPathFollower>();
        
        if (squadPathFollower == null)
            squadPathFollower = GetComponent<SquadPathFollower>();
    }
    
    void Start()
    {
        RefreshSoldierList();
    }
    
    void Update()
    {
        if (autoSeekCoverOnArrival && !hasReachedDestination)
        {
            bool pathComplete = false;
            
            // Vérifier selon le système utilisé
            if (waypointPathFollower != null)
            {
                pathComplete = !waypointPathFollower.IsFollowingPath();
            }
            else if (squadPathFollower != null)
            {
                pathComplete = !squadPathFollower.IsFollowingPath();
            }
            
            if (pathComplete)
            {
                bool wasMoving = false;
                foreach (var soldier in soldiers)
                {
                    if (soldier != null && soldier.StateMachine != null && 
                        soldier.StateMachine.IsInState<SquadMovementState>())
                    {
                        wasMoving = true;
                        break;
                    }
                }
                
                if (wasMoving)
                {
                    hasReachedDestination = true;
                    Invoke(nameof(OrderSquadToSeekNearbyCover), delayBeforeSeekCover);
                }
            }
        }
    }
    
    public void RefreshSoldierList()
    {
        soldiers.Clear();
        
        if (squad != null && squad.soldiers != null)
        {
            foreach (var soldierObj in squad.soldiers)
            {
                if (soldierObj != null)
                {
                    SoldierAgent agent = null;

                    if (soldierObj is Component)
                    {
                        agent = (soldierObj as Component).GetComponent<SoldierAgent>();
                    }
                    
                    if (agent != null)
                    {
                        soldiers.Add(agent);
                    }
                }
            }
        }
        
        if (soldiers.Count == 0)
        {
            SoldierAgent[] childSoldiers = GetComponentsInChildren<SoldierAgent>();
            soldiers.AddRange(childSoldiers);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadController] {soldiers.Count} soldats trouvés");
        }
    }
    
    /// <summary>
    /// Démarre le mouvement de la squad
    /// </summary>
    public void StartMovement()
    {
        Debug.Log("[SquadController] === StartMovement appelé ===");
        
        hasReachedDestination = false;
        
        // Debug des références
        Debug.Log($"[SquadController] waypointPathFollower = {(waypointPathFollower != null ? "OK" : "NULL")}");
        Debug.Log($"[SquadController] squadPathFollower = {(squadPathFollower != null ? "OK" : "NULL")}");
        
        // Démarrer selon le système disponible
        if (waypointPathFollower != null)
        {
            Debug.Log("[SquadController] Appel de waypointPathFollower.StartFollowingPath()");
            waypointPathFollower.StartFollowingPath();
            
            if (showDebugLogs)
            {
                Debug.Log("[SquadController] Démarrage du WaypointPath");
            }
        }
        else if (squadPathFollower != null)
        {
            // Fallback ancien système
            Debug.LogWarning("[SquadController] WaypointPathFollower non trouvé, utilise ancien système");
        }
        else
        {
            Debug.LogError("[SquadController] Aucun PathFollower trouvé !");
            return;
        }
        
        // Mettre tous les soldats en SquadMovementState
        int transitionCount = 0;
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.JoinSquadMovement(); // Mouvement normal de squad
                transitionCount++;
            }
        }
        
        // Démarrer la coordination de cover si présente
        SquadCoverCoordinator coordinator = GetComponent<SquadCoverCoordinator>();
        if (coordinator != null)
        {
            coordinator.StartCoordination();
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadController] {transitionCount} soldats en mouvement");
        }
    }
    
    /// <summary>
    /// Version legacy pour compatibilité
    /// </summary>
    public void MoveSquadToDestination(Vector3 destination)
    {
        // Si on utilise WaypointPath, ignorer la destination
        if (waypointPathFollower != null)
        {
            StartMovement();
        }
        else if (squadPathFollower != null)
        {
            squadPathFollower.MoveSquadToDestination(destination);
            
            foreach (var soldier in soldiers)
            {
                if (soldier != null)
                {
                    soldier.JoinSquadMovement();
                }
            }
        }
    }
    
    public void OrderSquadToSeekNearbyCover()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[SquadController] Cherche covers (rayon {searchRadius}m)");
        }
        
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.SeekNearbyCover();
            }
        }
    }
    
    public void StopSquad()
    {
        hasReachedDestination = false;
        
        if (waypointPathFollower != null)
        {
            waypointPathFollower.StopFollowing();
        }
        
        if (squadPathFollower != null)
        {
            squadPathFollower.StopFollowing();
        }
        
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.StopAndIdle();
            }
        }
    }
    
    public void OrderSquadToLeaveCover()
    {
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.ReleaseCover();
                soldier.StopAndIdle();
            }
        }
    }
    
    public bool IsSquadInCover()
    {
        foreach (var soldier in soldiers)
        {
            if (soldier != null && !soldier.IsInCover())
                return false;
        }
        return true;
    }
    
    public bool IsSquadMoving()
    {
        foreach (var soldier in soldiers)
        {
            if (soldier != null && soldier.IsMoving())
                return true;
        }
        return false;
    }
    
    public int GetAliveCount()
    {
        int count = 0;
        foreach (var soldier in soldiers)
        {
            if (soldier != null && soldier.gameObject.activeSelf)
                count++;
        }
        return count;
    }
    
    public int GetInCoverCount()
    {
        int count = 0;
        foreach (var soldier in soldiers)
        {
            if (soldier != null && soldier.IsInCover())
                count++;
        }
        return count;
    }
    
    public Vector3 GetSquadCenter()
    {
        if (soldiers.Count == 0) return transform.position;
        
        Vector3 center = Vector3.zero;
        int count = 0;
        
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
            {
                center += soldier.transform.position;
                count++;
            }
        }
        
        return count > 0 ? center / count : transform.position;
    }
    
    /// <summary>
    /// Retourne la liste des soldats (pour SquadCoverCoordinator)
    /// </summary>
    public List<SoldierAgent> GetSoldiers()
    {
        return soldiers;
    }
}