using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// SquadController CORRIGÉ - récupère correctement les soldats
/// </summary>
public class SquadController : MonoBehaviour
{
    [Header("References")]
    public Squad squad;
    public SquadPathFollower pathFollower;
    
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
        if (pathFollower == null) pathFollower = GetComponent<SquadPathFollower>();
    }
    
    void Start()
    {
        RefreshSoldierList();
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadController] {soldiers.Count} soldats trouvés");
        }
    }
    
    void Update()
    {
        if (autoSeekCoverOnArrival && !hasReachedDestination)
        {
            if (pathFollower != null && !pathFollower.IsFollowingPath())
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
    
    /// <summary>
    /// Rafraîchir la liste des soldats - CORRIGÉ
    /// </summary>
    public void RefreshSoldierList()
    {
        soldiers.Clear();
        
        // Méthode 1 : Depuis Squad.soldiers (si c'est une liste de GameObjects ou Transforms)
        if (squad != null && squad.soldiers != null)
        {
            foreach (var soldierObj in squad.soldiers)
            {
                if (soldierObj != null)
                {
                    // Essayer de récupérer SoldierAgent depuis le GameObject
                    SoldierAgent agent = null;
                    // Si c'est un Component (comme SoldierBehavior ou autre)
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
        
        // Méthode 2 (backup) : Chercher tous les enfants de cette squad
        if (soldiers.Count == 0)
        {
            SoldierAgent[] childSoldiers = GetComponentsInChildren<SoldierAgent>();
            soldiers.AddRange(childSoldiers);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SquadController] Méthode backup : {soldiers.Count} soldats trouvés dans les enfants");
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadController] RefreshSoldierList: {soldiers.Count} soldats");
        }
    }
    
    /// <summary>
    /// Déplacer la squad vers une destination
    /// </summary>
    public void MoveSquadToDestination(Vector3 destination)
    {
        if (pathFollower == null)
        {
            Debug.LogError($"Pas de SquadPathFollower !");
            return;
        }
        
        hasReachedDestination = false;
        
        // Démarrer le pathfinding
        pathFollower.MoveSquadToDestination(destination);
        
        // IMPORTANT : Mettre tous les soldats en SquadMovementState
        int transitionCount = 0;
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.JoinSquadMovement();
                transitionCount++;
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadController] Moving to {destination}");
            Debug.Log($"[SquadController] {transitionCount} soldats en SquadMovementState");
        }
    }
    
    /// <summary>
    /// Ordonner de chercher des covers proches
    /// </summary>
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
                //soldier.SeekNearbyCover();
            }
        }
    }
    
    public void StopSquad()
    {
        hasReachedDestination = false;
        
        if (pathFollower != null)
        {
            pathFollower.StopFollowing();
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
    
    // === QUERIES ===
    
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
}