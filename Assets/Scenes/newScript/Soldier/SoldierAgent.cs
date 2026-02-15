using UnityEngine;

/// <summary>
/// SoldierAgent SIMPLE avec recherche de cover proche
/// </summary>
public class SoldierAgent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Squad parentSquad;
    [SerializeField] private int squadID;
    
    [Header("Cover")]
    [SerializeField] private Transform assignedCoverTransform;
    [SerializeField] private CoverObject currentCover;
    
    [Header("Components")]
    private SoldierStateMachine stateMachine;
    private MovementController movement;
    private SteeringBehaviors steering;
    
    // Propriétés publiques
    public Squad ParentSquad => parentSquad;
    public int SquadID => squadID;
    public Transform AssignedCoverTransform => assignedCoverTransform;
    public CoverObject CurrentCover => currentCover;
    public SoldierStateMachine StateMachine => stateMachine;
    public MovementController Movement => movement;
    public SteeringBehaviors Steering => steering;
    
    void Awake()
    {
        stateMachine = GetComponent<SoldierStateMachine>();
        movement = GetComponent<MovementController>();
        steering = GetComponent<SteeringBehaviors>();
        
        if (stateMachine == null) stateMachine = gameObject.AddComponent<SoldierStateMachine>();
        if (movement == null) movement = gameObject.AddComponent<MovementController>();
        if (steering == null) steering = gameObject.AddComponent<SteeringBehaviors>();
    }
    
    void Start()
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
    
    public void SyncWithSquad()
    {
        if (parentSquad == null) return;
        
        squadID = parentSquad.squadID;
        
        if (movement != null)
        {
            movement.maxSpeed = parentSquad.maxSpeed;
            movement.maxForce = parentSquad.maxForce;
            movement.mass = parentSquad.mass;
        }
        
        if (steering != null)
        {
            steering.slowingRadius = parentSquad.slowingRadius;
            steering.arrivalRadius = parentSquad.arrivalRadius;
            steering.stopRadius = parentSquad.stopRadius;
            steering.separationRadius = parentSquad.separationRadius;
            steering.cohesionRadius = parentSquad.cohesionRadius;
            steering.alignmentRadius = parentSquad.alignmentRadius;
            steering.maxSpeed = parentSquad.maxSpeed;
            steering.maxForce = parentSquad.maxForce;
            steering.soldierLayer = parentSquad.soldierLayer;
        }
    }
    
    public void AssignCover(Transform cover)
    {
        assignedCoverTransform = cover;
        
        if (cover != null)
        {
            currentCover = cover.GetComponent<CoverObject>();
        }
    }
    
    public void ReleaseCover()
    {
        if (currentCover != null && currentCover.isOccupied)
        {
            currentCover.SetFree();
        }
        
        assignedCoverTransform = null;
        currentCover = null;
    }
    
    // === QUERIES ===
    
    public bool IsMoving()
    {
        return movement != null && movement.IsMoving();
    }
    
    public bool IsInCover()
    {
        return stateMachine != null && stateMachine.IsInState<InCoverState>();
    }
    
    // === COMMANDES ===
    
    /// <summary>
    /// Rejoindre le mouvement de squad
    /// </summary>
    public void JoinSquadMovement()
    {
        Debug.Log($"[{name}] JoinSquadMovement appelé");
        
        if (stateMachine == null)
        {
            Debug.LogError($"[{name}] StateMachine est NULL !");
            return;
        }
        
        Debug.Log($"[{name}] Appel de TransitionTo<SquadMovementState>");
        stateMachine.TransitionTo<SquadMovementState>();
        Debug.Log($"[{name}] Transition terminée");
    }
    
    /// <summary>
    /// Chercher un cover proche (SIMPLE - juste le plus proche)
    /// </summary>
    public void SeekNearbyCover()
    {
        if (stateMachine != null)
        {
            //stateMachine.TransitionTo<SeekNearbyCoverState>();
        }
    }
    
    /// <summary>
    /// Se mettre en cover
    /// </summary>
    public void TakeCover()
    {
        if (stateMachine != null)
        {
            stateMachine.TransitionTo<InCoverState>();
        }
    }
    
    /// <summary>
    /// Arrêter et rester idle
    /// </summary>
    public void StopAndIdle()
    {
        if (stateMachine != null)
        {
            stateMachine.TransitionTo<IdleState>();
        }
    }
}