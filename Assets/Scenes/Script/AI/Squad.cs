/*using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour
{
    [Header("Squad Identity")]
    public string squadName = "Alpha Squad";
    public int squadID = 0;
    public Color squadColor = Color.blue;
    
    [Header("Movement Parameters")]
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float mass = 1f;
    public float dampingFactor = 0.9f;
    
    [Header("Arrival Parameters")]
    public float slowingRadius = 3f;
    public float arrivalRadius = 0.5f;
    public float stopRadius = 0.3f;
    
    [Header("Flocking Parameters")]
    public float separationRadius = 1.5f;
    public float cohesionRadius = 5f;
    public float alignmentRadius = 5f;
    
    [Header("Behavior Weights")]
    public float arriveWeight = 1.0f;
    public float separationWeight = 2.5f;
    public float cohesionWeight = 1.0f;
    public float alignmentWeight = 1.0f;
    public float obstacleAvoidanceWeight = 3.0f;

    [Header("Obstacle Avoidance")]
    public float obstacleAvoidanceDistance = 3f;
    public LayerMask obstacleLayer;
        
    [Header("Squad Settings")]
    public bool useSquadFlocking = true;
    public LayerMask soldierLayer;
    
    [Header("Soldiers (Auto-populated from children)")]
    public List<SoldierAgent> soldiers = new List<SoldierAgent>();
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public float flockingDistanceThreshold;

    Vector3 currentWaypoint;

    private void Awake()
    {
        InitializeSquad();
    }
    
    private void OnValidate()
    {
        InitializeSquad();
        ApplyParametersToSoldiers();
    }

    public void InitializeSquad()
    {
        soldiers.Clear();

        SoldierAgent[] childSoldiers = GetComponentsInChildren<SoldierAgent>();
        
        foreach (var soldier in childSoldiers)
        {
            soldiers.Add(soldier);
        }
        
        Debug.Log($"{squadName}: Found {soldiers.Count} soldiers");
    }

    public void ApplyParametersToSoldiers()
    {
        foreach (var soldier in soldiers)
        {
            if (soldier == null) continue;

            soldier.squadID = squadID;

            soldier.maxSpeed = maxSpeed;
            soldier.maxForce = maxForce;
            soldier.mass = mass;

            soldier.slowingRadius = slowingRadius;
            soldier.arrivalRadius = arrivalRadius;
            soldier.stopRadius = stopRadius;

            soldier.separationRadius = separationRadius;
            soldier.cohesionRadius = cohesionRadius;
            soldier.alignmentRadius = alignmentRadius;

            soldier.arriveWeight = arriveWeight;
            soldier.separationWeight = separationWeight;
            soldier.cohesionWeight = cohesionWeight;
            soldier.alignmentWeight = alignmentWeight;

            soldier.useSquadFlocking = useSquadFlocking;
            soldier.soldierLayer = soldierLayer;
            soldier.flockingDistanceThreshold = flockingDistanceThreshold;

            ApplyColorToSoldier(soldier);
        }
    }

    void ApplyColorToSoldier(SoldierAgent soldier)
    {
        Renderer renderer = soldier.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (Application.isPlaying)
            {
                renderer.material.color = squadColor;
            }
            else
            {
                // En mode édition, utiliser sharedMaterial
                renderer.sharedMaterial = new Material(renderer.sharedMaterial);
                renderer.sharedMaterial.color = squadColor;
            }
        }
    }

    public void SetCurrentWaypoint(Vector3 waypoint)
    {
        currentWaypoint = waypoint;
    }

    public void SetSoldierWaypoints(Vector3 waypoint)
    {
        foreach (var soldier in soldiers)
        {
            if (soldier != null && soldier.currentState == SoldierState.Moving)
            {
                soldier.SetCurrentWaypoint(waypoint);
            }
        }
    }

    public void SendToCover(List<CoverObject> coverTargets, int startIndex = 0)
    {
        if (coverTargets == null || coverTargets.Count == 0) return;
        List<Transform> targetPositions = new List<Transform>();
        foreach (var cover in coverTargets)
        {
            if (cover.transform != null)
            {
                targetPositions.Add(cover.transform);
            }
        }

        for (int i = 0; i < soldiers.Count; i++)
        {
            if (soldiers[i] == null) continue;
            
            int coverIndex = (startIndex + i) % coverTargets.Count;
            var cover = coverTargets[coverIndex];
            Transform target = cover.transform;
            soldiers[i].LeaveCover();
            Debug.Log($"{squadName} soldier {i} moving to cover at {target.position}");
            if (target != null)
            {
                int attempts = 0;
                while (cover.isOccupied && attempts < coverTargets.Count)
                {
                    coverIndex = (coverIndex + 1) % coverTargets.Count;
                    cover = coverTargets[coverIndex];
                    target = cover.transform;
                    attempts++;
                }
                if (!cover.isOccupied)
                {
                    cover.SetOccupied(soldiers[i]);
                    soldiers[i].SetTarget(target);
                }
            }
        }
        
        //Debug.Log($"{squadName} moving to covers starting at index {startIndex}");
    }

    public bool IsSquadInCover()
    {
        foreach (var soldier in soldiers)
        {
            if (soldier != null && soldier.currentState == SoldierState.Moving)
            {
                return false;
            }
        }
        return true;
    }
    
    // Compter les soldats survivants
    public int GetAliveCount()
    {
        int count = 0;
        foreach (var soldier in soldiers)
        {
            if (soldier != null && soldier.gameObject.activeSelf)
            {
                count++;
            }
        }
        return count;
    }
    public void LeaveAllCovers()
    {
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.LeaveCover();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || soldiers.Count == 0) return;

        Vector3 center = GetSquadCenter();
        
        Gizmos.color = squadColor;
        Gizmos.DrawWireSphere(center, cohesionRadius);

        Gizmos.color = new Color(squadColor.r, squadColor.g, squadColor.b, 0.3f);
        for (int i = 0; i < soldiers.Count - 1; i++)
        {
            if (soldiers[i] != null && soldiers[i + 1] != null)
            {
                Gizmos.DrawLine(soldiers[i].transform.position, soldiers[i + 1].transform.position);
            }
        }
    }
    
    // Obtenir le centre de l'escouade
    public Vector3 GetSquadCenter()
    {
        if (soldiers.Count == 0) return transform.position;
        
        Vector3 center = Vector3.zero;
        int count = 0;
        
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
            {
                center += soldier.transform.position;
                count++;
            }
        }
        
        return count > 0 ? center / count : transform.position;
    }
}*/