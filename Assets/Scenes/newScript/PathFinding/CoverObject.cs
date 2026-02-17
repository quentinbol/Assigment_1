using UnityEngine;
using UnityEngine.AI;

public class CoverObject : MonoBehaviour
{
    [Header("cover properties")]
    public bool isPlaceable = true;
    public bool isFixed = false;
    public bool isOccupied = false;
    public SoldierAgent occupyingSoldier = null;
    
    [Header("placement")]
    public bool isPlaced = false;
    public Color normalColor = Color.gray;
    public Color placedColor = Color.green;
    public Color occupiedColor = Color.red;
    public Color hoverColor = Color.yellow;
    
    private Renderer objectRenderer;
    private Material objectMaterial;
    private bool isHovered = false;
    
    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectMaterial = objectRenderer.material;

            if (isFixed)
            {
                isPlaceable = false;
                isPlaced = true;
            }

            UpdateColor();
        }
        
    }
    
    void Update()
    {
        UpdateColor();
    }
    
    public void SetOccupied(SoldierAgent soldier)
    {
        isOccupied = true;
        occupyingSoldier = soldier;
        UpdateColor();
    }
    
    public void SetFree()
    {
        isOccupied = false;
        occupyingSoldier = null;
        UpdateColor();
    }
    
    public void SetHovered(bool hovered)
    {
        if (isFixed) return;
        isHovered = hovered;
        UpdateColor();
    }
    
    void UpdateColor()
    {
        if (objectMaterial == null) return;
        
        if (isFixed)
        {
            objectMaterial.color = normalColor;
        }
        if (!isPlaceable)
        {
            objectMaterial.color = normalColor;
        }
        else if (isHovered && !isPlaced)
        {
            objectMaterial.color = hoverColor;
        }
        else if (isOccupied)
        {
            objectMaterial.color = occupiedColor;
        }
        else if (isPlaced)
        {
            objectMaterial.color = placedColor;
        }
        else
        {
            objectMaterial.color = normalColor;
        }
    }
}