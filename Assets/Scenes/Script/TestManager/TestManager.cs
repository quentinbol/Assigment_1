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
    public KeyCode sendNextSquadKey = KeyCode.N;
    public bool gameStarted = false;
    
    [Header("Squad")]
    public List<Squad> squads = new List<Squad>();
    
    [Header("Destination")]
    public Transform finishPoint; // Point de fin du canyon
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private List<SquadController> squadControllers = new List<SquadController>();
    
    void Start()
    {
        // Récupérer le SquadController pour chaque squad
        foreach (Squad squad in squads)
        {
            SquadController squadController = squad.GetComponent<SquadController>();
            if (squadController != null)
            {
                squadControllers.Add(squadController);
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
        if (Input.GetKeyDown(sendNextSquadKey) && gameStarted)
        {
            SendNextSquad();
        }
    }
    
    void StartGame()
    {
        gameStarted = true;
        
        if (showDebugLogs)
        {
            Debug.Log("=== JEU COMMENCE ===");
        }
        
        if (squadControllers.Count == 0 || finishPoint == null)
        {
            Debug.LogError("Squad ou FinishPoint manquant !");
            return;
        }
        
        
        squadControllers[0].MoveSquadToDestination(finishPoint.position);
        
        if (showDebugLogs)
        {
            Debug.Log($"[{squads[0].squadName}] Envoyée vers la fin du canyon !");
            Debug.Log("Les soldats chercheront des covers proches automatiquement.");
        }
    }
    
    void SendNextSquad()
    {
        if (squadControllers.Count == 0 || finishPoint == null)
        {
            Debug.LogError("Squad ou FinishPoint manquant !");
            return;
        }

        for (int i = 1; i < squadControllers.Count; i++)
        {
            if (!squadControllers[i].IsSquadMoving())
            {
                squadControllers[i].MoveSquadToDestination(finishPoint.position);
                
                if (showDebugLogs)
                {
                    Debug.Log($"[{squads[i].squadName}] Envoyée vers la fin du canyon !");
                }
                return;
            }
        }
    }
}