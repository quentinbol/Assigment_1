using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour
{
    [Header("squad identity")]
    public string squadName = "Alpha";
    public int squadID = 0;
    public Color squadColor = Color.blue;
    
    [Header("Soldiers")]
    public List<SoldierAgent> soldiers = new List<SoldierAgent>();
    
    [Header("movement parameters")]
    public float maxSpeed = 6f;
    public float maxForce = 10f;
    public float mass = 1f;
    
    [Header("steering distances")]
    public float slowingRadius = 7f;
    public float arrivalRadius = 1f;
    public float stopRadius = 0.3f;
    
    [Header("flocking distances")]
    public float separationRadius = 2f;
    public float cohesionRadius = 10f;
    public float alignmentRadius = 10f;
    
    [Header("behavior weights")]
    public float arriveWeight = 2f;
    public float separationWeight = 3f;
    public float cohesionWeight = 1.5f;
    public float alignmentWeight = 1f;
    [Header("flocking settings")]
    //stop follow if treshold hit 
    public float flockingDistanceThreshold = 10f;
    [Header("detection")]
    public LayerMask soldierLayer;
    
    [Header("visual debug")]
    public bool showDebug = false;

    public Vector3 GetSquadCenter()
    {
        if (soldiers.Count == 0) return transform.position;
        
        Vector3 center = Vector3.zero;
        int count = 0;
        
        foreach (var soldier in soldiers)
        {
            if (soldier != null && soldier.gameObject.activeSelf)
            {
                center += soldier.transform.position;
                count++;
            }
        }
        
        return count > 0 ? center / count : transform.position;
    }

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

    public List<SoldierAgent> GetSoldierAgents()
    {
        List<SoldierAgent> agents = new List<SoldierAgent>();
        
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
            {
                SoldierAgent agent = soldier.GetComponent<SoldierAgent>();
                if (agent != null)
                {
                    agents.Add(agent);
                }
            }
        }
        
        return agents;
    }
    

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetSquadCenter(), 1f);

        if (soldiers.Count > 1)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            for (int i = 0; i < soldiers.Count - 1; i++)
            {
                if (soldiers[i] != null && soldiers[i + 1] != null)
                {
                    Gizmos.DrawLine(soldiers[i].transform.position, soldiers[i + 1].transform.position);
                }
            }
        }
    }
}