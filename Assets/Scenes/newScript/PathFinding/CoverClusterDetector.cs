using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Représente un cluster (groupe) de covers proches
/// Permet de détecter des groupes de covers pouvant accueillir une squad
/// </summary>
public class CoverCluster
{
    public List<CoverObject> covers;
    public Vector3 centerPosition;
    public int availableCount; // Nombre de covers libres
    
    public CoverCluster(List<CoverObject> covers)
    {
        this.covers = covers;
        this.availableCount = covers.Count(c => !c.isOccupied);
        
        // Calculer le centre du cluster
        if (covers.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (var cover in covers)
            {
                sum += cover.transform.position;
            }
            centerPosition = sum / covers.Count;
        }
    }
}

/// <summary>
/// Détecte et gère les clusters de covers pour les squads
/// </summary>
public class CoverClusterDetector : MonoBehaviour
{
    [Header("Cluster Detection")]
    [Tooltip("Distance max entre deux covers pour être dans le même cluster")]
    public float clusterRadius = 10f;
    
    [Tooltip("Distance max pour détecter des clusters depuis la squad")]
    public float detectionRadius = 20f;
    
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    public bool visualizeClusters = true;
    
    private static CoverClusterDetector instance;
    public static CoverClusterDetector Instance => instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Trouve le meilleur cluster de covers pour une squad
    /// </summary>
    public CoverCluster FindBestClusterForSquad(Vector3 squadPosition, int squadSize)
    {
        // Récupérer tous les covers
        CoverObject[] allCovers = FindObjectsByType<CoverObject>(FindObjectsSortMode.None);
        return FindBestClusterForSquad(squadPosition, squadSize, allCovers.ToList());
    }
    
    /// <summary>
    /// Trouve le meilleur cluster parmi une liste de covers spécifique
    /// </summary>
    public CoverCluster FindBestClusterForSquad(Vector3 squadPosition, int squadSize, List<CoverObject> coversToConsider)
    {
        // Filtrer les covers dans le rayon de détection et non occupés
        List<CoverObject> nearbyCovers = coversToConsider
            .Where(c => Vector3.Distance(squadPosition, c.transform.position) <= detectionRadius)
            .ToList();
        
        if (nearbyCovers.Count < squadSize)
        {
            // Pas assez de covers disponibles
            return null;
        }
        
        // Créer des clusters
        List<CoverCluster> clusters = CreateClusters(nearbyCovers);
        
        // Filtrer les clusters qui peuvent accueillir toute la squad
        List<CoverCluster> validClusters = clusters
            .Where(c => c.covers.Count >= squadSize) // Au moins assez de covers au total
            .ToList();
        
        if (validClusters.Count == 0)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[CoverCluster] Aucun cluster trouvé pour {squadSize} soldats (covers: {nearbyCovers.Count})");
            }
            return null;
        }
        
        // Trouver le cluster le plus proche
        CoverCluster bestCluster = validClusters
            .OrderBy(c => Vector3.Distance(squadPosition, c.centerPosition))
            .First();
        
        // Recalculer availableCount sur les covers non occupés
        bestCluster.availableCount = bestCluster.covers.Count(c => !c.isOccupied);
        
        if (showDebugLogs)
        {
            Debug.Log($"[CoverCluster] Cluster trouvé : {bestCluster.covers.Count} covers, " +
                      $"{bestCluster.availableCount} disponibles, " +
                      $"distance: {Vector3.Distance(squadPosition, bestCluster.centerPosition):F1}m");
        }
        
        return bestCluster;
    }
    
    /// <summary>
    /// Crée des clusters à partir d'une liste de covers
    /// Utilise un algorithme de clustering par distance
    /// </summary>
    private List<CoverCluster> CreateClusters(List<CoverObject> covers)
    {
        List<CoverCluster> clusters = new List<CoverCluster>();
        List<CoverObject> unassigned = new List<CoverObject>(covers);
        
        while (unassigned.Count > 0)
        {
            // Commencer un nouveau cluster avec le premier cover
            CoverObject seed = unassigned[0];
            unassigned.RemoveAt(0);
            
            List<CoverObject> cluster = new List<CoverObject> { seed };
            
            // Ajouter tous les covers proches
            bool added = true;
            while (added)
            {
                added = false;
                
                for (int i = unassigned.Count - 1; i >= 0; i--)
                {
                    CoverObject candidate = unassigned[i];
                    
                    // Vérifier si proche d'un cover du cluster
                    foreach (CoverObject inCluster in cluster)
                    {
                        float distance = Vector3.Distance(candidate.transform.position, inCluster.transform.position);
                        
                        if (distance <= clusterRadius)
                        {
                            cluster.Add(candidate);
                            unassigned.RemoveAt(i);
                            added = true;
                            break;
                        }
                    }
                }
            }
            
            clusters.Add(new CoverCluster(cluster));
        }
        
        return clusters;
    }
    
    void OnDrawGizmos()
    {
        if (!visualizeClusters) return;
        
        // Visualiser tous les clusters
        CoverObject[] allCovers = FindObjectsByType<CoverObject>(FindObjectsSortMode.None);
        if (allCovers.Length == 0) return;
        
        List<CoverCluster> clusters = CreateClusters(allCovers.ToList());
        
        // Couleurs différentes pour chaque cluster
        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
        
        for (int i = 0; i < clusters.Count; i++)
        {
            Color clusterColor = colors[i % colors.Length];
            clusterColor.a = 0.5f;
            Gizmos.color = clusterColor;
            
            CoverCluster cluster = clusters[i];
            
            // Dessiner une sphère au centre
            Gizmos.DrawSphere(cluster.centerPosition, 1f);
            
            // Dessiner des lignes vers chaque cover du cluster
            foreach (CoverObject cover in cluster.covers)
            {
                Gizmos.DrawLine(cluster.centerPosition, cover.transform.position);
            }
            
            // Dessiner le rayon du cluster
            Gizmos.DrawWireSphere(cluster.centerPosition, clusterRadius);
        }
    }
}