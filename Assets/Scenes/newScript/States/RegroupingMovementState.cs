using UnityEngine;
using System.Collections.Generic;

public class RegroupingMovementState : SoldierState
{
    [Header("behavior weights")]
    private float cohesionWeight = 4.0f;
    private float separationWeight = 2.5f;
    private Vector3 squadCenter;
    public RegroupingMovementState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();

        if (soldier.ParentSquad != null)
        {
            cohesionWeight = soldier.ParentSquad.cohesionWeight * 2f;
            separationWeight = soldier.ParentSquad.separationWeight;
        }
        
        Debug.Log($"[{soldier.name}] Entre en RegroupingMovementState");
    }
    
    public override void Execute()
    {
        base.Execute();

        if (soldier.ParentSquad != null)
        {
            squadCenter = soldier.ParentSquad.GetSquadCenter();
        }
        else
        {
            return;
        }
        Vector3 toCenter = squadCenter - transform.position;
        toCenter.y = 0;
        
        float distanceToCenter = toCenter.magnitude;

        if (distanceToCenter > 0.5f)
        {
            Vector3 desiredVelocity = toCenter.normalized * steering.maxSpeed;
            Vector3 steer = desiredVelocity - movement.Velocity;
            steer = Vector3.ClampMagnitude(steer, steering.maxForce);
            
            movement.ApplyForce(steer * cohesionWeight);
            
            if (Time.frameCount % 30 == 0)
            {
                //Debug.Log("regtoupinh");
            }
        }

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
        Debug.Log($"[{soldier.name}] Quitte RegroupingMovementState");
    }
}