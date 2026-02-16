using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Composant qui fournit des calculs de steering behaviors PURS
/// NE gère PAS le mouvement lui-même - retourne juste des forces
/// </summary>
public class SteeringBehaviors : MonoBehaviour
{
    [Header("Arrival Parameters")]
    public float slowingRadius = 3f;
    public float arrivalRadius = 0.5f;
    public float stopRadius = 0.3f;
    
    [Header("Flocking Parameters")]
    public float separationRadius = 1.5f;
    public float cohesionRadius = 5f;
    public float alignmentRadius = 5f;
    
    [Header("Force Limits")]
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    
    [Header("Detection")]
    public LayerMask soldierLayer;
    
    private MovementController movement;
    private SoldierAgent soldier;
    
    void Awake()
    {
        movement = GetComponent<MovementController>();
        soldier = GetComponent<SoldierAgent>();
    }
    
    /// <summary>
    /// Calcule la force d'arrivée vers une position
    /// </summary>
    public Vector3 Arrive(Vector3 targetPosition)
    {
        Vector3 desired = targetPosition - transform.position;
        float distance = desired.magnitude;
        desired.y = 0; // Garder sur le plan horizontal
        
        if (distance < 0.01f)
        {
            return Vector3.zero;
        }
        
        float speed = maxSpeed;
        
        // Ralentir dans le rayon de ralentissement
        if (distance < slowingRadius)
        {
            speed = maxSpeed * (distance / slowingRadius);
        }
        
        desired = desired.normalized * speed;
        
        Vector3 steer = desired - movement.Velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    /// <summary>
    /// Calcule la force de seek vers une position (SANS ralentissement)
    /// Utilisé pour suivre les waypoints à vitesse constante
    /// </summary>
    public Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 desired = targetPosition - transform.position;
        desired.y = 0; // Garder sur le plan horizontal
        
        if (desired.magnitude < 0.01f)
        {
            return Vector3.zero;
        }
        
        // Vitesse constante, pas de ralentissement
        desired = desired.normalized * maxSpeed;
        
        Vector3 steer = desired - movement.Velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }
    
    /// <summary>
    /// Calcule la force de séparation par rapport aux voisins
    /// </summary>
    public Vector3 Separation(List<Transform> neighbors)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;
        
        foreach (Transform other in neighbors)
        {
            float distance = Vector3.Distance(transform.position, other.position);
            
            if (distance > 0 && distance < separationRadius)
            {
                Vector3 diff = transform.position - other.position;
                diff.y = 0;
                diff = diff.normalized / distance; // Plus fort quand plus proche
                steer += diff;
                count++;
            }
        }
        
        if (count > 0)
        {
            steer /= count;
            steer = steer.normalized * maxSpeed;
            steer -= movement.Velocity;
            return Vector3.ClampMagnitude(steer, maxForce);
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Calcule la force de cohésion (aller vers le centre du groupe)
    /// </summary>
    public Vector3 Cohesion(List<Transform> neighbors)
    {
        if (neighbors.Count == 0) return Vector3.zero;
        
        Vector3 center = Vector3.zero;
        foreach (Transform other in neighbors)
        {
            center += other.position;
        }
        center /= neighbors.Count;
        
        Vector3 desired = center - transform.position;
        desired.y = 0;
        desired = desired.normalized * maxSpeed;
        
        Vector3 steer = desired - movement.Velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }
    
    /// <summary>
    /// Calcule la force d'alignement (même direction que les voisins)
    /// </summary>
    public Vector3 Alignment(List<Transform> neighbors)
    {
        if (neighbors.Count == 0) return Vector3.zero;
        
        Vector3 avgVel = Vector3.zero;
        foreach (Transform other in neighbors)
        {
            MovementController otherMovement = other.GetComponent<MovementController>();
            if (otherMovement != null)
            {
                avgVel += otherMovement.Velocity;
            }
        }
        avgVel /= neighbors.Count;
        
        avgVel = avgVel.normalized * maxSpeed;
        Vector3 steer = avgVel - movement.Velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }
    
    /// <summary>
    /// Trouve les voisins dans un rayon donné
    /// </summary>
    public List<Transform> FindNeighbors(float radius, bool sameSquadOnly = false)
    {
        List<Transform> neighbors = new List<Transform>();
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, radius, soldierLayer);
        
        foreach (Collider col in nearbyColliders)
        {
            if (col.transform == transform) continue;
            
            if (sameSquadOnly && soldier != null)
            {
                SoldierAgent otherAgent = col.GetComponent<SoldierAgent>();
                if (otherAgent != null /*&& otherAgent.SquadID == soldier.SquadID*/)
                {
                    neighbors.Add(col.transform);
                }
            }
            else
            {
                neighbors.Add(col.transform);
            }
        }
        
        return neighbors;
    }
}