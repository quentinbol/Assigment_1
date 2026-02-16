using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// État : Va vers un cover DÉJÀ ASSIGNÉ (par SquadCoverCoordinator)
/// Ne cherche PAS de cover, utilise celui déjà assigné
/// </summary>
public class GoToAssignedCoverState : SoldierState
{
    [Header("Behavior Weights")]
    private float arriveWeight = 2.0f;
    private float separationWeight = 3.0f;
    
    [Header("Settings")]
    private float arrivalThreshold = 1.0f;
    
    private CoverObject targetCover;
    
    public GoToAssignedCoverState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();
        
        // Récupérer le cover DÉJÀ ASSIGNÉ
        targetCover = soldier.CurrentCover;
        
        if (targetCover == null)
        {
            Debug.LogError($"{soldier.name} : Aucun cover assigné ! Retour à Idle");
            soldier.StateMachine.TransitionTo<IdleState>();
            return;
        }
        
        // Réserver le cover
        if (!targetCover.isOccupied)
        {
            targetCover.SetOccupied(soldier);
        }
        
        Debug.Log($"{soldier.name} : Va vers cover assigné : {targetCover.name}");
        
        // Sync weights
        if (soldier.ParentSquad != null)
        {
            arriveWeight = soldier.ParentSquad.arriveWeight;
            separationWeight = soldier.ParentSquad.separationWeight;
            arrivalThreshold = soldier.ParentSquad.arrivalRadius;
        }
    }
    
    public override void Execute()
    {
        base.Execute();
        
        // Vérifier qu'on a toujours un cover valide
        if (targetCover == null)
        {
            soldier.StateMachine.TransitionTo<IdleState>();
            return;
        }
        
        // Vérifier si arrivé
        float distance = Vector3.Distance(transform.position, targetCover.transform.position);
        if (distance < arrivalThreshold && movement.GetSpeed() < 0.5f)
        {
            // Arrivé au cover !
            Debug.Log($"{soldier.name} : Arrivé au cover → InCoverState");
            soldier.StateMachine.TransitionTo<InCoverState>();
            return;
        }
        
        // 1. ARRIVE vers le cover (avec ralentissement)
        Vector3 arriveForce = steering.Arrive(targetCover.transform.position);
        movement.ApplyForce(arriveForce * arriveWeight);
        
        // 2. SEPARATION pour éviter collisions
        List<Transform> neighbors = steering.FindNeighbors(steering.separationRadius, sameSquadOnly: false);
        if (neighbors.Count > 0)
        {
            Vector3 separationForce = steering.Separation(neighbors);
            movement.ApplyForce(separationForce * separationWeight);
        }
    }
    
    public override void OnExit()
    {
        base.OnExit();
    }
}