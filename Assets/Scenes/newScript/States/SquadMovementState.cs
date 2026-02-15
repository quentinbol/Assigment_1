using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// État "mouvement en squad" - le soldat suit le chemin A* de sa squad
/// avec flocking et cohésion
/// </summary>
public class SquadMovementState : SoldierState
{
    [Header("Behavior Weights")]
    private float arriveWeight = 2.0f;
    private float separationWeight = 3.0f;
    private float cohesionWeight = 1.5f;
    private float alignmentWeight = 1.0f;
    
    [Header("Distances")]
    private float flockingActivationDistance = 10f; // Active le flocking si on est loin du waypoint
    
    private SquadPathFollower squadPathFollower;
    
    public SquadMovementState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();
        
        // Récupérer le path follower de la squad
        if (soldier.ParentSquad != null)
        {
            squadPathFollower = soldier.ParentSquad.GetComponent<SquadPathFollower>();
        }
        
        // Synchroniser les paramètres avec la squad
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
        
        // Vérifier qu'on a bien un path follower
        if (squadPathFollower == null || !squadPathFollower.IsFollowingPath())
        {
            Debug.LogWarning($"{soldier.name} : SquadPathFollower introuvable ou inactif");
            return;
        }
        
        // 1. ARRIVE vers le waypoint actuel de la squad
        Vector3 currentWaypoint = squadPathFollower.GetCurrentWaypoint();
        Vector3 arriveForce = steering.Arrive(currentWaypoint);
        movement.ApplyForce(arriveForce * arriveWeight);
        
        // 2. SEPARATION (toujours active pour éviter les collisions)
        List<Transform> allNeighbors = steering.FindNeighbors(steering.separationRadius, sameSquadOnly: false);
        if (allNeighbors.Count > 0)
        {
            Vector3 separationForce = steering.Separation(allNeighbors);
            movement.ApplyForce(separationForce * separationWeight);
        }
        
        // 3. FLOCKING (cohésion + alignment) - seulement si on est loin du waypoint
        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint);
        
        if (distanceToWaypoint > flockingActivationDistance)
        {
            List<Transform> squadNeighbors = steering.FindNeighbors(steering.cohesionRadius, sameSquadOnly: true);
            
            if (squadNeighbors.Count > 0)
            {
                // Cohésion : rester groupés
                Vector3 cohesionForce = steering.Cohesion(squadNeighbors);
                movement.ApplyForce(cohesionForce * cohesionWeight);
                
                // Alignment : même direction que les voisins
                Vector3 alignmentForce = steering.Alignment(squadNeighbors);
                movement.ApplyForce(alignmentForce * alignmentWeight);
            }
        }
    }
    
    public override void OnExit()
    {
        base.OnExit();
    }
}