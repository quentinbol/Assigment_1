using UnityEngine;

public class ExposureTimer : MonoBehaviour
{
    public float maxExposureTime = 15f;

    public bool visualFeedback = true;
    [Header("debug")]
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
        if (!enabled) return;
        if (isDead) return;
        bool isInCover = soldier != null && soldier.IsInCover();
        if (isInCover)
        {
            if (currentExposureTime > 0f)
            {
                currentExposureTime = 0f;
                UpdateVisualFeedback();
            }
        }
        else
        {
            currentExposureTime += Time.deltaTime;
            if (visualFeedback)
            {
                UpdateVisualFeedback();
            }
            if (currentExposureTime >= maxExposureTime)
            {
                Die();
            }
        }
    }

    void UpdateVisualFeedback()
    {
        if (soldierRenderer == null || materialBlock == null) return;
        float exposureRatio = currentExposureTime / maxExposureTime;
        if (exposureRatio <= 0.01f)
        {
            materialBlock.SetColor("_Color", originalColor);
        }
        else if (exposureRatio < 0.5f)
        {
            Color warningColor = Color.Lerp(originalColor, Color.yellow, exposureRatio * 2f);
            materialBlock.SetColor("_Color", warningColor);
        }
        else if (exposureRatio < 0.8f)
        {
            Color dangerColor = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (exposureRatio - 0.5f) * 3.33f);
            materialBlock.SetColor("_Color", dangerColor);
        }
        else
        {
            Color criticalColor = Color.Lerp(new Color(1f, 0.5f, 0f), Color.red, (exposureRatio - 0.8f) * 5f);
            materialBlock.SetColor("_Color", criticalColor);
        }
        
        soldierRenderer.SetPropertyBlock(materialBlock);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (soldierRenderer != null && materialBlock != null)
        {
            materialBlock.SetColor("_Color", Color.black);
            soldierRenderer.SetPropertyBlock(materialBlock);
        }
        if (soldier != null)
        {
            soldier.ReleaseCover();
        }
        gameObject.SetActive(false);
    }

    public float GetCurrentExposureTime()
    {
        return currentExposureTime;
    }

    public float GetExposureRatio()
    {
        return Mathf.Clamp01(currentExposureTime / maxExposureTime);
    }

    public float GetTimeUntilDeath()
    {
        return Mathf.Max(0f, maxExposureTime - currentExposureTime);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void ResetTimer()
    {
        currentExposureTime = 0f;
        UpdateVisualFeedback();
    }

    public void Revive()
    {
        isDead = false;
        currentExposureTime = 0f;
        gameObject.SetActive(true);
        UpdateVisualFeedback();
    }
}