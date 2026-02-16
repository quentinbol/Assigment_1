using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Définit un chemin avec plusieurs waypoints que la squad doit suivre
/// Place ce script sur un GameObject vide et ajoute des waypoints dans l'Inspector
/// </summary>
public class WaypointPath : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Liste des waypoints dans l'ordre (Start → Finish)")]
    public List<Transform> waypoints = new List<Transform>();
    
    [Header("Visual Debug")]
    public bool showPath = true;
    public Color pathColor = Color.yellow;
    public float sphereRadius = 1f;
    
    /// <summary>
    /// Retourne le nombre de waypoints
    /// </summary>
    public int WaypointCount => waypoints.Count;
    
    /// <summary>
    /// Retourne un waypoint à un index donné
    /// </summary>
    public Transform GetWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count)
        {
            return waypoints[index];
        }
        return null;
    }
    
    /// <summary>
    /// Retourne la position d'un waypoint
    /// </summary>
    public Vector3 GetWaypointPosition(int index)
    {
        Transform wp = GetWaypoint(index);
        return wp != null ? wp.position : Vector3.zero;
    }
    
    /// <summary>
    /// Retourne le premier waypoint (départ)
    /// </summary>
    public Vector3 GetStartPosition()
    {
        return GetWaypointPosition(0);
    }
    
    /// <summary>
    /// Retourne le dernier waypoint (arrivée)
    /// </summary>
    public Vector3 GetEndPosition()
    {
        return GetWaypointPosition(waypoints.Count - 1);
    }
    
    /// <summary>
    /// Retourne tous les waypoints sous forme de liste de Vector3
    /// </summary>
    public List<Vector3> GetAllWaypointPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Transform wp in waypoints)
        {
            if (wp != null)
            {
                positions.Add(wp.position);
            }
        }
        return positions;
    }
    
    void OnDrawGizmos()
    {
        if (!showPath || waypoints == null || waypoints.Count == 0) return;
        
        // Dessiner les sphères aux waypoints
        Gizmos.color = pathColor;
        foreach (Transform wp in waypoints)
        {
            if (wp != null)
            {
                Gizmos.DrawWireSphere(wp.position, sphereRadius);
            }
        }
        
        // Dessiner les lignes entre waypoints
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
        
        // Numéroter les waypoints
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(waypoints[i].position + Vector3.up * 2f, $"WP {i}");
#endif
            }
        }
    }
    
    /// <summary>
    /// Valide le chemin (tous les waypoints sont assignés)
    /// </summary>
    public bool IsValid()
    {
        if (waypoints == null || waypoints.Count < 2) return false;
        
        foreach (Transform wp in waypoints)
        {
            if (wp == null) return false;
        }
        
        return true;
    }
}