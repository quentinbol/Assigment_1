using Unity.VisualScripting;
using UnityEngine;

public class SoldierAgent : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private Squad parentSquad;
    [SerializeField] private int squadID;

    [SerializeField] private string soldierColor;
    
    [Header("cover")]
    [SerializeField] private Transform assignedCoverTransform;
    [SerializeField] private CoverObject currentCover;
    
    [Header("components")]
    private SoldierStateMachine stateMachine;
    private MovementController movement;
    private SteeringBehaviors steering;

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

    private MaterialPropertyBlock _mpb;

    void OnValidate()
    {
        if (parentSquad == null)
            return;

        soldierColor = parentSquad.squadColor.ToString();

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer == null)
            return;

        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Color", parentSquad.squadColor);
        renderer.SetPropertyBlock(_mpb);
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
            ForceUpdateColor();
        }
    }

    public void ForceUpdateColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && parentSquad != null)
        {
            Material uniqueMaterial = new Material(renderer.sharedMaterial);
            uniqueMaterial.color = parentSquad.squadColor;
            renderer.material = uniqueMaterial;
        }
    }
    
    public void SyncWithSquad()
    {
        if (parentSquad == null) 
            return;

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

    public void GoToAssignedCover()
    {
        if (stateMachine != null)
        {
            stateMachine.TransitionTo<GoToAssignedCoverState>();
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

    public bool IsMoving()
    {
        return movement != null && movement.IsMoving();
    }
    
    public bool IsInCover()
    {
        return stateMachine != null && stateMachine.IsInState<InCoverState>();
    }

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

    public void SeekNearbyCover()
    {
        if (stateMachine != null)
        {
            stateMachine.TransitionTo<IndividualMovementState>();
        }
    }
    public void TakeCover()
    {
        if (stateMachine != null)
        {
            stateMachine.TransitionTo<InCoverState>();
        }
    }
    public void StopAndIdle()
    {
        if (stateMachine != null)
        {
            stateMachine.TransitionTo<IdleState>();
        }
    }
}