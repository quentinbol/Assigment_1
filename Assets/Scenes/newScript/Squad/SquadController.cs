using UnityEngine;
using System.Collections.Generic;

public class SquadController : MonoBehaviour
{
    [Header("References")]
    public Squad squad;
    
    [Header("Path Follower (choisir UN seul)")]
    public WaypointPathFollower waypointPathFollower;
    //public SquadPathFollower squadPathFollower;
    
    [Header("Auto Cover")]
    public bool autoSeekCoverOnArrival = true;
    public float searchRadius = 15f;
    public float delayBeforeSeekCover = 0.5f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    [Header("Soldiers (Debug)")]
    [SerializeField] private List<SoldierAgent> soldiers = new List<SoldierAgent>();
    private bool hasReachedDestination = false;
    
    void Awake()
    {
        if (squad == null) squad = GetComponent<Squad>();

        if (waypointPathFollower == null)
            waypointPathFollower = GetComponent<WaypointPathFollower>();
        
        /*if (squadPathFollower == null)
            squadPathFollower = GetComponent<SquadPathFollower>();*/
    }
    
    void Start()
    {
        RefreshSoldierList();
    }
    
    void Update()
    {
        if (autoSeekCoverOnArrival && !hasReachedDestination)
        {
            bool pathComplete = false;

            if (waypointPathFollower != null)
            {
                pathComplete = !waypointPathFollower.IsFollowingPath();
            }
           /* else if (squadPathFollower != null)
            {
                pathComplete = !squadPathFollower.IsFollowingPath();
            }*/
            
            if (pathComplete)
            {
                bool wasMoving = false;
                foreach (var soldier in soldiers)
                {
                    if (soldier != null && soldier.StateMachine != null && 
                        soldier.StateMachine.IsInState<SquadMovementState>())
                    {
                        wasMoving = true;
                        break;
                    }
                }
                
                if (wasMoving)
                {
                    hasReachedDestination = true;
                    Invoke(nameof(OrderSquadToSeekNearbyCover), delayBeforeSeekCover);
                }
            }
        }
    }
    
    public void RefreshSoldierList()
    {
        soldiers.Clear();
        
        if (squad != null && squad.soldiers != null)
        {
            foreach (var soldierObj in squad.soldiers)
            {
                if (soldierObj != null)
                {
                    SoldierAgent agent = null;

                    if (soldierObj is Component)
                    {
                        agent = (soldierObj as Component).GetComponent<SoldierAgent>();
                    }
                    
                    if (agent != null)
                    {
                        soldiers.Add(agent);
                    }
                }
            }
        }
        
        if (soldiers.Count == 0)
        {
            SoldierAgent[] childSoldiers = GetComponentsInChildren<SoldierAgent>();
            soldiers.AddRange(childSoldiers);
        }
    }

    public void StartMovement()
    {
        hasReachedDestination = false;

        if (waypointPathFollower != null)
        {
            waypointPathFollower.StartFollowingPath();
        }
        else {
            return;
        }

        int transitionCount = 0;
        foreach (var soldier in GetAliveSoldiers())
        {
            if (soldier != null)
            {
                soldier.JoinSquadMovement();
                transitionCount++;
            }
        }

        SquadCoverCoordinator coordinator = GetComponent<SquadCoverCoordinator>();
        if (coordinator != null)
        {
            coordinator.StartCoordination();
        }
    }

    public void MoveSquadToDestination(Vector3 destination)
    {
        if (waypointPathFollower != null)
        {
            StartMovement();
        }
       /* else if (squadPathFollower != null)
        {
            squadPathFollower.MoveSquadToDestination(destination);
            
            foreach (var soldier in GetAliveSoldiers())
            {
                if (soldier != null)
                {
                    soldier.JoinSquadMovement();
                }
            }
        }*/
    }
    
    public void OrderSquadToSeekNearbyCover()
    {
        
        foreach (var soldier in GetAliveSoldiers())
        {
            if (soldier != null)
            {
                soldier.SeekNearbyCover();
            }
        }
    }
    
    public void StopSquad()
    {
        hasReachedDestination = false;
        
        if (waypointPathFollower != null)
        {
            waypointPathFollower.StopFollowing();
        }
        
        /*if (squadPathFollower != null)
        {
            squadPathFollower.StopFollowing();
        }*/
        
        foreach (var soldier in GetAliveSoldiers())
        {
            if (soldier != null)
            {
                soldier.StopAndIdle();
            }
        }
    }
    
    public void OrderSquadToLeaveCover()
    {
        foreach (var soldier in GetAliveSoldiers())
        {
            if (soldier != null)
            {
                soldier.ReleaseCover();
                soldier.StopAndIdle();
            }
        }
    }

    public bool IsSquadInCover()
    {
        int aliveCount = 0;
        int inCoverCount = 0;
        
        foreach (var soldier in soldiers)
        {
            if (soldier == null || !soldier.gameObject.activeSelf)
                continue;

            ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
            if (timer != null && timer.IsDead())
                continue;
            
            aliveCount++;
            
            if (soldier.IsInCover())
                inCoverCount++;
        }

        if (aliveCount == 0)
            return false;
        return inCoverCount >= aliveCount;
    }
    public bool IsSquadMoving()
    {
        foreach (var soldier in soldiers)
        {
            if (soldier == null || !soldier.gameObject.activeSelf)
                continue;

            ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
            if (timer != null && timer.IsDead())
                continue;
            
            if (soldier.IsMoving())
                return true;
        }
        return false;
    }
    public int GetAliveCount()
    {
        int count = 0;
        foreach (var soldier in soldiers)
        {
            if (soldier == null || !soldier.gameObject.activeSelf)
                continue;

            ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
            if (timer != null && timer.IsDead())
                continue;
            
            count++;
        }
        return count;
    }

    public int GetInCoverCount()
    {
        int count = 0;
        foreach (var soldier in soldiers)
        {
            if (soldier == null || !soldier.gameObject.activeSelf)
                continue;

            ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
            if (timer != null && timer.IsDead())
                continue;
            
            if (soldier.IsInCover())
                count++;
        }
        return count;
    }

    public Vector3 GetSquadCenter()
    {
        if (soldiers.Count == 0) return transform.position;
        
        Vector3 center = Vector3.zero;
        int count = 0;
        
        foreach (var soldier in soldiers)
        {
            if (soldier == null || !soldier.gameObject.activeSelf)
                continue;

            ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
            if (timer != null && timer.IsDead())
                continue;
            
            center += soldier.transform.position;
            count++;
        }
        
        return count > 0 ? center / count : transform.position;
    }

    public List<SoldierAgent> GetSoldiers()
    {
        return soldiers;
    }

    public List<SoldierAgent> GetAliveSoldiers()
    {
        List<SoldierAgent> aliveSoldiers = new List<SoldierAgent>();
        
        foreach (var soldier in soldiers)
        {
            if (soldier == null || !soldier.gameObject.activeSelf)
                continue;
            
            ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
            if (timer != null && timer.IsDead())
                continue;
            
            aliveSoldiers.Add(soldier);
        }
        
        return aliveSoldiers;
    }
}