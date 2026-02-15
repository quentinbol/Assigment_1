// SteeringBehavior.cs - Version SANS damping
using UnityEngine;
using System.Collections.Generic;

public class SteeringBehavior : MonoBehaviour
{
    [Header("Movement Parameters")]
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float mass = 1f;
    
    [Header("Arrival Parameters")]
    public float slowingRadius = 3f;
    public float arrivalRadius = 0.5f;
    public float stopRadius = 1f;
    
    [Header("Separation Parameters")]
    public float separationRadius = 1.5f;
    public float cohesionRadius = 5f;
    public float alignmentRadius = 5f;
    public LayerMask soldierLayer;
    
    public Vector3 velocity = Vector3.zero;
    protected Vector3 acceleration = Vector3.zero;
    
    protected virtual void Update()
    {
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        
        transform.position += velocity * Time.deltaTime;

        if (velocity.magnitude > 0.1f)
        {
            transform.forward = velocity.normalized;
        }
        
        acceleration = Vector3.zero;
    }
    
    protected void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    public Vector3 Arrive(Vector3 targetPosition)
    {
        Vector3 desired = targetPosition - transform.position;
        float distance = desired.magnitude;
        desired.y = 0;
        
        if (distance < 0.01f)
        {
            return Vector3.zero;
        }
        
        float speed = maxSpeed;
        
        if (distance < slowingRadius)
        {
            speed = maxSpeed * (distance / slowingRadius);
        }
        
        desired = desired.normalized * speed;
        
        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }
    
    // SEPARATION
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
                diff = diff.normalized / distance;
                steer += diff;
                count++;
            }
        }
        
        if (count > 0)
        {
            steer /= count;
            steer = steer.normalized * maxSpeed;
            steer -= velocity;
            return Vector3.ClampMagnitude(steer, maxForce);
        }
        
        return Vector3.zero;
    }
    
    // COHESION
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
        
        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }
    
    // ALIGNMENT
    public Vector3 Alignment(List<Transform> neighbors)
    {
        if (neighbors.Count == 0) return Vector3.zero;
        
        Vector3 avgVel = Vector3.zero;
        foreach (Transform other in neighbors)
        {
            SteeringBehavior otherSteering = other.GetComponent<SteeringBehavior>();
            if (otherSteering != null)
            {
                avgVel += otherSteering.velocity;
            }
        }
        avgVel /= neighbors.Count;
        
        avgVel = avgVel.normalized * maxSpeed;
        Vector3 steer = avgVel - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }
    
    // FIND NEIGHBORS
    public List<Transform> FindNeighbors(float radius, bool sameSquadOnly = false)
    {
        List<Transform> neighbors = new List<Transform>();
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, radius, soldierLayer);
        
        SoldierAgent thisAgent = GetComponent<SoldierAgent>();
        
        foreach (Collider col in nearbyColliders)
        {
            if (col.transform == transform) continue;
            
            if (sameSquadOnly && thisAgent != null)
            {
                SoldierAgent otherAgent = col.GetComponent<SoldierAgent>();
                if (otherAgent != null && otherAgent.squadID == thisAgent.squadID)
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