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
    
    [Header("Cluster Detection")]
    [Tooltip("Fréquence de scan pour détecter des clusters")]
    public float clusterScanInterval = 1f;
    
    [Tooltip("Distance min pour considérer un cluster (évite clusters trop proches)")]
    public float minClusterDistance = 8f;
    
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
        // Scanner périodiquement pour des clusters
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
            
            if (distance > minClusterDistance)
            {
                Debug.Log($"[SquadCoverCoordinator] Cluster détecté à {Vector3.Distance(squadPosition, cluster.centerPosition):F1}m " +
                      $"avec {cluster.availableCount} covers disponibles");
                // CLUSTER TROUVÉ ! Ordonner à la squad d'y aller
                OrderSquadToCluster(cluster);
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
        
        for (int i = 0; i < soldiers.Count && i < availableCovers.Count; i++)
        {
            soldiers[i].AssignCover(availableCovers[i].transform);
            soldiers[i].SeekNearbyCover(); // Transition vers le cover assigné
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] 📍 {soldiers.Count} soldats assignés aux covers");
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
    /// État : Squad en cover, attend avant de repartir
    /// </summary>
    void UpdateInCoverState()
    {
        float timePassed = Time.time - timeEnteredCover;
        
        if (timePassed >= timeInCover)
        {
            // Temps écoulé, repartir !
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator] ⏰ Temps écoulé → Reprise du mouvement");
            }
            
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
                soldier.JoinSquadMovement();
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