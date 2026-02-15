using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ObjectPalette : MonoBehaviour, IPointerClickHandler
{
    [Header("Object Info")]
    public CoverObject coverPrefab; // Le prefab à spawner
    public Sprite icon; // Icône de l'objet
    
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Image backgroundImage;
    
    [Header("Selection Visual")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    
    private int remainingCount;
    private PlacementManager placementManager;
    private bool isSelected = false;
    
    public void Initialize(CoverObject prefab, int count, PlacementManager manager)
    {
        coverPrefab = prefab;
        remainingCount = count;
        placementManager = manager;
        
        UpdateUI();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (remainingCount > 0)
        {
            placementManager.SelectObjectType(this);
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
    }
    
    public CoverObject SpawnObject()
    {
        if (remainingCount <= 0) return null;
        
        CoverObject newCover = Instantiate(coverPrefab);
        newCover.isPlaceable = true;
        newCover.isPlaced = false;
        
        remainingCount--;
        UpdateUI();
        
        return newCover;
    }
    
    public void ReturnObject()
    {
        remainingCount++;
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = remainingCount.ToString();
        }
        
        // Griser si plus d'objets disponibles
        if (iconImage != null)
        {
            Color imgColor = iconImage.color;
            imgColor.a = remainingCount > 0 ? 1f : 0.3f;
            iconImage.color = imgColor;
        }
    }
    
    public int GetRemainingCount()
    {
        return remainingCount;
    }
}