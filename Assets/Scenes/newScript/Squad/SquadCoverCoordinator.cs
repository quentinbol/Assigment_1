using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Coordonne les décisions de cover au niveau de la squad
/// La squad entière décide ensemble d'aller vers un cluster de covers
/// </summary>
public class SquadCoverCoordinator : MonoBehaviour
{
    [Header("References")]
    public SquadController squadController;
    public WaypointPathFollower waypointPathFollower;
    public CoverLeaveDecisionMaker leaveDecisionMaker; // NOUVEAU
    
    [Header("Cluster Detection")]
    [Tooltip("Fréquence de scan pour détecter des clusters")]
    public float clusterScanInterval = 1f;
    
    [Tooltip("Distance min pour considérer un cluster (évite clusters trop proches)")]
    public float minClusterDistance = 1f; // RÉDUIT de 8 à 1
    
    [Header("Cover Timing")]
    [Tooltip("Temps que la squad reste en cover avant de repartir")]
    public float timeInCover = 5f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private enum SquadCoverState
    {
        Moving,           // En mouvement vers waypoint
        GoingToCover,     // En route vers un cluster de covers
        InCover,          // Tous en cover, en attente
    }
    
    private SquadCoverState currentState = SquadCoverState.Moving;
    private float lastClusterScanTime = 0f;
    private float timeEnteredCover = 0f;
    private CoverCluster targetCluster = null;
    
    void Update()
    {
        switch (currentState)
        {
            case SquadCoverState.Moving:
                UpdateMovingState();
                break;
                
            case SquadCoverState.GoingToCover:
                UpdateGoingToCoverState();
                break;
                
            case SquadCoverState.InCover:
                UpdateInCoverState();
                break;
        }
    }
    
    /// <summary>
    /// État : Squad en mouvement, scanne pour des clusters
    /// </summary>
    void UpdateMovingState()
    {
        // Scanner périodiquement pour des clusters UNIQUEMENT en état Moving
        if (Time.time - lastClusterScanTime > clusterScanInterval)
        {
            lastClusterScanTime = Time.time;
            ScanForCoverCluster();
        }
    }
    
    /// <summary>
    /// Scanne pour un cluster de covers approprié
    /// </summary>
    void ScanForCoverCluster()
    {
        // NE PAS scanner si on est déjà en train d'aller vers un cluster
        if (currentState != SquadCoverState.Moving)
        {
            return;
        }
        
        if (CoverClusterDetector.Instance == null)
        {
            Debug.LogWarning("[SquadCoverCoordinator] CoverClusterDetector non trouvé !");
            return;
        }
        
        Vector3 squadPosition = squadController.GetSquadCenter();
        int squadSize = squadController.GetAliveCount();
        
        CoverCluster cluster = CoverClusterDetector.Instance.FindBestClusterForSquad(
            squadPosition, 
            squadSize
        );
        
        if (cluster != null)
        {
            // Vérifier distance minimale (ne pas aller à un cluster trop proche, on y est peut-être déjà)
            float distance = Vector3.Distance(squadPosition, cluster.centerPosition);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator] Cluster à {distance:F1}m, min requis: {minClusterDistance}m");
            }
            
            if (distance > minClusterDistance)
            {
                // CLUSTER TROUVÉ ! Ordonner à la squad d'y aller
                OrderSquadToCluster(cluster);
            }
            else if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator] ❌ Cluster trop proche ({distance:F1}m < {minClusterDistance}m) - ignoré");
            }
        }
    }
    
    /// <summary>
    /// Ordonne à la squad d'aller vers un cluster de covers
    /// </summary>
    void OrderSquadToCluster(CoverCluster cluster)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] 🎯 Cluster détecté ! " +
                      $"{cluster.covers.Count} covers à {Vector3.Distance(squadController.GetSquadCenter(), cluster.centerPosition):F1}m");
        }
        
        targetCluster = cluster;
        currentState = SquadCoverState.GoingToCover;
        
        // Assigner un cover à chaque soldat
        List<SoldierAgent> soldiers = squadController.GetSoldiers();
        List<CoverObject> availableCovers = cluster.covers.Where(c => !c.isOccupied).ToList();
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] Soldats: {soldiers.Count}, Covers dispo: {availableCovers.Count}");
        }
        
        for (int i = 0; i < soldiers.Count && i < availableCovers.Count; i++)
        {
            if (soldiers[i] == null)
            {
                Debug.LogWarning($"[SquadCoverCoordinator] Soldier {i} est NULL !");
                continue;
            }
            
            if (availableCovers[i] == null)
            {
                Debug.LogWarning($"[SquadCoverCoordinator] Cover {i} est NULL !");
                continue;
            }
            
            // Assigner le cover
            soldiers[i].AssignCover(availableCovers[i].transform);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator] {soldiers[i].name} → {availableCovers[i].name}");
            }
            
            // Transition vers GoToAssignedCover
            soldiers[i].GoToAssignedCover();
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] 📍 Assignations terminées, état = {currentState}");
        }
    }
    
    /// <summary>
    /// État : Squad en route vers les covers
    /// </summary>
    void UpdateGoingToCoverState()
    {
        // Vérifier si tous les soldats sont en cover
        if (squadController.IsSquadInCover())
        {
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator] ✅ Tous en cover ! Attente de {timeInCover}s");
            }
            
            currentState = SquadCoverState.InCover;
            timeEnteredCover = Time.time;
        }
    }
    
    /// <summary>
    /// État : Squad en cover, évalue stratégiquement si elle peut partir
    /// </summary>
    void UpdateInCoverState()
    {
        float timePassed = Time.time - timeEnteredCover;
        
        // Attendre au moins le temps minimum
        if (timePassed < timeInCover)
        {
            return;
        }
        
        // DÉCISION STRATÉGIQUE : Peut-on partir ?
        if (leaveDecisionMaker != null)
        {
            // Trouver le prochain cluster (EXCLUANT le cluster actuel)
            CoverCluster nextCluster = leaveDecisionMaker.FindNextCluster(squadController, targetCluster);
            
            // Évaluer si on peut partir
            bool canLeave = leaveDecisionMaker.CanLeaveCovers(squadController, nextCluster);
            
            if (canLeave)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[SquadCoverCoordinator] ✅ Conditions remplies → Départ autorisé");
                }
                OrderSquadToResume();
            }
            else
            {
                if (showDebugLogs && Time.frameCount % 120 == 0) // Log toutes les 2 secondes
                {
                    Debug.Log($"[SquadCoverCoordinator] ⏳ Conditions non remplies → Reste en cover");
                }
            }
        }
        else
        {
            // Fallback : timer simple si pas de decision maker
            OrderSquadToResume();
        }
    }
    
    /// <summary>
    /// Ordonne à la squad de reprendre le mouvement
    /// </summary>
    void OrderSquadToResume()
    {
        List<SoldierAgent> soldiers = squadController.GetSoldiers();
        
        foreach (SoldierAgent soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.ReleaseCover();
                soldier.JoinSquadMovement(); // Retour en mouvement normal
            }
        }
        
        currentState = SquadCoverState.Moving;
        targetCluster = null;
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] 🚀 Squad en mouvement");
        }
    }
    
    /// <summary>
    /// Démarre la coordination
    /// </summary>
    public void StartCoordination()
    {
        currentState = SquadCoverState.Moving;
        lastClusterScanTime = Time.time;
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] Démarrage de la coordination squad");
        }
    }
    
    /// <summary>
    /// Obtient l'état actuel pour debug
    /// </summary>
    public string GetCurrentStateString()
    {
        return currentState.ToString();
    }
}