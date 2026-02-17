using UnityEngine;
using System.Collections.Generic;

public class GoToAssignedCoverState : SoldierState
{
    [Header("behavior weights")]
    private float arriveWeight = 2.0f;
    private float separationWeight = 3.0f;
    
    [Header("cover settings")]
    private float arrivalThreshold = 1.0f;
    private CoverObject targetCover;
    public GoToAssignedCoverState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();

        targetCover = soldier.CurrentCover;
        
        if (targetCover == null)
        {
            Debug.LogError($"{soldier.name} : no cover");
            soldier.StateMachine.TransitionTo<IdleState>();
            return;
        }
        if (!targetCover.isOccupied)
        {
            targetCover.SetOccupied(soldier);
        }
        
        //Debug.Log($"{soldier.name} : goto covre");

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
            soldier.StateMachine.TransitionTo<IdleState>();
            return;
        }
        float distance = Vector3.Distance(transform.position, targetCover.transform.position);
        if (distance < arrivalThreshold && movement.GetSpeed() < 0.5f)
        {
            //Debug.Log($"{soldier.name} : is in cover");
            soldier.StateMachine.TransitionTo<InCoverState>();
            return;
        }

        Vector3 arriveForce = steering.Arrive(targetCover.transform.position);
        movement.ApplyForce(arriveForce * arriveWeight);

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