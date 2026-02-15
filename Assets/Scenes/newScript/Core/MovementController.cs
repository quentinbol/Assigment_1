using UnityEngine;

/// <summary>
/// Responsable UNIQUEMENT de l'application des forces et de la mise à jour de la vélocité
/// Aucune logique de comportement ici - juste de la physique pure
/// </summary>
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
    
    /// <summary>
    /// Applique une force au movement controller
    /// </summary>
    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }
    
    /// <summary>
    /// Réinitialise l'accélération (appelé automatiquement après Update)
    /// </summary>
    public void ResetAcceleration()
    {
        acceleration = Vector3.zero;
    }
    
    /// <summary>
    /// Arrête complètement le mouvement
    /// </summary>
    public void Stop()
    {
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
    }
    
    /// <summary>
    /// Définit la vélocité directement (use avec précaution)
    /// </summary>
    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }
    
    /// <summary>
    /// Update physique - appelé par le state machine
    /// </summary>
    public void UpdateMovement()
    {
        // Intégrer l'accélération
        velocity += acceleration * Time.deltaTime;
        
        // Clamper la vitesse
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        
        // Appliquer la vélocité à la position
        transform.position += velocity * Time.deltaTime;
        
        // Orienter le personnage dans la direction du mouvement
        if (velocity.magnitude > 0.1f)
        {
            transform.forward = velocity.normalized;
        }
        
        // Reset pour la prochaine frame
        ResetAcceleration();
    }
    
    /// <summary>
    /// Vérifie si le personnage bouge
    /// </summary>
    public bool IsMoving(float threshold = 0.1f)
    {
        return velocity.magnitude > threshold;
    }
    
    /// <summary>
    /// Distance parcourue par seconde
    /// </summary>
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