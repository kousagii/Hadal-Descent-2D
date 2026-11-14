using UnityEngine;

/// <summary>
/// Chemosynthesis — Passive Skill
/// Regenerates HP over time. Higher levels increase the regen rate.
/// </summary>
public class Chemosynthesis : MonoBehaviour
{
    private int level = 1;
    private bool synergiesUnlocked = false;
    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();
    }

    /// <summary>
    /// Called by SkillManager when the skill is acquired or leveled up.
    /// </summary>
    public void SetLevel(int newLevel)
    {
        level = newLevel;
        if (level >= 5) synergiesUnlocked = true;
    }

    /// <summary>
    /// Returns the regen rate in HP per second for the current level.
    /// </summary>
    public float GetRegenRate()
    {
        switch (level)
        {
            case 1: return 0.5f;
            case 2: return 1.0f;
            case 3: return 1.5f;
            case 4: return 2.0f;
            case 5: return 2.5f;
            default: return 0.5f;
        }
    }

    private void Update()
    {
        if (playerStats == null) return;

        // Only regen if not at full HP
        if (playerStats.currentHP < playerStats.maxHP)
        {
            float healAmount = GetRegenRate() * Time.deltaTime;
            // Use TakeDamage with a negative value won't work since it clamps to 0.
            // Instead, directly modify HP and call the public heal path.
            playerStats.currentHP += healAmount;
            playerStats.currentHP = Mathf.Min(playerStats.currentHP, playerStats.maxHP);

            // Force the UI to refresh by calling TakeDamage(0) — this triggers UpdateUI()
            playerStats.TakeDamage(0f);
        }
    }
}
