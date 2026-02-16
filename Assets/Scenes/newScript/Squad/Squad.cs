using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Composant Squad - Contient toutes les données de configuration d'une squad
/// Pas de logique de mouvement ici - juste des données
/// </summary>
public class Squad : MonoBehaviour
{
    [Header("Squad Identity")]
    public string squadName = "Alpha";
    public int squadID = 0;
    public Color squadColor = Color.blue;
    
    [Header("Soldiers")]
    public List<SoldierAgent> soldiers = new List<SoldierAgent>();
    
    [Header("Movement Parameters")]
    public float maxSpeed = 6f;
    public float maxForce = 10f;
    public float mass = 1f;
    
    [Header("Steering Distances")]
    public float slowingRadius = 7f;
    public float arrivalRadius = 1f;
    public float stopRadius = 0.3f;
    
    [Header("Flocking Distances")]
    public float separationRadius = 2f;
    public float cohesionRadius = 10f;
    public float alignmentRadius = 10f;
    
    [Header("Behavior Weights")]
    public float arriveWeight = 2f;
    public float separationWeight = 3f;
    public float cohesionWeight = 1.5f;
    public float alignmentWeight = 1f;
    
    [Header("Flocking Settings")]
    public float flockingDistanceThreshold = 10f; // Distance pour activer le flocking
    
    [Header("Detection")]
    public LayerMask soldierLayer;
    
    [Header("Visual Debug")]
    public bool showDebug = false;
    
    void Start()
    {
        // Vérifier que tous les soldats sont bien assignés
        if (soldiers.Count == 0)
        {
            Debug.LogWarning($"Squad {squadName} : Aucun soldat assigné !");
        }
        else
        {
            Debug.Log($"Squad {squadName} créée avec {soldiers.Count} soldats");
        }
    }
    
    /// <summary>
    /// Obtient le centre de la squad (position moyenne)
    /// </summary>
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
    
    /// <summary>
    /// Compte le nombre de soldats vivants/actifs
    /// </summary>
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
    
    /// <summary>
    /// Obtient tous les SoldierAgent de la squad
    /// </summary>
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
        
        // Dessiner le centre de la squad
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetSquadCenter(), 1f);
        
        // Dessiner les connexions entre soldats
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