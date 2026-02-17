using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Movement Parameters")]
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float mass = 1f;
    
    [Header("Debug")]
    public bool showVelocity = false;


    [SerializeField]
    private Vector3 velocity = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;
    
    public Vector3 Velocity => velocity;
    public Vector3 Forward => velocity.magnitude > 0.1f ? velocity.normalized : transform.forward;
    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    public void ResetAcceleration()
    {
        acceleration = Vector3.zero;
    }

    public void Stop()
    {
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }
    
    public void UpdateMovement()
    {
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
        if (velocity.magnitude > 0.1f)
        {
            transform.forward = velocity.normalized;
        }
        ResetAcceleration();
    }

    public bool IsMoving(float threshold = 0.1f)
    {
        return velocity.magnitude > threshold;
    }
    public float GetSpeed()
    {
        return velocity.magnitude;
    }
    
    void OnDrawGizmos()
    {
        if (!showVelocity) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + acceleration);
    }
}