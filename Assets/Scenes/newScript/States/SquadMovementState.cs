using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// État "mouvement en squad" - compatible avec WaypointPathFollower
/// Le soldat suit le chemin de waypoints avec flocking
/// </summary>
public class SquadMovementState : SoldierState
{
    [Header("Behavior Weights")]
    private float arriveWeight = 2.0f;
    private float separationWeight = 3.0f;
    private float cohesionWeight = 1.5f;
    private float alignmentWeight = 1.0f;
    
    [Header("Distances")]
    private float flockingActivationDistance = 10f;
    
    private WaypointPathFollower waypointPathFollower;
    private SquadPathFollower squadPathFollower; // Backup si pas de waypoints
    
    public SquadMovementState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();
        
        // Essayer de récupérer WaypointPathFollower en priorité
        if (soldier.ParentSquad != null)
        {
            waypointPathFollower = soldier.ParentSquad.GetComponent<WaypointPathFollower>();
            
            // Backup : ancien système
            if (waypointPathFollower == null)
            {
                squadPathFollower = soldier.ParentSquad.GetComponent<SquadPathFollower>();
            }
        }
        
        // Synchroniser les poids
        if (soldier.ParentSquad != null)
        {
            arriveWeight = soldier.ParentSquad.arriveWeight;
            separationWeight = soldier.ParentSquad.separationWeight;
            cohesionWeight = soldier.ParentSquad.cohesionWeight;
            alignmentWeight = soldier.ParentSquad.alignmentWeight;
            flockingActivationDistance = soldier.ParentSquad.flockingDistanceThreshold;
        }
    }
    
    public override void Execute()
    {
        base.Execute();
        
        Vector3 targetPosition;
        
        // Déterminer la position cible selon le système utilisé
        if (waypointPathFollower != null && waypointPathFollower.IsFollowingPath())
        {
            targetPosition = waypointPathFollower.GetCurrentTargetPosition();
        }
        else if (squadPathFollower != null && squadPathFollower.IsFollowingPath())
        {
            targetPosition = squadPathFollower.GetCurrentWaypoint();
        }
        else
        {
            // Aucun path actif
            return;
        }
        
        // 1. SEEK vers la position cible (pas de ralentissement pour les waypoints)
        Vector3 seekForce = steering.Seek(targetPosition);
        movement.ApplyForce(seekForce * arriveWeight);
        
        // 2. SEPARATION (toujours active)
        List<Transform> allNeighbors = steering.FindNeighbors(steering.separationRadius, sameSquadOnly: false);
        if (allNeighbors.Count > 0)
        {
            Vector3 separationForce = steering.Separation(allNeighbors);
            movement.ApplyForce(separationForce * separationWeight);
        }
        
        // 3. FLOCKING (si loin de la cible)
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (distanceToTarget > flockingActivationDistance)
        {
            List<Transform> squadNeighbors = steering.FindNeighbors(steering.cohesionRadius, sameSquadOnly: true);
            
            if (squadNeighbors.Count > 0)
            {
                Vector3 cohesionForce = steering.Cohesion(squadNeighbors);
                movement.ApplyForce(cohesionForce * cohesionWeight);
                
                Vector3 alignmentForce = steering.Alignment(squadNeighbors);
                movement.ApplyForce(alignmentForce * alignmentWeight);
            }
        }
    }
}