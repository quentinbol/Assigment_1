using UnityEngine;
using System.Collections.Generic;

[System.Serializable]

public class TestManager : MonoBehaviour
{
    [Header("Squads")]
    public List<Squad> squads = new List<Squad>();
    
    [Header("Covers")]
    public List<CoverObject> coverObjects = new List<CoverObject>();
    
    [Header("Game State")]
    public bool gameStarted = false;
    
    [Header("Test Controls")]
    public KeyCode startKey = KeyCode.Space;
    public KeyCode nextSquadKey = KeyCode.N;
    public KeyCode resetKey = KeyCode.R;
    public KeyCode infoKey = KeyCode.I;
    
    [Header("Test Modes")]
    public TestMode currentMode = TestMode.Manual;
    public float delayBetweenSquads = 3f;
    
    private int currentSquadIndex = 0;
    
    public enum TestMode
    {
        Manual,
        AutoSequential,
        AutoSimultaneous
    }
    
    void Start()
    {
        /*ValidateSetup();
        SendSquad(0);
        currentSquadIndex = 1;

        
        switch (currentMode)
        {
            case TestMode.Manual:
                Debug.Log("=== MANUAL MODE ===");
                Debug.Log("Press SPACE to send first squad");
                Debug.Log("Press N to send next squad");
                Debug.Log("Press R to reset");
                Debug.Log("Press I for squad info");
                break;
                
            case TestMode.AutoSequential:
                StartCoroutine(AutoSequentialTest());
                break;
                
            case TestMode.AutoSimultaneous:
                AutoSimultaneousTest();
                break;
        }*/
    }
    
    void Update()
    {
        if (!gameStarted) return;
        if (currentMode != TestMode.Manual) return;
        
        // Envoyer la première escouade
        if (Input.GetKeyDown(startKey))
        {
            SendSquad(0);
            currentSquadIndex = 1;
        }
        
        // Envoyer l'escouade suivante
        if (Input.GetKeyDown(nextSquadKey))
        {
            if (currentSquadIndex < squads.Count)
            {
                SendSquad(currentSquadIndex);
                currentSquadIndex++;
            }
            else
            {
                Debug.Log("All squads have been sent!");
            }
        }
        
        // Reset
        if (Input.GetKeyDown(resetKey))
        {
            ResetTest();
        }
        
        // Info
        if (Input.GetKeyDown(infoKey))
        {
            PrintSquadInfo();
        }
    }
    
    void ValidateSetup()
    {
        if (squads.Count == 0)
        {
            Debug.LogError("No squads assigned to TestManager!");
            return;
        }
        
        if (coverObjects.Count == 0)
        {
            Debug.LogError("No cover positions assigned to TestManager!");
            return;
        }
        
        // Vérifier que chaque squad a des soldats
        foreach (var squad in squads)
        {
            if (squad.soldiers.Count == 0)
            {
                Debug.LogWarning($"{squad.squadName} has no soldiers!");
            }
        }
        
        Debug.Log($"Setup OK: {squads.Count} squads, {coverObjects.Count} covers");
    }
    
    void SendSquad(int squadIndex)
    {
        if (squadIndex >= squads.Count)
        {
            Debug.LogWarning($"Squad index {squadIndex} out of range!");
            return;
        }
        
        Squad squad = squads[squadIndex];
        
        // Calculer l'index de départ des covers pour cette escouade
        // Chaque escouade utilise 6 covers (ou soldier.Count)
        int coverStartIndex = squadIndex * squad.soldiers.Count;
        
        // Utiliser la méthode SendToCover du Squad
        squad.SendToCover(coverObjects, coverStartIndex);
        
        Debug.Log($"[{Time.time:F1}s] Sent {squad.squadName} (covers {coverStartIndex} to {coverStartIndex + squad.soldiers.Count - 1})");
    }
    
