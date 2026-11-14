using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    [Header("Enemy Identity")]
    [Tooltip("Unique string identifier for tracking kill counts (e.g., 'Shark', 'Tuna')")]
    public string speciesID; // use later for the bestiary    

    [Header("Health Settings")]
    public float maxHP = 100f;
    public float currentHP { get; private set; }

    [Header("UI References")]
    [Tooltip("Drag the Slider or Image from the World Space Canvas on top of the enemy's head")]
    public Image hpFillImage;
    [Tooltip("Optional: The entire canvas group to hide/show the health bar")]
    public GameObject healthBarUI;

    [Header("Rewards")]
    public float baseExpReward = 30f;

    [Header("Dynamic Difficulty Scaling")]
    public bool scaleWithLevel = true;
    [Tooltip("e.g. 0.15 = +15% HP per level above 1")]
    public float hpScaleFactor = 0.15f;
    [Tooltip("e.g. 0.1 = +10% EXP per level above 1")]
    public float expScaleFactor = 0.10f;

    private PlayerStats playerStats;

    private void Start()
    {
        // Find the player in the scene to reward them later
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerStats = playerObj.GetComponent<PlayerStats>();
            
            // Apply Dynamic Scaling based on Player Level
            if (scaleWithLevel && playerStats != null)
            {
                int levelDiff = Mathf.Max(0, playerStats.currentLevel - 1);
                maxHP = maxHP * (1f + (levelDiff * hpScaleFactor));
                baseExpReward = baseExpReward * (1f + (levelDiff * expScaleFactor));
            }
        }
        
        currentHP = maxHP;

        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = damage;

        // Apply Echolocation Mark multiplier if present
        EnemyStatusEffects status = GetComponent<EnemyStatusEffects>();
        if (status != null && status.isMarked)
        {
            finalDamage *= status.markDamageMultiplier;
        }

        currentHP -= finalDamage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        // Show health bar if it was hidden
        if (healthBarUI != null) healthBarUI.SetActive(true);

        UpdateHealthUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = currentHP / maxHP;
        }
    }

    private void Die()
    {
        if (playerStats != null)
        {
            // Calculate dynamic diminishing returns for repeated kills using PlayerStats tracking
            float calculatedExp = playerStats.GetDiminishingCalculatedEXP(speciesID, baseExpReward);
            playerStats.GainEXP(calculatedExp);
            
            Debug.Log($"Defeated {speciesID}! Awarded {calculatedExp} EXP (Base was {baseExpReward}).");
        }

        // Add to Bestiary globally
        if (BestiaryUI.Instance != null && !string.IsNullOrEmpty(speciesID))
        {
            bool isNewDiscovery = BestiaryUI.Instance.UnlockSpecies(speciesID);
            
            // If this is the FIRST time we defeated this enemy type, show the popup!
            if (isNewDiscovery && NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification("Species added to bestiary!", speciesID);
            }
        }

        // Destroy this enemy object instance
        Destroy(gameObject);
    }
}