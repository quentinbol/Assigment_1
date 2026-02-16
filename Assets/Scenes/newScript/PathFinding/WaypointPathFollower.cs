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
    public SquadController squadController;
    
    [Header("Settings")]
    public float waypointReachedDistance = 3f;
    
    [Header("Debug")]
    public bool showDebug = true;
    
    private int currentWaypointIndex = 0;
    private bool isFollowingPath = false;

    public bool IsFollowingPath() => isFollowingPath;
    public int CurrentWaypointIndex => currentWaypointIndex;
    
    void Awake()
    {
        if (squadController == null)
        {
            squadController = GetComponent<SquadController>();
        }
    }

    public void StartFollowingPath()
    {
        if (waypointPath == null || !waypointPath.IsValid())
        {
            Debug.LogError("[WaypointPathFollower] Cant waypoint");
            return;
        }
        
        currentWaypointIndex = 0;
        isFollowingPath = true;
    }
    
    void Update()
    {
        if (!isFollowingPath) return;
        
        FollowWaypoints();
    }

    void FollowWaypoints()
    {
        if (currentWaypointIndex >= waypointPath.WaypointCount)
        {
            OnPathCompleted();
            return;
        }

        Vector3 squadPosition = squadController.GetSquadCenter();
    
        Vector3 currentWaypoint = waypointPath.GetWaypointPosition(currentWaypointIndex);
        float distance = Vector3.Distance(squadPosition, currentWaypoint);
        
        /*if (showDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[WaypointPathFollower] WP{currentWaypointIndex} distance: {distance:F1}m");
        }*/
        
        if (distance < waypointReachedDistance)
        {   
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= waypointPath.WaypointCount)
            {
                OnPathCompleted();
            }
        }
    }

    public Vector3 GetCurrentTargetPosition()
    {
        if (waypointPath != null && currentWaypointIndex < waypointPath.WaypointCount)
        {
            return waypointPath.GetWaypointPosition(currentWaypointIndex);
        }
        
        return transform.position;
    }
    void OnPathCompleted()
    {
        isFollowingPath = false;
        
        if (showDebug)
        {
            Debug.Log("[WaypointPathFollower] Chemin terminé !");
        }
    }

    public void StopFollowing()
    {
        isFollowingPath = false;
        currentWaypointIndex = 0;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebug || waypointPath == null || !isFollowingPath) return;

        if (currentWaypointIndex < waypointPath.WaypointCount)
        {
            Vector3 target = waypointPath.GetWaypointPosition(currentWaypointIndex);
            Vector3 squadPos = squadController.GetSquadCenter();
            Gizmos.color = Color.green;
            Gizmos.DrawLine(squadPos, target);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(target, 1f);
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(target, waypointReachedDistance);
        }
    }
}