    void ResetTest()
    {
        Debug.Log("=== RESET ===");
        currentSquadIndex = 0;
        
        // Faire sortir tous les soldats de leur cover
        foreach (var squad in squads)
        {
            squad.LeaveAllCovers();
        }
        
        // Libérer tous les covers
        if (coverObjects != null)
        {
            foreach (var cover in coverObjects)
            {
                if (cover != null)
                    cover.SetFree();
            }
        }
        
        // Optionnel : Repositionner les escouades à leur position de départ
        // (Vous pouvez stocker les positions initiales si nécessaire)
    }
    
    void PrintSquadInfo()
    {
        Debug.Log("=== SQUAD STATUS ===");
        
        foreach (var squad in squads)
        {
            int moving = 0;
            int inCover = 0;
            
            foreach (var soldier in squad.soldiers)
            {
                if (soldier.currentState == SoldierState.Moving)
                    moving++;
                else
                    inCover++;
            }
            
            bool allInCover = squad.IsSquadInCover();
            string status = allInCover ? "[ALL IN COVER]" : "[MOVING]";
            
            Debug.Log($"{squad.squadName} {status}: {moving} moving, {inCover} in cover, {squad.GetAliveCount()} alive");
        }
    }
    
    // Mode Auto : Séquentiel
    System.Collections.IEnumerator AutoSequentialTest()
    {
        Debug.Log("=== AUTO SEQUENTIAL TEST ===");
        
        for (int i = 0; i < squads.Count; i++)
        {
            SendSquad(i);
            yield return new WaitForSeconds(delayBetweenSquads);
        }
        
        Debug.Log("=== All squads sent ===");
    }
    
    // Mode Auto : Simultané
    void AutoSimultaneousTest()
    {
        Debug.Log("=== AUTO SIMULTANEOUS TEST ===");
        
        for (int i = 0; i < squads.Count; i++)
        {
            SendSquad(i);
        }
        
        Debug.Log("=== All squads sent at once ===");
    }
    
    // Visualisation dans l'éditeur
    private void OnDrawGizmos()
    {
        if (coverObjects == null || coverObjects.Count == 0) return;

        // Dessiner les connections entre les covers
        Gizmos.color = Color.yellow;
        for (int i = 0; i < coverObjects.Count - 1; i++)
        {
            if (coverObjects[i].transform != null && coverObjects[i + 1].transform != null)
            {
                Gizmos.DrawLine(
                    coverObjects[i].transform.position + Vector3.up * 0.5f, 
                    coverObjects[i + 1].transform.position + Vector3.up * 0.5f
                );
            }
        }

        // Numéroter les covers
        for (int i = 0; i < coverObjects.Count; i++)
        {
            if (coverObjects[i].transform != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(coverObjects[i].transform.position, 0.5f);
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    coverObjects[i].transform.position + Vector3.up * 1.5f, 
                    i.ToString()
                );
                #endif
            }
        }
    }

    public void OnPlacementComplete()
    {
        gameStarted = true;
        
        // Mettre à jour le tableau de covers avec les nouvelles positions
        UpdateCoverPositions();
        
        // Commencer le jeu selon le mode
        switch (currentMode)
        {
            case TestMode.Manual:
                Debug.Log("=== MANUAL MODE ===");
                Debug.Log("Press SPACE to send first squad");
                break;
                
            case TestMode.AutoSequential:
                StartCoroutine(AutoSequentialTest());
                break;
                
            case TestMode.AutoSimultaneous:
                AutoSimultaneousTest();
                break;
        }
    }
    
    void UpdateCoverPositions()
    {
        // Récupérer toutes les covers (fixes + placées)
        CoverObject[] allCovers = FindObjectsByType<CoverObject>(FindObjectsSortMode.None);
        List<CoverObject> covers = new List<CoverObject>();

        foreach (var cover in allCovers)
        {
            if (cover.isPlaced || !cover.isPlaceable) // Fixes ou placées
            {
                covers.Add(cover);
            }
        }

        // Trier par position Z (du début à la fin du canyon)
        covers.Sort((a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

        coverObjects = covers;

        Debug.Log($"Updated cover positions: {coverObjects.Count} covers available");
    }
}