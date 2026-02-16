using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestionnaire de jeu simple
/// - Affiche le nombre de survivants
/// - Détecte la fin de partie (tous morts ou tous arrivés)
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Squads")]
    public List<Squad> squads = new List<Squad>();
    
    [Header("Victory Condition")]
    [Tooltip("Position finale (si un soldat arrive ici, il a gagné)")]
    public Transform finishPoint;
    
    [Tooltip("Distance pour considérer arrivé")]
    public float finishDistance = 5f;
    
    [Header("UI")]
    public bool showGUI = true;
    
    private int totalSoldiers = 0;
    private int deadSoldiers = 0;
    private int arrivedSoldiers = 0;
    private bool gameEnded = false;
    
    void Start()
    {
        // Compter le nombre total de soldats
        foreach (Squad squad in squads)
        {
            if (squad != null && squad.soldiers != null)
            {
                totalSoldiers += squad.soldiers.Count;
            }
        }
        
        Debug.Log($"[GameManager] Démarrage - {totalSoldiers} soldats au total");
    }
    
    void Update()
    {
        if (gameEnded) return;
        
        UpdateStatistics();
        CheckEndConditions();
    }
    
    /// <summary>
    /// Mise à jour des statistiques
    /// </summary>
    void UpdateStatistics()
    {
        deadSoldiers = 0;
        arrivedSoldiers = 0;
        
        foreach (Squad squad in squads)
        {
            if (squad == null || squad.soldiers == null) continue;
            
            foreach (SoldierAgent soldierAgent in squad.soldiers)
            {
                if (soldierAgent == null) continue;
                
                // Vérifier si mort
                ExposureTimer timer = soldierAgent.GetComponent<ExposureTimer>();
                if (timer != null && timer.IsDead())
                {
                    deadSoldiers++;
                    continue;
                }
                
                // Vérifier si arrivé
                if (finishPoint != null)
                {
                    float distance = Vector3.Distance(soldierAgent.transform.position, finishPoint.position);
                    if (distance <= finishDistance)
                    {
                        arrivedSoldiers++;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Vérifier les conditions de fin de partie
    /// </summary>
    void CheckEndConditions()
    {
        int aliveSoldiers = totalSoldiers - deadSoldiers;
        
        // Tous morts
        if (aliveSoldiers == 0)
        {
            EndGame("DÉFAITE - Tous les soldats sont morts !");
        }
        
        // Tous arrivés
        if (arrivedSoldiers == aliveSoldiers && aliveSoldiers > 0)
        {
            EndGame($"VICTOIRE - {arrivedSoldiers}/{totalSoldiers} soldats ont survécu !");
        }
    }
    
    /// <summary>
    /// Fin de partie
    /// </summary>
    void EndGame(string message)
    {
        if (gameEnded) return;
        
        gameEnded = true;
        
        Debug.Log($"[GameManager] ===== FIN DE PARTIE =====");
        Debug.Log($"[GameManager] {message}");
        Debug.Log($"[GameManager] Score: {arrivedSoldiers}/{totalSoldiers} survivants");
        Debug.Log($"[GameManager] Morts: {deadSoldiers}");
        
        // Optionnel : Mettre le jeu en pause
        // Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Obtenir le nombre de survivants
    /// </summary>
    public int GetSurvivors()
    {
        return totalSoldiers - deadSoldiers;
    }
    
    /// <summary>
    /// Obtenir le score en pourcentage
    /// </summary>
    public float GetSurvivalRate()
    {
        if (totalSoldiers == 0) return 0f;
        return (float)(totalSoldiers - deadSoldiers) / totalSoldiers;
    }
    
    void OnGUI()
    {
        if (!showGUI) return;
        
        // Affichage du score
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== STATISTIQUES ===", new GUIStyle() { 
            fontSize = 14, 
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState() { textColor = Color.white }
        });
        
        GUILayout.Space(5);
        
        int aliveSoldiers = totalSoldiers - deadSoldiers;
        
        // Survivants
        GUI.color = Color.green;
        GUILayout.Label($"Survivants: {aliveSoldiers}/{totalSoldiers}");
        
        // Morts
        GUI.color = Color.red;
        GUILayout.Label($"Morts: {deadSoldiers}");
        
        // Arrivés
        if (finishPoint != null)
        {
            GUI.color = Color.cyan;
            GUILayout.Label($"Arrivés: {arrivedSoldiers}");
        }
        
        // Taux de survie
        GUI.color = Color.white;
        float survivalRate = GetSurvivalRate() * 100f;
        GUILayout.Label($"Taux de survie: {survivalRate:F1}%");
        
        GUILayout.Space(10);
        
        // Message de fin
        if (gameEnded)
        {
            GUI.color = arrivedSoldiers > 0 ? Color.green : Color.red;
            GUILayout.Label(arrivedSoldiers > 0 ? "=== VICTOIRE ===" : "=== DÉFAITE ===", 
                new GUIStyle() { 
                    fontSize = 16, 
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState() { textColor = GUI.color }
                });
        }
        
        GUI.color = Color.white;
        GUILayout.EndVertical();
        GUILayout.EndArea();
        
        // Afficher le timer de chaque soldat actif
        if (Input.GetKey(KeyCode.T))
        {
            ShowAllTimers();
        }
    }
    
    /// <summary>
    /// Afficher les timers de tous les soldats (maintenir T)
    /// </summary>
    void ShowAllTimers()
    {
        int yOffset = 220;
        
        GUILayout.BeginArea(new Rect(10, yOffset, 400, 400));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Timers d'exposition (Maintenir T)", new GUIStyle() { 
            fontSize = 12, 
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState() { textColor = Color.yellow }
        });
        
        foreach (Squad squad in squads)
        {
            if (squad == null || squad.soldiers == null) continue;
            
            foreach (SoldierAgent soldierAgent in squad.soldiers)
            {
                if (soldierAgent == null) continue;
                
                ExposureTimer timer = soldierAgent.GetComponent<ExposureTimer>();
                if (timer != null && !timer.IsDead())
                {
                    float timeRemaining = timer.GetTimeUntilDeath();
                    float ratio = timer.GetExposureRatio();
                    
                    Color textColor = Color.white;
                    if (ratio > 0f)
                    {
                        if (ratio < 0.5f)
                            textColor = Color.yellow;
                        else if (ratio < 0.8f)
                            textColor = new Color(1f, 0.5f, 0f);
                        else
                            textColor = Color.red;
                    }
                    
                    GUI.color = textColor;
                    
                    string inCover = soldierAgent.IsInCover() ? "[COVER]" : "[EXPOSED]";
                    GUILayout.Label($"{soldierAgent.name}: {timeRemaining:F1}s {inCover}");
                }
            }
        }
        
        GUI.color = Color.white;
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Reset le jeu (pour tests)
    /// </summary>
    public void ResetGame()
    {
        gameEnded = false;
        deadSoldiers = 0;
        arrivedSoldiers = 0;
        
        // Ressusciter tous les soldats
        foreach (Squad squad in squads)
        {
            if (squad == null || squad.soldiers == null) continue;
            
            foreach (SoldierAgent soldierAgent in squad.soldiers)
            {
                if (soldierAgent == null) continue;
                
                ExposureTimer timer = soldierAgent.GetComponent<ExposureTimer>();
                if (timer != null)
                {
                    timer.Revive();
                }
            }
        }
        
        Debug.Log("[GameManager] Jeu reset !");
    }
}