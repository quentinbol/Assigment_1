using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CoverCluster
{
    public List<CoverObject> covers;
    public Vector3 centerPosition;
    public int availableCount;
    
    public CoverCluster(List<CoverObject> covers)
    {
        this.covers = covers;
        this.availableCount = covers.Count(c => !c.isOccupied);

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

public class CoverClusterDetector : MonoBehaviour
{
    [Header("cluster detection")]
    public float clusterRadius = 10f;

    public float detectionRadius = 20f;
    
    [Header("cluster limits")]
    public int maxCoversPerCluster = 6;

    public int minCoversPerCluster = 1;
    
    [Header("debug")]
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

    public CoverCluster FindBestClusterForSquad(Vector3 squadPosition, int squadSize)
    {
        CoverObject[] allCovers = FindObjectsByType<CoverObject>(FindObjectsSortMode.None);
        return FindBestClusterForSquad(squadPosition, squadSize, allCovers.ToList());
    }

    public CoverCluster FindBestClusterForSquad(Vector3 squadPosition, int squadSize, List<CoverObject> coversToConsider)
    {
        List<CoverObject> nearbyCovers = coversToConsider
            .Where(c => Vector3.Distance(squadPosition, c.transform.position) <= detectionRadius)
            .ToList();
        
        if (nearbyCovers.Count < squadSize)
        {
            return null;
        }

        List<CoverCluster> clusters = CreateLimitedClusters(nearbyCovers);

        List<CoverCluster> validClusters = clusters
            .Where(c => c.covers.Count >= squadSize)
            .ToList();
        
        if (validClusters.Count == 0)
        {
            return null;
        }

        CoverCluster bestCluster = validClusters
            .OrderBy(c => Vector3.Distance(squadPosition, c.centerPosition))
            .First();
        bestCluster.availableCount = bestCluster.covers.Count(c => !c.isOccupied);
        
        return bestCluster;
    }
    private List<CoverCluster> CreateLimitedClusters(List<CoverObject> covers)
    {
        List<CoverCluster> clusters = new List<CoverCluster>();
        List<CoverObject> unassigned = new List<CoverObject>(covers);
        
        while (unassigned.Count > 0)
        {
            CoverObject seed = unassigned[0];
            unassigned.RemoveAt(0);
            
            List<CoverObject> cluster = new List<CoverObject> { seed };

            bool added = true;
            while (added && cluster.Count < maxCoversPerCluster)
            {
                added = false;
                
                for (int i = unassigned.Count - 1; i >= 0; i--)
                {
                    if (cluster.Count >= maxCoversPerCluster)
                    {
                        break;
                    }
                    
                    CoverObject candidate = unassigned[i];
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
            if (cluster.Count >= minCoversPerCluster)
            {
                clusters.Add(new CoverCluster(cluster));
            }
        }
        
        return clusters;
    }
    public List<CoverCluster> SplitLargeCluster(CoverCluster largeCluster)
    {
        List<CoverCluster> splitClusters = new List<CoverCluster>();
        
        if (largeCluster.covers.Count <= maxCoversPerCluster)
        {
            splitClusters.Add(largeCluster);
            return splitClusters;
        }

        List<CoverObject> sortedCovers = largeCluster.covers.OrderBy(c => c.transform.position.z).ToList();

        for (int i = 0; i < sortedCovers.Count; i += maxCoversPerCluster)
        {
            int count = Mathf.Min(maxCoversPerCluster, sortedCovers.Count - i);
            List<CoverObject> subGroup = sortedCovers.GetRange(i, count);
            
            if (subGroup.Count >= minCoversPerCluster)
            {
                splitClusters.Add(new CoverCluster(subGroup));
            }
        }
        return splitClusters;
    }

    public List<CoverCluster> GetAllClustersInRange(Vector3 position, float range)
    {
        CoverObject[] allCovers = FindObjectsByType<CoverObject>(FindObjectsSortMode.None);
        
        List<CoverObject> nearbyCovers = allCovers
            .Where(c => Vector3.Distance(position, c.transform.position) <= range)
            .ToList();
        
        return CreateLimitedClusters(nearbyCovers);
    }
    
    void OnDrawGizmos()
    {
        if (!visualizeClusters) return;
        CoverObject[] allCovers = FindObjectsByType<CoverObject>(FindObjectsSortMode.None);
        if (allCovers.Length == 0) return;
        
        List<CoverCluster> clusters = CreateLimitedClusters(allCovers.ToList());
        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
        
        for (int i = 0; i < clusters.Count; i++)
        {
            Color clusterColor = colors[i % colors.Length];
            clusterColor.a = 0.5f;
            Gizmos.color = clusterColor;
            
            CoverCluster cluster = clusters[i];
            Gizmos.DrawSphere(cluster.centerPosition, 1f);
            foreach (CoverObject cover in cluster.covers)
            {
                Gizmos.DrawLine(cluster.centerPosition, cover.transform.position);
            }
            Gizmos.DrawWireSphere(cluster.centerPosition, clusterRadius);
        }
    }
}