using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// VERSION SIMPLE : Timer basique, pas de conditions compliquées
/// - Détecte cluster → va au cluster → en cover X secondes → sort → recommence
/// </summary>
public class SquadCoverCoordinator : MonoBehaviour
{
    [Header("References")]
    public SquadController squadController;
    
    [Header("Cluster Detection")]
    [Tooltip("Distance min pour considérer un cluster différent")]
    public float minClusterDistance = 5f;
    
    [Header("Cover Timing - SIMPLE")]
    [Tooltip("Temps en cover avant de repartir (secondes)")]
    public float timeInCover = 3f;
    
    [Header("Cluster Approach")]
    [Tooltip("Distance pour se disperser vers les covers")]
    public float clusterApproachDistance = 15f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private enum SquadCoverState
    {
        Moving,           // Cherche un cluster
        GoingToCover,     // Va vers le cluster
        InCover,          // En cover, attend le timer
    }
    
    private SquadCoverState currentState = SquadCoverState.Moving;
    private float timeEnteredCover = 0f;
    private CoverCluster targetCluster = null;
    private CoverCluster lastCluster = null; // Pour éviter de revenir au même
    
    void Start()
    {
        currentState = SquadCoverState.Moving;
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] Démarrage - Mode SIMPLE");
        }
    }
    
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
    /// État Moving : Cherche un cluster disponible
    /// </summary>
    void UpdateMovingState()
    {
        if (CoverClusterDetector.Instance == null)
        {
            Debug.LogWarning("[SquadCoverCoordinator] Pas de CoverClusterDetector !");
            return;
        }
        
        Vector3 squadPosition = squadController.GetSquadCenter();
        int squadSize = squadController.GetAliveCount();
        
        // Chercher un cluster
        CoverCluster cluster = CoverClusterDetector.Instance.FindBestClusterForSquad(
            squadPosition, 
            squadSize
        );
        
        if (cluster != null)
        {
            float distance = Vector3.Distance(squadPosition, cluster.centerPosition);
            
            // Vérifier que ce n'est pas le dernier cluster utilisé
            bool isSameAsLast = false;
            if (lastCluster != null)
            {
                float distToLast = Vector3.Distance(cluster.centerPosition, lastCluster.centerPosition);
                if (distToLast < minClusterDistance)
                {
                    isSameAsLast = true;
                }
            }
            
            if (!isSameAsLast && distance > 2f)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[SquadCoverCoordinator] 🎯 Cluster trouvé à {distance:F1}m avec {cluster.covers.Count} covers");
                }
                
                GoToCluster(cluster);
            }
            else if (showDebugLogs && Time.frameCount % 120 == 0)
            {
                Debug.Log($"[SquadCoverCoordinator] ⏳ Cherche un nouveau cluster (évite le dernier utilisé)...");
            }
        }
        else if (showDebugLogs && Time.frameCount % 120 == 0)
        {
            Debug.Log($"[SquadCoverCoordinator] 🔍 Cherche un cluster...");
        }
    }
    
    /// <summary>
    /// Ordonner à la squad d'aller vers un cluster
    /// </summary>
    void GoToCluster(CoverCluster cluster)
    {
        targetCluster = cluster;
        currentState = SquadCoverState.GoingToCover;
        
        // Calculer distance pour savoir si on doit disperser maintenant ou plus tard
        float distance = Vector3.Distance(squadController.GetSquadCenter(), cluster.centerPosition);
        
        if (distance <= clusterApproachDistance)
        {
            // Assez proche → disperser immédiatement
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator] 💥 Assez proche ({distance:F1}m) → Dispersion immédiate");
            }
            DisperseToCover(cluster);
        }
        else
        {
            // Trop loin → rester groupé pour l'instant
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator] 🚶 Trop loin ({distance:F1}m) → Reste groupé, dispersion à {clusterApproachDistance}m");
            }
        }
    }
    
    /// <summary>
    /// Disperser les soldats vers leurs covers
    /// </summary>
    void DisperseToCover(CoverCluster cluster)
    {
        List<SoldierAgent> soldiers = squadController.GetSoldiers();
        List<CoverObject> availableCovers = cluster.covers.Where(c => !c.isOccupied).ToList();
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] Assigne {soldiers.Count} soldats à {availableCovers.Count} covers");
        }
        
        for (int i = 0; i < soldiers.Count && i < availableCovers.Count; i++)
        {
            if (soldiers[i] == null || availableCovers[i] == null)
            {
                continue;
            }
            
            soldiers[i].AssignCover(availableCovers[i].transform);
            soldiers[i].GoToAssignedCover();
            
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator]   {soldiers[i].name} → {availableCovers[i].name}");
            }
        }
    }
    
    /// <summary>
    /// État GoingToCover : Surveille la distance et disperse si nécessaire
    /// </summary>
    void UpdateGoingToCoverState()
    {
        if (targetCluster == null)
        {
            currentState = SquadCoverState.Moving;
            return;
        }
        
        // Vérifier la distance au cluster
        Vector3 squadCenter = squadController.GetSquadCenter();
        float distanceToCluster = Vector3.Distance(squadCenter, targetCluster.centerPosition);
        
        // Log périodique
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[SquadCoverCoordinator] 📍 Distance: {distanceToCluster:F1}m (seuil: {clusterApproachDistance}m)");
        }
        
        // Disperser quand assez proche
        if (distanceToCluster <= clusterApproachDistance)
        {
            // Vérifier si déjà dispersés
            List<SoldierAgent> soldiers = squadController.GetSoldiers();
            bool alreadyDispered = false;
            
            foreach (SoldierAgent soldier in soldiers)
            {
                if (soldier != null && soldier.AssignedCoverTransform != null)
                {
                    alreadyDispered = true;
                    break;
                }
            }
            
            if (!alreadyDispered)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[SquadCoverCoordinator] 💥 Distance atteinte → Dispersion !");
                }
                DisperseToCover(targetCluster);
            }
        }
        
        // Vérifier si tous en cover
        if (squadController.IsSquadInCover())
        {
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator] ✅ Tous en cover ! Timer : {timeInCover}s");
            }
            
            currentState = SquadCoverState.InCover;
            timeEnteredCover = Time.time;
            lastCluster = targetCluster; // Mémoriser ce cluster
        }
    }
    
    /// <summary>
    /// État InCover : Attend le timer puis repart
    /// </summary>
    void UpdateInCoverState()
    {
        float timePassed = Time.time - timeEnteredCover;
        
        // Log périodique
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[SquadCoverCoordinator] ⏰ En cover: {timePassed:F1}s / {timeInCover}s");
        }
        
        // Attendre le timer
        if (timePassed >= timeInCover)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator] ⏱️ Timer écoulé → Soldats partent !");
            }
            
            LeaveCover();
        }
    }
    
    /// <summary>
    /// Quitter les covers et chercher le prochain cluster
    /// </summary>
    void LeaveCover()
    {
        List<SoldierAgent> soldiers = squadController.GetSoldiers();
        
        // Libérer les covers
        foreach (SoldierAgent soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.ReleaseCover();
            }
        }
        
        // Mettre en SquadMovementState (suit waypoints)
        foreach (SoldierAgent soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.JoinSquadMovement();
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] 🚀 Soldats libérés, cherche prochain cluster");
        }
        
        // Retour à Moving
        targetCluster = null;
        currentState = SquadCoverState.Moving;
    }
    
    /// <summary>
    /// Démarre la coordination
    /// </summary>
    public void StartCoordination()
    {
        currentState = SquadCoverState.Moving;
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] Démarrage de la coordination (mode SIMPLE)");
        }
    }
    
    /// <summary>
    /// État actuel pour debug
    /// </summary>
    public string GetCurrentStateString()
    {
        return currentState.ToString();
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugLogs || squadController == null) return;
        
        Vector3 squadCenter = squadController.GetSquadCenter();
        
        // Visualiser le cluster ciblé
        if (targetCluster != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(squadCenter, targetCluster.centerPosition);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetCluster.centerPosition, 1f);
            
            // Zone de dispersion
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(targetCluster.centerPosition, clusterApproachDistance);
        }
    }
}