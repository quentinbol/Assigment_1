using UnityEngine;

/// <summary>
/// Gère le timer d'exposition d'un soldat
/// Si le soldat n'est pas en cover pendant maxExposureTime secondes → MORT
/// </summary>
public class ExposureTimer : MonoBehaviour
{
    [Header("Exposure Settings")]
    [Tooltip("Temps max hors cover avant de mourir (secondes)")]
    public float maxExposureTime = 15f;
    
    [Header("Visual Feedback")]
    [Tooltip("Changer la couleur du soldat selon l'exposition")]
    public bool visualFeedback = true;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    
    private SoldierAgent soldier;
    private float currentExposureTime = 0f;
    private bool isDead = false;
    private Renderer soldierRenderer;
    private MaterialPropertyBlock materialBlock;
    private Color originalColor;
    
    void Awake()
    {
        soldier = GetComponent<SoldierAgent>();
        soldierRenderer = GetComponent<Renderer>();
        
        if (soldierRenderer != null)
        {
            materialBlock = new MaterialPropertyBlock();
            soldierRenderer.GetPropertyBlock(materialBlock);
            originalColor = materialBlock.GetColor("_Color");
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Vérifier si le soldat est en cover
        bool isInCover = soldier != null && soldier.IsInCover();
        
        if (isInCover)
        {
            // En cover → Reset le timer
            if (currentExposureTime > 0f)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[{gameObject.name}] En cover → Timer reset (était à {currentExposureTime:F1}s)");
                }
                currentExposureTime = 0f;
                UpdateVisualFeedback();
            }
        }
        else
        {
            // Hors cover → Timer augmente
            currentExposureTime += Time.deltaTime;
            
            // Logs périodiques
            if (showDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[{gameObject.name}] Exposition: {currentExposureTime:F1}s / {maxExposureTime}s");
            }
            
            // Visual feedback
            if (visualFeedback)
            {
                UpdateVisualFeedback();
            }
            
            // MORT si timer dépassé
            if (currentExposureTime >= maxExposureTime)
            {
                Die();
            }
        }
    }
    
    /// <summary>
    /// Mise à jour du feedback visuel (couleur selon exposition)
    /// </summary>
    void UpdateVisualFeedback()
    {
        if (soldierRenderer == null || materialBlock == null) return;
        
        float exposureRatio = currentExposureTime / maxExposureTime;
        
        if (exposureRatio <= 0.01f)
        {
            // En cover → Couleur normale
            materialBlock.SetColor("_Color", originalColor);
        }
        else if (exposureRatio < 0.5f)
        {
            // Exposition faible → Jaune
            Color warningColor = Color.Lerp(originalColor, Color.yellow, exposureRatio * 2f);
            materialBlock.SetColor("_Color", warningColor);
        }
        else if (exposureRatio < 0.8f)
        {
            // Exposition moyenne → Orange
            Color dangerColor = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (exposureRatio - 0.5f) * 3.33f);
            materialBlock.SetColor("_Color", dangerColor);
        }
        else
        {
            // Exposition critique → Rouge
            Color criticalColor = Color.Lerp(new Color(1f, 0.5f, 0f), Color.red, (exposureRatio - 0.8f) * 5f);
            materialBlock.SetColor("_Color", criticalColor);
        }
        
        soldierRenderer.SetPropertyBlock(materialBlock);
    }
    
    /// <summary>
    /// Mort du soldat
    /// </summary>
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        Debug.LogWarning($"[{gameObject.name}] ☠️ MORT par exposition ({currentExposureTime:F1}s)");
        
        // Visuel de mort
        if (soldierRenderer != null && materialBlock != null)
        {
            materialBlock.SetColor("_Color", Color.black);
            soldierRenderer.SetPropertyBlock(materialBlock);
        }
        
        // Libérer le cover si occupé
        if (soldier != null)
        {
            soldier.ReleaseCover();
        }
        
        // Désactiver le soldat
        // Option 1 : Désactiver complètement
        gameObject.SetActive(false);
        
        // Option 2 : Désactiver seulement les composants de mouvement
        // MovementController movement = GetComponent<MovementController>();
        // if (movement != null) movement.enabled = false;
        
        // SoldierStateMachine stateMachine = GetComponent<SoldierStateMachine>();
        // if (stateMachine != null) stateMachine.enabled = false;
    }
    
    /// <summary>
    /// Obtenir le temps d'exposition actuel
    /// </summary>
    public float GetCurrentExposureTime()
    {
        return currentExposureTime;
    }
    
    /// <summary>
    /// Obtenir le ratio d'exposition (0 à 1)
    /// </summary>
    public float GetExposureRatio()
    {
        return Mathf.Clamp01(currentExposureTime / maxExposureTime);
    }
    
    /// <summary>
    /// Temps restant avant la mort
    /// </summary>
    public float GetTimeUntilDeath()
    {
        return Mathf.Max(0f, maxExposureTime - currentExposureTime);
    }
    
    /// <summary>
    /// Le soldat est-il mort ?
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
    
    /// <summary>
    /// Reset le timer (pour tests ou réinitialisation)
    /// </summary>
    public void ResetTimer()
    {
        currentExposureTime = 0f;
        UpdateVisualFeedback();
        
        if (showDebugLogs)
        {
            Debug.Log($"[{gameObject.name}] Timer d'exposition reset");
        }
    }
    
    /// <summary>
    /// Ressusciter le soldat (pour tests)
    /// </summary>
    public void Revive()
    {
        isDead = false;
        currentExposureTime = 0f;
        gameObject.SetActive(true);
        UpdateVisualFeedback();
        
        if (showDebugLogs)
        {
            Debug.Log($"[{gameObject.name}] Ressuscité !");
        }
    }
    
    // Debug GUI
    void OnGUI()
    {
        if (!showDebugLogs || isDead) return;
        
        // Afficher le timer au-dessus du soldat
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        
        if (screenPos.z > 0)
        {
            float timeRemaining = GetTimeUntilDeath();
            Color guiColor = Color.white;
            
            if (currentExposureTime > 0f)
            {
                float ratio = GetExposureRatio();
                
                if (ratio < 0.5f)
                    guiColor = Color.yellow;
                else if (ratio < 0.8f)
                    guiColor = new Color(1f, 0.5f, 0f); // Orange
                else
                    guiColor = Color.red;
                
                GUI.color = guiColor;
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 30, 100, 20), 
                    $"{timeRemaining:F1}s", 
                    new GUIStyle() { 
                        alignment = TextAnchor.MiddleCenter, 
                        fontSize = 12, 
                        fontStyle = FontStyle.Bold,
                        normal = new GUIStyleState() { textColor = guiColor }
                    });
            }
        }
    }
}