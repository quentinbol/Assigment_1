using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ImprovedGameManager : MonoBehaviour
{
    [Header("=== PLACEMENT PHASE ===")]
    public int movableObjectsToPlace = 12;

    public List<GameObject> movableObjectPrefabs = new List<GameObject>();

    public Transform placementZoneStart;
    public Transform placementZoneEnd;
    public float placementZoneWidth = 10f;
    
    [Header("=== FIXED COVERS ===")]
    public List<CoverObject> fixedCovers = new List<CoverObject>();
    
    [Header("=== SQUADS ===")]
    public List<Squad> squads = new List<Squad>();

    public float squadSpawnDelay = 15f;

    public float initialSpawnDelay = 2f;
    
    [Header("=== VICTORY CONDITIONS ===")]
    public FinishZone finishZone;

    public Transform finishPoint;

    public float finishDistance = 5f;
    
    [Header("=== UI ===")]
    public bool showGUI = true;
    public GameObject placementUI;
    public UnityEngine.UI.Button startGameButton;
    public UnityEngine.UI.Text statusText;
    
    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;
    private enum GameState
    {
        Placement,
        Ready,
        Running,
        GameOver
    }
    
    private GameState currentState = GameState.Placement;

    private List<CoverObject> placedMovableCovers = new List<CoverObject>();
    private GameObject currentObjectBeingPlaced = null;
    private Camera mainCamera;
    private LayerMask groundLayer;

    private int currentSquadIndex = 0;
    private List<SquadController> squadControllers = new List<SquadController>();
    private Coroutine squadSpawnCoroutine;

    private int totalSoldiers = 0;
    private int deadSoldiers = 0;
    private int arrivedSoldiers = 0;
    private float gameStartTime = 0f;
    
    void Start()
    {
        mainCamera = Camera.main;
        groundLayer = LayerMask.GetMask("Ground", "Default");
        
        InitializeFixedCovers();
        InitializeSquads();
        SetupPlacementPhase();
    }
    
    void InitializeFixedCovers()
    {
        if (fixedCovers.Count == 0)
        {
            CoverObject[] allCovers = FindObjectsByType<CoverObject>(FindObjectsSortMode.None);
            foreach (var cover in allCovers)
            {
                if (cover.isFixed)
                {
                    fixedCovers.Add(cover);
                }
            }
        }

        foreach (var cover in fixedCovers)
        {
            if (cover != null)
            {
                cover.isFixed = true;
                cover.isPlaceable = false;
                cover.isPlaced = true;
            }
        }
    }
    
    void InitializeSquads()
    {
        totalSoldiers = 0;
        squadControllers.Clear();
        
        foreach (Squad squad in squads)
        {
            if (squad == null) continue;

            SquadController controller = squad.GetComponent<SquadController>();
            if (controller != null)
            {
                squadControllers.Add(controller);
                controller.RefreshSoldierList();

                totalSoldiers += squad.soldiers.Count;
                HideSquad(squad);
                DisableSquadTimers(squad);
            }
        }
    }

    void HideSquad(Squad squad)
    {
        if (squad == null || squad.soldiers == null) return;
        
        foreach (SoldierAgent soldier in squad.soldiers)
        {
            if (soldier == null) continue;

            Renderer[] renderers = soldier.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
    }

    void ShowSquad(Squad squad)
    {
        if (squad == null || squad.soldiers == null) return;
        
        foreach (SoldierAgent soldier in squad.soldiers)
        {
            if (soldier == null) continue;

            Renderer[] renderers = soldier.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }
    }

    void DisableSquadTimers(Squad squad)
    {
        if (squad == null || squad.soldiers == null) return;
        
        foreach (SoldierAgent soldier in squad.soldiers)
        {
            if (soldier == null) continue;
            
            ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
            if (timer != null)
            {
                timer.enabled = false;
            }
        }
    }

    void EnableSquadTimers(Squad squad)
    {
        if (squad == null || squad.soldiers == null) return;
        
        foreach (SoldierAgent soldier in squad.soldiers)
        {
            if (soldier == null) continue;
            
            ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
            if (timer != null)
            {
                timer.enabled = true;
                timer.ResetTimer();
            }
        }
    }
    
    void SetupPlacementPhase()
    {
        currentState = GameState.Placement;

        if (placementUI != null)
        {
            placementUI.SetActive(true);
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(OnStartGameClicked);
            startGameButton.interactable = false;
        }
        
        UpdateStatusText("Placement Phase: Click to place covers");
    }
    
    void Update()
    {
        switch (currentState)
        {
            case GameState.Placement:
                HandlePlacementInput();
                break;
                
            case GameState.Ready:
                break;
                
            case GameState.Running:
                UpdateGameStatistics();
                CheckEndConditions();
                break;
                
            case GameState.GameOver:
                break;
        }
    }
    
    void HandlePlacementInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentObjectBeingPlaced == null && placedMovableCovers.Count < movableObjectsToPlace)
            {
                SpawnNewMovableObject();
            }
            else if (currentObjectBeingPlaced != null)
            {
                PlaceCurrentObject();
            }
        }

        if (currentObjectBeingPlaced != null)
        {
            MoveObjectWithMouse();
        }

        if (Input.GetMouseButtonDown(1) && currentObjectBeingPlaced != null)
        {
            CancelPlacement();
        }

        if (Input.GetKey(KeyCode.R) && Input.GetMouseButtonDown(0))
        {
            TryRemovePlacedObject();
        }
    }
    
    void SpawnNewMovableObject()
    {
        GameObject prefab = movableObjectPrefabs[0];
        currentObjectBeingPlaced = Instantiate(prefab);

        CoverObject cover = currentObjectBeingPlaced.GetComponent<CoverObject>();
        if (cover != null)
        {
            cover.isPlaceable = true;
            cover.isFixed = false;
            cover.isPlaced = false;
        }
    }
    
    void MoveObjectWithMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 targetPos = hit.point;
            targetPos.y = 0.5f;
            if (IsInPlacementZone(targetPos))
            {
                currentObjectBeingPlaced.transform.position = targetPos;
                CoverObject cover = currentObjectBeingPlaced.GetComponent<CoverObject>();
                if (cover != null)
                {
                    cover.SetHovered(true);
                }
            }
        }
    }
    
    void PlaceCurrentObject()
    {
        if (currentObjectBeingPlaced == null) return;
        
        Vector3 pos = currentObjectBeingPlaced.transform.position;

        CoverObject cover = currentObjectBeingPlaced.GetComponent<CoverObject>();
        if (cover != null)
        {
            cover.isPlaced = true;
            cover.SetHovered(false);
            placedMovableCovers.Add(cover);
        }
        currentObjectBeingPlaced = null;
        CheckPlacementComplete();
    }
    
    void CancelPlacement()
    {
        if (currentObjectBeingPlaced != null)
        {
            Destroy(currentObjectBeingPlaced);
            currentObjectBeingPlaced = null;
        }
    }
    
    void TryRemovePlacedObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            CoverObject cover = hit.collider.GetComponent<CoverObject>();

            if (cover != null && !cover.isFixed && cover.isPlaced && placedMovableCovers.Contains(cover))
            {
                placedMovableCovers.Remove(cover);
                Destroy(cover.gameObject);

                if (startGameButton != null)
                {
                    startGameButton.interactable = false;
                }

            }
        }
    }
    
    void CheckPlacementComplete()
    {
        if (placedMovableCovers.Count >= movableObjectsToPlace)
        {
            currentState = GameState.Ready;
            
            if (startGameButton != null)
            {
                startGameButton.interactable = true;
            }
            UpdateStatusText($"Ready! All covers placed. Click START to begin.");
        }
        else
        {
            UpdateStatusText($"Placement: {placedMovableCovers.Count}/{movableObjectsToPlace} covers placed");
        }
    }
    
    bool IsInPlacementZone(Vector3 position)
    {
        if (placementZoneStart == null || placementZoneEnd == null) return true;
        
        float minZ = Mathf.Min(placementZoneStart.position.z, placementZoneEnd.position.z);
        float maxZ = Mathf.Max(placementZoneStart.position.z, placementZoneEnd.position.z);
        
        if (position.z < minZ || position.z > maxZ) return false;
        
        float centerX = (placementZoneStart.position.x + placementZoneEnd.position.x) / 2f;
        float minX = centerX - placementZoneWidth / 2f;
        float maxX = centerX + placementZoneWidth / 2f;
        
        if (position.x < minX || position.x > maxX) return false;
        
        return true;
    }
    
    void OnStartGameClicked()
    {
        if (currentState != GameState.Ready)
        {
            return;
        }
        
        StartGame();
    }
    
    void StartGame()
    {
        currentState = GameState.Running;
        gameStartTime = Time.time;
        currentSquadIndex = 0;

        if (placementUI != null)
        {
            placementUI.SetActive(false);
        }

        List<CoverObject> allCovers = new List<CoverObject>();
        allCovers.AddRange(fixedCovers);
        allCovers.AddRange(placedMovableCovers);
        squadSpawnCoroutine = StartCoroutine(SpawnSquadsSequentially());
        
        UpdateStatusText("Game Running...");
    }
    
    IEnumerator SpawnSquadsSequentially()
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        for (int i = 0; i < squadControllers.Count; i++)
        {
            if (currentState != GameState.Running)
            {
                yield break;
            }
            
            SendSquad(i);
            if (i < squadControllers.Count - 1)
            {
                yield return new WaitForSeconds(squadSpawnDelay);
            }
        }
    }
    
    void SendSquad(int index)
    {
        if (index < 0 || index >= squadControllers.Count)
        {
            return;
        }
        
        SquadController controller = squadControllers[index];
        Squad squad = squads[index];
        
        if (controller == null || squad == null)
        {
            return;
        }

        ShowSquad(squad);
        EnableSquadTimers(squad);
        controller.StartMovement();
        currentSquadIndex = index;
    }
    
    void UpdateGameStatistics()
    {
        deadSoldiers = 0;
        arrivedSoldiers = 0;
        if (finishZone != null)
        {
            arrivedSoldiers = finishZone.SoldiersArrived;
            foreach (Squad squad in squads)
            {
                if (squad == null || squad.soldiers == null) continue;
                
                foreach (SoldierAgent soldier in squad.soldiers)
                {
                    if (soldier == null) continue;
                    
                    ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
                    if (timer != null && timer.IsDead())
                    {
                        deadSoldiers++;
                    }
                }
            }
        }
        else
        {
            foreach (Squad squad in squads)
            {
                if (squad == null || squad.soldiers == null) continue;
                
                foreach (SoldierAgent soldier in squad.soldiers)
                {
                    if (soldier == null) continue;
                    ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
                    if (timer != null && timer.IsDead())
                    {
                        deadSoldiers++;
                        continue;
                    }
                    if (finishPoint != null)
                    {
                        float distance = Vector3.Distance(soldier.transform.position, finishPoint.position);
                        if (distance <= finishDistance)
                        {
                            arrivedSoldiers++;
                        }
                    }
                }
            }
        }
    }
    
    void CheckEndConditions()
    {
        int aliveSoldiers = totalSoldiers - deadSoldiers;
        if (aliveSoldiers == 0)
        {
            EndGame(false, "DEFEAT - All soldiers are dead!");
        }
        if (arrivedSoldiers == aliveSoldiers && aliveSoldiers > 0)
        {
            EndGame(true, $"VICTORY - {arrivedSoldiers}/{totalSoldiers} soldiers survived!");
        }
    }
    
    void EndGame(bool victory, string message)
    {
        if (currentState == GameState.GameOver) return;
        
        currentState = GameState.GameOver;
        if (squadSpawnCoroutine != null)
        {
            StopCoroutine(squadSpawnCoroutine);
        }
        
        float gameTime = Time.time - gameStartTime;
        
        UpdateStatusText(message);
    }
    
    void UpdateStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
    }
    
    void OnGUI()
    {
        if (!showGUI) return;

        if (currentState == GameState.Placement || currentState == GameState.Ready)
        {
            DrawPlacementGUI();
        }
        if (currentState == GameState.Running || currentState == GameState.GameOver)
        {
            DrawGameGUI();
        }
    }
    
    void DrawPlacementGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== PLACEMENT PHASE ===", new GUIStyle() { 
            fontSize = 14, 
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState() { textColor = Color.white }
        });
        
        GUILayout.Space(5);
        GUI.color = placedMovableCovers.Count >= movableObjectsToPlace ? Color.green : Color.yellow;
        GUILayout.Label($"Covers Placed: {placedMovableCovers.Count}/{movableObjectsToPlace}");
        GUI.color = Color.cyan;
        GUILayout.Label($"Fixed Covers: {fixedCovers.Count}");
        GUI.color = Color.white;
        int totalCovers = fixedCovers.Count + placedMovableCovers.Count;
        GUILayout.Label($"Total Covers: {totalCovers}");
        GUILayout.Space(5);
        GUI.color = Color.gray;
        GUILayout.Label("Left Click: Place cover", new GUIStyle() { fontSize = 10 });
        GUILayout.Label("Right Click: Cancel", new GUIStyle() { fontSize = 10 });
        GUILayout.Label("R + Click: Remove placed", new GUIStyle() { fontSize = 10 });
        GUI.color = Color.white;
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void DrawGameGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 250));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== GAME STATISTICS ===", new GUIStyle() { 
            fontSize = 14, 
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState() { textColor = Color.white }
        });
        GUILayout.Space(5);
        int aliveSoldiers = totalSoldiers - deadSoldiers;
        GUI.color = Color.green;
        GUILayout.Label($"Alive: {aliveSoldiers}/{totalSoldiers}");
        GUI.color = Color.red;
        GUILayout.Label($"Dead: {deadSoldiers}");
        if (finishPoint != null)
        {
            GUI.color = Color.cyan;
            GUILayout.Label($"Arrived: {arrivedSoldiers}");
        }

        GUI.color = Color.white;
        float survivalRate = totalSoldiers > 0 ? (float)aliveSoldiers / totalSoldiers * 100f : 0f;
        GUILayout.Label($"Survival Rate: {survivalRate:F1}%");
        
        GUILayout.Space(5);
        GUI.color = Color.yellow;
        int deployedSquads = currentSquadIndex + 1;
        if (currentState == GameState.Running && squadSpawnCoroutine == null)
        {
            deployedSquads = squadControllers.Count;
        }
        GUILayout.Label($"Squads Deployed: {deployedSquads}/{squadControllers.Count}");
        
        GUILayout.Space(5);

        if (currentState == GameState.Running)
        {
            float gameTime = Time.time - gameStartTime;
            GUI.color = Color.white;
            GUILayout.Label($"Time: {gameTime:F1}s");
        }
        
        GUILayout.Space(10);

        if (currentState == GameState.GameOver)
        {
            GUI.color = arrivedSoldiers > 0 ? Color.green : Color.red;
            GUILayout.Label(arrivedSoldiers > 0 ? "=== VICTORY ===" : "=== DEFEAT ===", 
                new GUIStyle() { 
                    fontSize = 16, 
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState() { textColor = GUI.color }
                });
        }
        
        GUI.color = Color.white;
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void OnDrawGizmos()
    {
        if (placementZoneStart == null || placementZoneEnd == null) return;
        
        Gizmos.color = Color.cyan;
        
        float centerX = (placementZoneStart.position.x + placementZoneEnd.position.x) / 2f;
        float minZ = Mathf.Min(placementZoneStart.position.z, placementZoneEnd.position.z);
        float maxZ = Mathf.Max(placementZoneStart.position.z, placementZoneEnd.position.z);
        
        Vector3[] corners = new Vector3[4];
        corners[0] = new Vector3(centerX - placementZoneWidth / 2f, 0, minZ);
        corners[1] = new Vector3(centerX + placementZoneWidth / 2f, 0, minZ);
        corners[2] = new Vector3(centerX + placementZoneWidth / 2f, 0, maxZ);
        corners[3] = new Vector3(centerX - placementZoneWidth / 2f, 0, maxZ);
        
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
        if (finishZone == null && finishPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(finishPoint.position, finishDistance);
        }
    }
    
    public int GetTotalSoldiers() => totalSoldiers;
    public int GetAliveSoldiers() => totalSoldiers - deadSoldiers;
    public int GetDeadSoldiers() => deadSoldiers;
    public int GetArrivedSoldiers() => arrivedSoldiers;
    public float GetSurvivalRate() => totalSoldiers > 0 ? (float)(totalSoldiers - deadSoldiers) / totalSoldiers : 0f;
    public bool IsGameRunning() => currentState == GameState.Running;
    public bool IsGameOver() => currentState == GameState.GameOver;
}