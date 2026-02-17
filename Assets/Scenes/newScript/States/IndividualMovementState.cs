using UnityEngine;
using System.Collections.Generic;

public class IndividualMovementState : SoldierState
{
    [Header("behavior weights")]
    private float arriveWeight = 2.0f;
    private float separationWeight = 3.0f;
    
    [Header("cover settings")]
    private float arrivalThreshold = 1.0f;
    
    private Transform targetCover;
    
    public IndividualMovementState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();
        targetCover = soldier.AssignedCoverTransform;
        
        if (targetCover == null)
        {
            Debug.LogWarning("no cover");
        }

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
            Debug.LogWarning("no coverr");
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