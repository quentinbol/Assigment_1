/*using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPaletteEntry
{
    public GameObject prefab;
    public Sprite icon;
    public int count;
}


public class PlacementManager : MonoBehaviour
{
    [Header("Object Palette")]
    public List<ObjectPaletteEntry> availableObjects = new List<ObjectPaletteEntry>();
    public Transform paletteContainer;
    public GameObject paletteSlotPrefab;
    
    [Header("Fixed Covers")]
    public List<CoverObject> fixedCovers = new List<CoverObject>();
    
    [Header("Placement Settings")]
    public LayerMask placementAreaLayer;
    public float placementHeight = 0.5f;
    public float gridSnapSize = 0.5f;
    public bool useGridSnap = true;
    
    [Header("Placement Bounds")]
    public Transform canyonStart;
    public Transform canyonEnd;
    public float canyonWidth = 10f;
    
    [Header("UI References")]
    public GameObject placementUI;
    public UnityEngine.UI.Button startGameButton;
    public UnityEngine.UI.Text instructionText;
    
    [Header("Game Rules")]
    public int totalObjectsToPlace = 12;
    public int minimumObjectsRequired = 12; // NOUVEAU : 0 = pas de minimum
    public bool allowStartWithoutAll = true; // NOUVEAU : autoriser lancement sans tout placer
    
    [Header("State")]
    public bool isPlacementMode = true;
    
    private Camera mainCamera;
    private CoverObject currentCover;
    private ObjectPalette selectedPalette;
    private List<CoverObject> placedCovers = new List<CoverObject>();
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (startGameButton != null)
        {
            startGameButton.interactable = allowStartWithoutAll;
            startGameButton.onClick.AddListener(OnStartGame);
        }
        
        InitializeFixedCovers(); // NOUVEAU
        InitializePalette();
        UpdateInstructionText();
    }
    
    void InitializeFixedCovers()
    {
        foreach (var cover in fixedCovers)
        {
            if (cover != null)
            {
                cover.isFixed = true;
                cover.isPlaceable = false;
                cover.isPlaced = true;
            }
        }
        
        Debug.Log($"Initialized {fixedCovers.Count} fixed covers");
    }
    
    void InitializePalette()
    {
        if (paletteContainer == null || paletteSlotPrefab == null) return;
        
        // Calculer combien d'objets on peut placer
        int totalAvailable = 0;
        foreach (var entry in availableObjects)
        {
            totalAvailable += entry.count;
        }
        totalObjectsToPlace = totalAvailable;
        
        foreach (var entry in availableObjects)
        {
            GameObject slotObj = Instantiate(paletteSlotPrefab, paletteContainer);
            ObjectPalette palette = slotObj.GetComponent<ObjectPalette>();
            
            if (palette != null)
            {
                CoverObject coverPrefab = entry.prefab.GetComponent<CoverObject>();
                palette.Initialize(coverPrefab, entry.count, this);
                
                if (palette.iconImage != null && entry.icon != null)
                {
                    palette.iconImage.sprite = entry.icon;
                }
            }
        }
    }
    
    void Update()
    {
        if (!isPlacementMode) return;
        
        HandlePlacementInput();
    }
    
    void HandlePlacementInput()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // CLIC POUR SPAWNER/PLACER
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedPalette != null && currentCover == null)
            {
                currentCover = selectedPalette.SpawnObject();
                
                if (currentCover != null)
                {
                    Debug.Log($"Spawned: {currentCover.name}");
                }
            }
            else if (currentCover != null)
            {
                if (IsWithinCanyonBounds(currentCover.transform.position))
                {
                    PlaceCover(currentCover);
                    currentCover = null;
                }
            }
        }
        
        // DÉPLACER L'OBJET
        if (currentCover != null)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, placementAreaLayer))
            {
                Vector3 targetPos = hit.point;
                targetPos.y = placementHeight;
                
                if (IsWithinCanyonBounds(targetPos))
                {
                    if (useGridSnap)
                    {
                        targetPos = SnapToGrid(targetPos);
                    }
                    
                    currentCover.transform.position = targetPos;
                    currentCover.SetHovered(true);
                }
            }
        }
        
        // ANNULER
        if (Input.GetMouseButtonDown(1))
        {
            if (currentCover != null)
            {
                if (selectedPalette != null)
                {
                    selectedPalette.ReturnObject();
                }
                
                Destroy(currentCover.gameObject);
                currentCover = null;
                
                Debug.Log("Placement cancelled");
            }
        }
        
        // SUPPRIMER UN OBJET PLACÉ (R + Clic)
        if (Input.GetKey(KeyCode.R) && Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                CoverObject cover = hit.collider.GetComponent<CoverObject>();
                
                // CHANGEMENT : Ne pas permettre de supprimer les covers fixes
                if (cover != null && cover.isPlaced && !cover.isFixed && placedCovers.Contains(cover))
                {
                    RemovePlacedCover(cover);
                }
                else if (cover != null && cover.isFixed)
                {
                    Debug.Log("Cannot remove fixed cover!");
                }
            }
        }
    }
    
    public void SelectObjectType(ObjectPalette palette)
    {
        if (selectedPalette != null)
        {
            selectedPalette.SetSelected(false);
        }
        
        selectedPalette = palette;
        selectedPalette.SetSelected(true);
        
        Debug.Log($"Selected object type: {palette.coverPrefab.name}");
    }
    
    void PlaceCover(CoverObject cover)
    {
        cover.isPlaced = true;
        cover.SetHovered(false);
        placedCovers.Add(cover);
        
        UpdateInstructionText();
        UpdateStartButton(); // NOUVEAU
        
        Debug.Log($"Placed {cover.name} ({placedCovers.Count}/{totalObjectsToPlace})");
    }
    
    void RemovePlacedCover(CoverObject cover)
    {
        placedCovers.Remove(cover);
        
        foreach (var entry in availableObjects)
        {
            if (cover.name.Contains(entry.prefab.name))
            {
                ObjectPalette[] palettes = paletteContainer.GetComponentsInChildren<ObjectPalette>();
                foreach (var palette in palettes)
                {
                    if (palette.coverPrefab == entry.prefab)
                    {
                        palette.ReturnObject();
                        break;
                    }
                }
                break;
            }
        }
        
        Destroy(cover.gameObject);
        
        UpdateInstructionText();
        UpdateStartButton(); // NOUVEAU
        
        Debug.Log($"Removed cover ({placedCovers.Count}/{totalObjectsToPlace})");
    }
    
    // NOUVEAU : Gérer l'état du bouton Start
    void UpdateStartButton()
    {
        if (startGameButton == null) return;
        
        if (allowStartWithoutAll)
        {
            // Toujours actif si on autorise le lancement partiel
            // OU actif si on a atteint le minimum requis
            startGameButton.interactable = placedCovers.Count >= minimumObjectsRequired;
        }
        else
        {
            // Actif seulement si tous les objets sont placés
            startGameButton.interactable = placedCovers.Count >= totalObjectsToPlace;
        }
    }
    
    bool IsWithinCanyonBounds(Vector3 position)
    {
        if (canyonStart == null || canyonEnd == null) return true;
        
        float minZ = Mathf.Min(canyonStart.position.z, canyonEnd.position.z);
        float maxZ = Mathf.Max(canyonStart.position.z, canyonEnd.position.z);
        
        if (position.z < minZ || position.z > maxZ) return false;
        
        float centerX = (canyonStart.position.x + canyonEnd.position.x) / 2f;
        float minX = centerX - canyonWidth / 2f;
        float maxX = centerX + canyonWidth / 2f;
        
        if (position.x < minX || position.x > maxX) return false;
        
        return true;
    }
    
    Vector3 SnapToGrid(Vector3 position)
    {
        position.x = Mathf.Round(position.x / gridSnapSize) * gridSnapSize;
        position.z = Mathf.Round(position.z / gridSnapSize) * gridSnapSize;
        return position;
    }
    
    void UpdateInstructionText()
    {
        if (instructionText != null)
        {
            string placementInfo = $"Placed: {placedCovers.Count}/{totalObjectsToPlace}";
            
            if (fixedCovers.Count > 0)
            {
                placementInfo += $" (+ {fixedCovers.Count} fixed)";
            }
            
            if (allowStartWithoutAll && minimumObjectsRequired > 0)
            {
                placementInfo += $" (min: {minimumObjectsRequired})";
            }
            
            instructionText.text = placementInfo + "\n" +
                                   "Click Slot → Click Map | Right Click: Cancel | R+Click: Remove";
        }
    }
    
    void OnStartGame()
    {
        isPlacementMode = false;
        
        if (placementUI != null)
        {
            placementUI.SetActive(false);
        }
        
        // NOUVEAU : Combiner covers fixes + covers placées
        List<Transform> allCovers = new List<Transform>();
        
        // Ajouter les covers fixes
        foreach (var cover in fixedCovers)
        {
            if (cover != null)
            {
                allCovers.Add(cover.transform);
            }
        }
        
        // Ajouter les covers placées par le joueur
        foreach (var cover in placedCovers)
        {
            if (cover != null)
            {
                allCovers.Add(cover.transform);
            }
        }
        
        // Trier par position Z (optionnel mais recommandé)
        allCovers.Sort((a, b) => a.position.z.CompareTo(b.position.z));
        
        Transform[] coverTransforms = allCovers.ToArray();

        TestManager testManager = FindObjectsByType<TestManager>(FindObjectsSortMode.None)[0];
        if (testManager != null)
        {
            testManager.OnPlacementComplete();
        }
        
        Debug.Log($"=== GAME STARTED === {allCovers.Count} total covers ({fixedCovers.Count} fixed + {placedCovers.Count} placed)");
    }
    
    void OnDrawGizmos()
    {
        if (canyonStart == null || canyonEnd == null) return;
        
        Gizmos.color = Color.cyan;
        
        float centerX = (canyonStart.position.x + canyonEnd.position.x) / 2f;
        float minZ = Mathf.Min(canyonStart.position.z, canyonEnd.position.z);
        float maxZ = Mathf.Max(canyonStart.position.z, canyonEnd.position.z);
        
        Vector3[] corners = new Vector3[4];
        corners[0] = new Vector3(centerX - canyonWidth / 2f, 0, minZ);
        corners[1] = new Vector3(centerX + canyonWidth / 2f, 0, minZ);
        corners[2] = new Vector3(centerX + canyonWidth / 2f, 0, maxZ);
        corners[3] = new Vector3(centerX - canyonWidth / 2f, 0, maxZ);
        
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }
}*/