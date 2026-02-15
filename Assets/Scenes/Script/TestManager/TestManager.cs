using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// TestManager SIMPLE pour le projet
/// - Envoie une squad vers la fin du canyon
/// - Si des covers sont à proximité, les soldats s'y dispersent automatiquement
/// </summary>
public class TestManager : MonoBehaviour
{
    [Header("Game Start")]
    public KeyCode startKey = KeyCode.Space;
    public bool gameStarted = false;
    
    [Header("Squad")]
    public Squad squad; // Une seule squad pour le test
    
    [Header("Destination")]
    public Transform finishPoint; // Point de fin du canyon
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private SquadController squadController;
    
    void Start()
    {
        // Récupérer le SquadController
        if (squad != null)
        {
            squadController = squad.GetComponent<SquadController>();
            if (squadController != null)
            {
                squadController.RefreshSoldierList();
            }
            else
            {
                Debug.LogError("Squad n'a pas de SquadController !");
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[TestManager] Prêt. Appuie sur SPACE pour commencer.");
        }
    }
    
    void Update()
    {
        // Démarrer avec SPACE
        if (Input.GetKeyDown(startKey) && !gameStarted)
        {
            StartGame();
        }
    }
    
    void StartGame()
    {
        gameStarted = true;
        
        if (showDebugLogs)
        {
            Debug.Log("=== JEU COMMENCE ===");
        }
        
        if (squadController == null || finishPoint == null)
        {
            Debug.LogError("Squad ou FinishPoint manquant !");
            return;
        }
        
        // Envoyer la squad vers la fin du canyon
        // Les soldats chercheront automatiquement des covers à proximité pendant le trajet
        squadController.MoveSquadToDestination(finishPoint.position);
        
        if (showDebugLogs)
        {
            Debug.Log($"[{squad.squadName}] Envoyée vers la fin du canyon !");
            Debug.Log("Les soldats chercheront des covers proches automatiquement.");
        }
    }
    
    void OnGUI()
    {
        if (!showDebugLogs) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== TEST MANAGER ===");
        GUILayout.Label($"Game Started: {gameStarted}");
        
        if (squadController != null && squad != null)
        {
            GUILayout.Space(5);
            GUILayout.Label($"Squad: {squad.squadName}");
            GUILayout.Label($"Moving: {squadController.IsSquadMoving()}");
            GUILayout.Label($"In Cover: {squadController.GetInCoverCount()}/{squadController.GetAliveCount()}");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}