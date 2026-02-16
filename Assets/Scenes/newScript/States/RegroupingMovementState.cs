using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// État spécial pour le regroupement - Force les soldats à converger vers le centre de la squad
/// Utilisé quand les soldats quittent leurs covers et doivent se regrouper
/// </summary>
public class RegroupingMovementState : SoldierState
{
    [Header("Behavior Weights")]
    private float cohesionWeight = 4.0f;      // Force FORTE vers le centre
    private float separationWeight = 2.5f;     // Évite les collisions
    
    private Vector3 squadCenter;
    
    public RegroupingMovementState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();
        
        // Synchroniser les poids avec la squad si disponible
        if (soldier.ParentSquad != null)
        {
            cohesionWeight = soldier.ParentSquad.cohesionWeight * 2f; // Double la force
            separationWeight = soldier.ParentSquad.separationWeight;
        }
        
        Debug.Log($"[{soldier.name}] Entre en RegroupingMovementState");
    }
    
    public override void Execute()
    {
        base.Execute();
        
        // Mettre à jour le centre de la squad
        if (soldier.ParentSquad != null)
        {
            squadCenter = soldier.ParentSquad.GetSquadCenter();
        }
        else
        {
            return;
        }
        
        // 1. FORCE PRINCIPALE : Aller vers le centre de la squad
        Vector3 toCenter = squadCenter - transform.position;
        toCenter.y = 0; // Garder sur le plan horizontal
        
        float distanceToCenter = toCenter.magnitude;
        
        // Seulement si on est loin du centre
        if (distanceToCenter > 0.5f)
        {
            // Créer une force vers le centre
            Vector3 desiredVelocity = toCenter.normalized * steering.maxSpeed;
            Vector3 steer = desiredVelocity - movement.Velocity;
            steer = Vector3.ClampMagnitude(steer, steering.maxForce);
            
            movement.ApplyForce(steer * cohesionWeight);
            
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"[{soldier.name}] Regrouping: distance au centre={distanceToCenter:F1}m, force={steer.magnitude:F2}");
            }
        }
        
        // 2. SEPARATION : Éviter les collisions avec les autres soldats
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