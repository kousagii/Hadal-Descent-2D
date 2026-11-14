using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Required for Dictionary

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHP = 100f;
    public float currentHP = 100f;
    public Image hpFillImage;

    [Header("Experience Settings")]
    public int currentLevel = 1; 
    private float maxEXP = 50f;
    public float currentEXP = 0f;
    public Image expFillImage;

    [Header("UI Text References")]
    [Tooltip("Text showing 'Lv. X' beside the XP bar at the top left")]
    public TMP_Text levelText;
    [Tooltip("Text centered inside the HP bar showing 'current / max'")]
    public TMP_Text hpBarText;
    [Tooltip("Text centered inside the XP bar showing 'current / max'")]
    public TMP_Text expBarText;

    // Event that fires when the player levels up — SkillManager listens to this
    public System.Action OnLevelUp;

    // Dictionary tracking: Key = EnemyID string, Value = Number of times killed
    private Dictionary<string, int> killTracker = new Dictionary<string, int>();

    private void Start()
    {
        maxEXP = currentLevel * 50;
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        // Don't take damage if already dead
        if (currentHP <= 0) return;

        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.TriggerGameOver(currentLevel);
            }
            else
            {
                Debug.LogWarning("Player died but no GameOverManager found in scene!");
            }
        }
        UpdateUI();
    }

    public void GainEXP(float amount)
    {
        currentEXP += amount;
        
        if (currentEXP >= maxEXP)
        {
            // Carry over excess experience
            float excessExp = currentEXP - maxEXP;
            
            // Level up
            currentLevel++;
            maxHP += 20;       
            currentHP = maxHP; // Full heal on level up
            maxEXP = currentLevel * 50; // Cumulative formula: Lv1=50, Lv2=100, Lv3=150...
            
            currentEXP = 0;

            // Fire event for SkillManager to show skill selection
            OnLevelUp?.Invoke();

            // Recursively handle excess XP
            if (excessExp > 0) GainEXP(excessExp);
        }
        UpdateUI();
    }

    /// Processes diminish rates for experience rewards based on absolute kill frequency.
    public float GetDiminishingCalculatedEXP(string enemyID, float baseExp)
    {
        if (string.IsNullOrEmpty(enemyID)) return baseExp;

        // Register or increment kill log
        if (!killTracker.ContainsKey(enemyID))
        {
            killTracker.Add(enemyID, 1);
            return baseExp; // Full experience on the first kill
        }
        
        killTracker[enemyID]++;
        int killCount = killTracker[enemyID];

        // Diminishing returns mathematical scale: Reduces reward value over progressive kills
        // 2nd kill = 80%, 3rd = 60%, 4th = 40%, capped at a minimum of 20% experience return
        float reductionFactor = Mathf.Max(0.2f, 1f - ((killCount - 1) * 0.2f));
        
        return baseExp * reductionFactor;
    }

    private void UpdateUI()
    {
        if (hpFillImage != null) 
            hpFillImage.fillAmount = currentHP / maxHP;
            
        if (expFillImage != null) 
            expFillImage.fillAmount = currentEXP / maxEXP;

        // Text overlays
        if (levelText != null)
            levelText.text = $"Lv. {currentLevel}";

        if (hpBarText != null)
            hpBarText.text = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";

        if (expBarText != null)
            expBarText.text = $"{Mathf.CeilToInt(currentEXP)} / {Mathf.CeilToInt(maxEXP)}";
    }
}