using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SquadCoverCoordinator : MonoBehaviour
{
    public SquadController squadController;

    public float minClusterDistance = 5f;

    public float timeInCover = 3f;

    public float clusterApproachDistance = 15f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private enum SquadCoverState
    {
        Moving,
        GoingToCover,
        InCover,
    }
    
    private SquadCoverState currentState = SquadCoverState.Moving;
    private float timeEnteredCover = 0f;
    private CoverCluster targetCluster = null;
    private CoverCluster lastCluster = null;
    
    void Start()
    {
        currentState = SquadCoverState.Moving;
        
        if (showDebugLogs)
        {
            //Debug.Log($"Start");
        }
    }
    
    void Update()
    {
        int aliveCount = squadController.GetAliveCount();
        if (aliveCount == 0)
        {
            return;
        }
        
        switch (currentState)
        {
            case SquadCoverState.Moving:
                UpdateMovingState();
                break;
                
            case SquadCoverState.GoingToCover:
                UpdateGoingToCoverState();
                break;
                
            case SquadCoverState.InCover:
                UpdateInCoverState();
                break;
        }
    }

    void UpdateMovingState()
    {
        if (CoverClusterDetector.Instance == null)
        {
            Debug.LogWarning("[SquadCoverCoordinator] Pas de CoverClusterDetector !");
            return;
        }
        
        Vector3 squadPosition = squadController.GetSquadCenter();
        int squadSize = squadController.GetAliveCount();

        CoverCluster cluster = CoverClusterDetector.Instance.FindBestClusterForSquad(
            squadPosition, 
            squadSize
        );
        
        if (cluster != null)
        {
            float distance = Vector3.Distance(squadPosition, cluster.centerPosition);
            bool isSameAsLast = false;
            if (lastCluster != null)
            {
                float distToLast = Vector3.Distance(cluster.centerPosition, lastCluster.centerPosition);
                if (distToLast < minClusterDistance)
                {
                    isSameAsLast = true;
                }
            }
            
            if (!isSameAsLast && distance > 2f)
            {
                GoToCluster(cluster);
            }
        }
    }

    void GoToCluster(CoverCluster cluster)
    {
        targetCluster = cluster;
        currentState = SquadCoverState.GoingToCover;

        float distance = Vector3.Distance(squadController.GetSquadCenter(), cluster.centerPosition);
        
        if (distance <= clusterApproachDistance)
        {
            DisperseToCover(cluster);
        }
    }

    void DisperseToCover(CoverCluster cluster)
    {
        List<SoldierAgent> soldiers = squadController.GetAliveSoldiers();
        List<CoverObject> availableCovers = cluster.covers.Where(c => !c.isOccupied).ToList();
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] Assigne {soldiers.Count} soldats VIVANTS à {availableCovers.Count} covers");
        }
        
        for (int i = 0; i < soldiers.Count && i < availableCovers.Count; i++)
        {
            if (soldiers[i] == null || availableCovers[i] == null)
            {
                continue;
            }
            
            soldiers[i].AssignCover(availableCovers[i].transform);
            soldiers[i].GoToAssignedCover();
            
            if (showDebugLogs)
            {
                Debug.Log($"[SquadCoverCoordinator]   {soldiers[i].name} → {availableCovers[i].name}");
            }
        }
    }

    void UpdateGoingToCoverState()
    {
        if (targetCluster == null)
        {
            currentState = SquadCoverState.Moving;
            return;
        }

        Vector3 squadCenter = squadController.GetSquadCenter();
        float distanceToCluster = Vector3.Distance(squadCenter, targetCluster.centerPosition);

        if (distanceToCluster <= clusterApproachDistance)
        {
            List<SoldierAgent> soldiers = squadController.GetAliveSoldiers();
            bool alreadyDispered = false;
            
            foreach (SoldierAgent soldier in soldiers)
            {
                if (soldier != null && soldier.AssignedCoverTransform != null)
                {
                    alreadyDispered = true;
                    break;
                }
            }
            
            if (!alreadyDispered)
            {
                DisperseToCover(targetCluster);
            }
        }

        if (squadController.IsSquadInCover())
        {   
            currentState = SquadCoverState.InCover;
            timeEnteredCover = Time.time;
            lastCluster = targetCluster;
        }
    }

    void UpdateInCoverState()
    {
        float timePassed = Time.time - timeEnteredCover;

        if (timePassed >= timeInCover)
        {   
            LeaveCover();
        }
    }

    void LeaveCover()
    {
        List<SoldierAgent> soldiers = squadController.GetAliveSoldiers();

        foreach (SoldierAgent soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.ReleaseCover();
            }
        }
        foreach (SoldierAgent soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.JoinSquadMovement();
            }
        }
        targetCluster = null;
        currentState = SquadCoverState.Moving;
    }

    public void StartCoordination()
    {
        currentState = SquadCoverState.Moving;
        
        if (showDebugLogs)
        {
            Debug.Log($"[SquadCoverCoordinator] Démarrage de la coordination (mode SIMPLE)");
        }
    }

    public string GetCurrentStateString()
    {
        return currentState.ToString();
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugLogs || squadController == null) return;
        
        Vector3 squadCenter = squadController.GetSquadCenter();

        if (targetCluster != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(squadCenter, targetCluster.centerPosition);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetCluster.centerPosition, 1f);

            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(targetCluster.centerPosition, clusterApproachDistance);
        }
    }
}