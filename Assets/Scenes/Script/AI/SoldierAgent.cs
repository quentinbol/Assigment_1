// SoldierAgent.cs - Version SIMPLE et PROPRE
using UnityEngine;
using System.Collections.Generic;

public enum SoldierState
{
    Moving,
    InCover
}

public class SoldierAgent : SteeringBehavior
{
    [Header("Squad Reference")]
    public Squad parentSquad;
    public int squadID = 0;
    
    [Header("Soldier State")]
    public SoldierState currentState = SoldierState.Moving;
    public Transform currentTarget;
    
    [Header("Behavior Weights - SIMPLIFIÉ")]
    public float arriveWeight = 2.0f;
    public float separationWeight = 3.0f;
    public float cohesionWeight = 0.5f;
    public float alignmentWeight = 0.3f;
    
    [Header("Flocking Settings")]
    public bool useSquadFlocking = true;
    public float flockingDistanceThreshold = 10f;
    
    [Header("Debug")]
    public bool showDebugGizmos = false;

    public float maxTimeOutOfCover = 10f;
    private float outOfCoverTimer = 0f;
    
    private void Start()
    {
        if (parentSquad == null)
        {
            parentSquad = GetComponentInParent<Squad>();
        }
        
        if (parentSquad != null)
        {
            SyncWithSquad();
        }
    }
    
    protected override void Update()
    {
        switch (currentState)
        {
            case SoldierState.Moving:
                if (currentTarget != null)
                {
                    float distance = Vector3.Distance(transform.position, currentTarget.position);
                    
                    if (distance < arrivalRadius && velocity.magnitude < 0.5f)
                    {
                        EnterCover();
                        return;
                    }

                    List<Transform> allNeighbors = FindNeighbors(separationRadius, false);
                    if (allNeighbors.Count > 0)
                    {
                        Vector3 sep = Separation(allNeighbors) * separationWeight;
                        ApplyForce(sep);
                    }

                    Vector3 arr = Arrive(currentTarget.position) * arriveWeight;
                    ApplyForce(arr);

                    if (useSquadFlocking && distance > flockingDistanceThreshold)
                    {
                        List<Transform> squadNeighbors = FindNeighbors(cohesionRadius, true);
                        
                        if (squadNeighbors.Count > 0)
                        {
                            Vector3 coh = Cohesion(squadNeighbors) * cohesionWeight;
                            Vector3 ali = Alignment(squadNeighbors) * alignmentWeight;
                            
                            ApplyForce(coh);
                            ApplyForce(ali);
                        }
                    }

                    outOfCoverTimer += Time.deltaTime;
                    Debug.Log($"{name} is out of cover for {outOfCoverTimer:F1} seconds");
                    if (outOfCoverTimer > maxTimeOutOfCover)
                    {
                        Debug.LogWarning($"{name} has been out of cover for {outOfCoverTimer:F1} seconds!");
                        //outOfCoverTimer = 0f; // Reset timer after warning
                    }
                }
                break;
                
            case SoldierState.InCover:
                velocity = Vector3.zero;
                acceleration = Vector3.zero;
                outOfCoverTimer = 0f;
                break;
        }
        
        base.Update();
    }
    
    public void SetTarget(Transform target)
    {
        currentTarget = target;
        currentState = SoldierState.Moving;
    }
    
    void EnterCover()
    {
        currentState = SoldierState.InCover;
        if (currentTarget != null)
        {
            CoverObject cover = currentTarget.GetComponent<CoverObject>();
            if (cover != null && !cover.isOccupied)
            {
                cover.SetOccupied(this);
            }
        }
    }

    public void LeaveCover()
    {
        if (currentTarget != null)
        {
            CoverObject cover = currentTarget.GetComponent<CoverObject>();
            if (cover != null && cover.isOccupied && cover.occupyingSoldier == this)
            {
                cover.SetFree();
            }
        }

        currentState = SoldierState.Moving;
    }

    public void outOfCover() {

    }
    
    public void SyncWithSquad()
    {
        if (parentSquad == null) return;
        
        squadID = parentSquad.squadID;
        maxSpeed = parentSquad.maxSpeed;
        maxForce = parentSquad.maxForce;
        mass = parentSquad.mass;
        slowingRadius = parentSquad.slowingRadius;
        arrivalRadius = parentSquad.arrivalRadius;
        separationRadius = parentSquad.separationRadius;
        cohesionRadius = parentSquad.cohesionRadius;
        alignmentRadius = parentSquad.alignmentRadius;
        arriveWeight = parentSquad.arriveWeight;
        separationWeight = parentSquad.separationWeight;
        cohesionWeight = parentSquad.cohesionWeight;
        alignmentWeight = parentSquad.alignmentWeight;
        useSquadFlocking = parentSquad.useSquadFlocking;
        soldierLayer = parentSquad.soldierLayer;
        flockingDistanceThreshold = parentSquad.flockingDistanceThreshold;
        stopRadius = parentSquad.stopRadius;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = (currentState == SoldierState.Moving) ? Color.green : Color.red;

        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.DrawWireSphere(transform.position, stopRadius);

        if (currentTarget != null)
        {
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}