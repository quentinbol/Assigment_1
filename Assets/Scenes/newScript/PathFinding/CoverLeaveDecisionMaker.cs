using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Prend la décision de quitter un cover en fonction de conditions stratégiques
/// - Disponibilité du prochain cluster
/// - Temps d'exposition estimé
/// - Cohésion de la squad
/// </summary>
public class CoverLeaveDecisionMaker : MonoBehaviour
{
    [Header("Exposure Settings")]
    [Tooltip("Temps max d'exposition avant mort (secondes)")]
    public float maxExposureTime = 15f;
    
    [Tooltip("Marge de sécurité (doit arriver avec X secondes restantes)")]
    public float safetyMargin = 3f;
    
    [Header("Cohesion Settings")]
    [Tooltip("Distance max acceptable entre soldats pour être cohésif")]
    public float maxSquadSpread = 8f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    /// <summary>
    /// Décide si la squad peut quitter les covers actuels
    /// </summary>
    public bool CanLeaveCovers(SquadController squadController, CoverCluster nextCluster)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[CoverLeaveDecision] === ÉVALUATION DE DÉPART ===");
        }
        
        // CONDITION 1 : Le prochain cluster a assez de covers libres
        bool hasEnoughCovers = CheckNextClusterAvailability(squadController, nextCluster);
        
        // CONDITION 2 : Le temps d'exposition est acceptable
        bool exposureAcceptable = CheckExposureTime(squadController, nextCluster);
        
        // CONDITION 3 : La squad est cohésive
        bool squadCohesive = CheckSquadCohesion(squadController);
        
        // DÉCISION FINALE
        bool canLeave = hasEnoughCovers && exposureAcceptable && squadCohesive;
        
        if (showDebugLogs)
        {
            Debug.Log($"[CoverLeaveDecision] Résultat:");
            Debug.Log($"  ✓ Covers disponibles: {hasEnoughCovers}");
            Debug.Log($"  ✓ Exposition acceptable: {exposureAcceptable}");
            Debug.Log($"  ✓ Squad cohésive: {squadCohesive}");
            Debug.Log($"[CoverLeaveDecision] → DÉCISION: {(canLeave ? "✅ PARTIR" : "❌ RESTER")}");
        }
        
        return canLeave;
    }
    
    /// <summary>
    /// CONDITION 1 : Vérifie que le prochain cluster a au moins 6 covers libres
    /// </summary>
    private bool CheckNextClusterAvailability(SquadController squadController, CoverCluster nextCluster)
    {
        if (nextCluster == null)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[CoverLeaveDecision] ❌ Aucun prochain cluster détecté");
            }
            return false;
        }
        
        int squadSize = squadController.GetAliveCount();
        int availableCovers = nextCluster.availableCount;
        
        bool hasEnough = availableCovers >= squadSize;
        
        if (showDebugLogs)
        {
            Debug.Log($"[CoverLeaveDecision] Cluster trouvé à {Vector3.Distance(squadController.GetSquadCenter(), nextCluster.centerPosition):F1}m");
            Debug.Log($"[CoverLeaveDecision] Total covers dans cluster: {nextCluster.covers.Count}");
            Debug.Log($"[CoverLeaveDecision] Covers LIBRES: {availableCovers}");
            Debug.Log($"[CoverLeaveDecision] Covers requis: {squadSize}");
            Debug.Log($"[CoverLeaveDecision] → {(hasEnough ? "✅ ASSEZ" : "❌ INSUFFISANT")}");
            
            // Debug détaillé des covers
            for (int i = 0; i < nextCluster.covers.Count; i++)
            {
                CoverObject cover = nextCluster.covers[i];
                Debug.Log($"  Cover {i}: {cover.name} - {(cover.isOccupied ? "OCCUPÉ" : "LIBRE")}");
            }
        }
        
        return hasEnough;
    }
    
    /// <summary>
    /// CONDITION 2 : Vérifie que le temps d'exposition pour atteindre le prochain cluster est acceptable
    /// Temps d'exposition = Distance / Vitesse
    /// Doit être < (maxExposureTime - safetyMargin)
    /// </summary>
    private bool CheckExposureTime(SquadController squadController, CoverCluster nextCluster)
    {
        if (nextCluster == null)
        {
            return false;
        }
        
        // Calculer la distance au prochain cluster
        Vector3 currentPosition = squadController.GetSquadCenter();
        float distance = Vector3.Distance(currentPosition, nextCluster.centerPosition);
        
        // Estimer le temps de trajet (distance / vitesse moyenne)
        float averageSpeed = CalculateAverageSquadSpeed(squadController);
        float estimatedTravelTime = distance / averageSpeed;
        
        // Temps d'exposition acceptable = maxExposureTime - safetyMargin
        float maxAcceptableTime = maxExposureTime - safetyMargin;
        
        bool isAcceptable = estimatedTravelTime <= maxAcceptableTime;
        
        if (showDebugLogs)
        {
            Debug.Log($"[CoverLeaveDecision] Distance: {distance:F1}m, Vitesse: {averageSpeed:F1}m/s");
            Debug.Log($"[CoverLeaveDecision] Temps estimé: {estimatedTravelTime:F1}s, Max acceptable: {maxAcceptableTime:F1}s → {(isAcceptable ? "OK" : "TROP LONG")}");
        }
        
        return isAcceptable;
    }
    
    /// <summary>
    /// CONDITION 3 : Vérifie que la squad est cohésive (soldats proches les uns des autres)
    /// </summary>
    private bool CheckSquadCohesion(SquadController squadController)
    {
        List<SoldierAgent> soldiers = squadController.GetSoldiers();
        
        if (soldiers.Count <= 1)
        {
            return true; // Un seul soldat = toujours cohésif
        }
        
        Vector3 center = squadController.GetSquadCenter();
        float maxDistance = 0f;
        
        // Trouver le soldat le plus éloigné du centre
        foreach (SoldierAgent soldier in soldiers)
        {
            if (soldier == null) continue;
            
            float distance = Vector3.Distance(soldier.transform.position, center);
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }
        
        bool isCohesive = maxDistance <= maxSquadSpread;
        
        if (showDebugLogs)
        {
            Debug.Log($"[CoverLeaveDecision] Dispersion max: {maxDistance:F1}m, Limite: {maxSquadSpread:F1}m → {(isCohesive ? "COHÉSIF" : "DISPERSÉ")}");
        }
        
        return isCohesive;
    }
    
    /// <summary>
    /// Calcule la vitesse moyenne de la squad
    /// </summary>
    private float CalculateAverageSquadSpeed(SquadController squadController)
    {
        List<SoldierAgent> soldiers = squadController.GetSoldiers();
        
        if (soldiers.Count == 0)
        {
            return 1f; // Fallback
        }
        
        float totalSpeed = 0f;
        int count = 0;
        
        foreach (SoldierAgent soldier in soldiers)
        {
            if (soldier == null || soldier.Movement == null) continue;
            
            totalSpeed += soldier.Movement.maxSpeed;
            count++;
        }
        
        return count > 0 ? totalSpeed / count : 5f; // Fallback 5 m/s
    }
    
    /// <summary>
    /// Trouve le prochain cluster disponible depuis la position actuelle
    /// IMPORTANT: Exclut les covers actuellement occupés par cette squad
    /// </summary>
    public CoverCluster FindNextCluster(SquadController squadController, CoverCluster currentCluster = null)
    {
        if (CoverClusterDetector.Instance == null)
        {
            return null;
        }
        
        Vector3 currentPosition = squadController.GetSquadCenter();
        int squadSize = squadController.GetAliveCount();
        
        // Obtenir tous les clusters disponibles
        CoverObject[] allCovers = GameObject.FindObjectsByType<CoverObject>(FindObjectsSortMode.None);
        
        // Créer une liste de covers EXCLUANT le cluster actuel
        List<CoverObject> availableCovers = new List<CoverObject>();
        
        foreach (CoverObject cover in allCovers)
        {
            // Ignorer si dans le cluster actuel
            if (currentCluster != null && currentCluster.covers.Contains(cover))
            {
                continue;
            }
            
            // Ignorer si occupé par AUTRE squad
            if (cover.isOccupied && !IsCoverOccupiedByThisSquad(cover, squadController))
            {
                continue;
            }
            
            availableCovers.Add(cover);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[CoverLeaveDecision] Recherche prochain cluster");
            Debug.Log($"[CoverLeaveDecision] Total covers: {allCovers.Length}");
            Debug.Log($"[CoverLeaveDecision] Covers disponibles (hors cluster actuel): {availableCovers.Count}");
        }
        
        // Trouver le meilleur cluster parmi les covers disponibles
        return CoverClusterDetector.Instance.FindBestClusterForSquad(
            currentPosition, 
            squadSize,
            availableCovers
        );
    }
    
    /// <summary>
    /// Vérifie si un cover est occupé par un soldat de cette squad
    /// </summary>
    private bool IsCoverOccupiedByThisSquad(CoverObject cover, SquadController squadController)
    {
        if (!cover.isOccupied || cover.occupyingSoldier == null)
        {
            return false;
        }
        
        int occupantSquadID = cover.occupyingSoldier.SquadID;
        
        // Comparer avec le squadID de la squad
        List<SoldierAgent> soldiers = squadController.GetSoldiers();
        if (soldiers.Count > 0 && soldiers[0] != null)
        {
            return occupantSquadID == soldiers[0].SquadID;
        }
        
        return false;
    }
}