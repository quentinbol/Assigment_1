using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Version SIMPLIFIÉE - Va directement de waypoint en waypoint
/// Pas d'A* entre les waypoints, juste arrive directement
/// </summary>
public class WaypointPathFollower : MonoBehaviour
{
    [Header("References")]
    public WaypointPath waypointPath;
    public SquadController squadController; // NOUVEAU : pour obtenir le centre de la squad
    
    [Header("Settings")]
    public float waypointReachedDistance = 3f;
    
    [Header("Debug")]
    public bool showDebug = true;
    
    private int currentWaypointIndex = 0;
    private bool isFollowingPath = false;
    
    // Propriétés publiques
    public bool IsFollowingPath() => isFollowingPath;
    public int CurrentWaypointIndex => currentWaypointIndex;
    
    void Awake()
    {
        if (squadController == null)
        {
            squadController = GetComponent<SquadController>();
        }
    }
    
    /// <summary>
    /// Démarre le suivi du chemin de waypoints
    /// </summary>
    public void StartFollowingPath()
    {
        if (waypointPath == null || !waypointPath.IsValid())
        {
            Debug.LogError("[WaypointPathFollower] WaypointPath invalide !");
            return;
        }
        
        currentWaypointIndex = 0;
        isFollowingPath = true;
        
        if (showDebug)
        {
            Debug.Log($"[WaypointPathFollower] Démarrage ! {waypointPath.WaypointCount} waypoints");
        }
    }
    
    void Update()
    {
        if (!isFollowingPath) return;
        
        FollowWaypoints();
    }
    
    /// <summary>
    /// Suit les waypoints séquentiellement
    /// </summary>
    void FollowWaypoints()
    {
        if (currentWaypointIndex >= waypointPath.WaypointCount)
        {
            OnPathCompleted();
            return;
        }
        
        // CORRECTION : Utiliser le centre de la squad au lieu de transform.position
        Vector3 squadPosition = squadController != null ? squadController.GetSquadCenter() : transform.position;
        
        // Vérifier la distance au waypoint actuel
        Vector3 currentWaypoint = waypointPath.GetWaypointPosition(currentWaypointIndex);
        float distance = Vector3.Distance(squadPosition, currentWaypoint);
        
        if (showDebug && Time.frameCount % 60 == 0) // Log toutes les 60 frames
        {
            Debug.Log($"[WaypointPathFollower] WP{currentWaypointIndex} distance: {distance:F1}m (seuil: {waypointReachedDistance}m)");
        }
        
        if (distance < waypointReachedDistance)
        {
            // Waypoint atteint !
            if (showDebug)
            {
                Debug.Log($"[WaypointPathFollower] ✅ Waypoint {currentWaypointIndex} atteint !");
            }
            
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= waypointPath.WaypointCount)
            {
                OnPathCompleted();
            }
            else if (showDebug)
            {
                Debug.Log($"[WaypointPathFollower] → Prochain waypoint: {currentWaypointIndex}");
            }
        }
    }
    
    /// <summary>
    /// Retourne la position du waypoint actuel
    /// </summary>
    public Vector3 GetCurrentTargetPosition()
    {
        if (waypointPath != null && currentWaypointIndex < waypointPath.WaypointCount)
        {
            return waypointPath.GetWaypointPosition(currentWaypointIndex);
        }
        
        return transform.position;
    }
    
    /// <summary>
    /// Appelé quand tout le chemin est terminé
    /// </summary>
    void OnPathCompleted()
    {
        isFollowingPath = false;
        
        if (showDebug)
        {
            Debug.Log("[WaypointPathFollower] Chemin terminé !");
        }
    }
    
    /// <summary>
    /// Arrête le suivi
    /// </summary>
    public void StopFollowing()
    {
        isFollowingPath = false;
        currentWaypointIndex = 0;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebug || waypointPath == null || !isFollowingPath) return;
        
        // Dessiner une ligne vers le waypoint actuel
        if (currentWaypointIndex < waypointPath.WaypointCount)
        {
            Vector3 target = waypointPath.GetWaypointPosition(currentWaypointIndex);
            Vector3 squadPos = squadController != null ? squadController.GetSquadCenter() : transform.position;
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(squadPos, target);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(target, 1f);
            
            // Debug : afficher le rayon de détection
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(target, waypointReachedDistance);
        }
    }
}