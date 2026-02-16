using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// État "mouvement individuel" - le soldat se dirige vers son cover assigné
/// Utilisé après que le squad ait atteint le waypoint et que les covers soient assignées
/// </summary>
public class IndividualMovementState : SoldierState
{
    [Header("Behavior Weights")]
    private float arriveWeight = 2.0f;
    private float separationWeight = 3.0f;
    
    [Header("Cover Settings")]
    private float arrivalThreshold = 1.0f; // Distance pour considérer qu'on est arrivé
    
    private Transform targetCover;
    
    public IndividualMovementState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();
        
        // Récupérer le cover assigné
        targetCover = soldier.AssignedCoverTransform;
        
        if (targetCover == null)
        {
            Debug.LogWarning($"{soldier.name} : Pas de cover assigné pour IndividualMovementState");
        }
        
        // Synchroniser les poids avec la squad si disponible
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

        if (targetCover == null)
        {
            Debug.LogWarning($"{soldier.name} : Pas de target cover - retour à Idle");
            soldier.StateMachine.TransitionTo<IdleState>();
            return;
        }

        float distanceToCover = Vector3.Distance(transform.position, targetCover.position);
        if (distanceToCover < arrivalThreshold && movement.GetSpeed() < 0.5f)
        {
            soldier.StateMachine.TransitionTo<InCoverState>();
            return;
        }

        Vector3 arriveForce = steering.Arrive(targetCover.position);
        movement.ApplyForce(arriveForce * arriveWeight);

        List<Transform> allNeighbors = steering.FindNeighbors(steering.separationRadius, sameSquadOnly: false);
        if (allNeighbors.Count > 0)
        {
            Vector3 separationForce = steering.Separation(allNeighbors);
            movement.ApplyForce(separationForce * separationWeight);
        }
    }
    
    public override void OnExit()
    {
        base.OnExit();
    }
}