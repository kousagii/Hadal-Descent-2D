using UnityEngine;
using System.Collections;

/// <summary>
/// Bioluminescent Flare — Active Skill
/// Blinds ALL enemies currently visible on the player's camera.
/// Blinded enemies completely stop (AI paused via EnemyStatusEffects.isBlinded).
/// Higher levels reduce cooldown, increase blind duration, and add shockwave damage.
/// </summary>
public class BioluminescentFlare : MonoBehaviour
{
    [Header("Base Settings")]
    public float baseCooldown = 8f;
    public float baseBlindDuration = 2f;
    public float shockwaveDamage = 15f;

    private int level = 1;
    private bool synergiesUnlocked = false;

    // Calculated values (updated when level changes)
    private float currentCooldown;
    private float currentBlindDuration;

    private void Awake()
    {
        RecalculateStats();
    }

    /// <summary>
    /// Called by SkillManager when the skill is acquired or leveled up.
    /// </summary>
    public void SetLevel(int newLevel)
    {
        level = newLevel;
        if (level >= 5) synergiesUnlocked = true;
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        currentCooldown = baseCooldown;
        currentBlindDuration = baseBlindDuration;

        // Level 2: Cooldown reduced by 10%
        if (level >= 2) currentCooldown = baseCooldown * 0.9f;

        // Level 4: Blind duration increased to 3 seconds
        if (level >= 4) currentBlindDuration = 3f;
    }

    public float GetCooldown() => currentCooldown;

    /// <summary>
    /// Activate the flare. Called by SkillManager.
    /// </summary> 
    public void Activate()
    {
        StartCoroutine(FlareRoutine());
    }

    private IEnumerator FlareRoutine()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("BioluminescentFlare: No main camera found!");
            yield break;
        }

        // Find ALL enemies in the scene tagged "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int blindedCount = 0;

        foreach (var enemyObj in enemies)
        {
            if (enemyObj == null) continue;

            // Check if this enemy is visible on the player's camera
            Vector3 viewportPos = cam.WorldToViewportPoint(enemyObj.transform.position);
            bool isOnScreen = viewportPos.x >= 0f && viewportPos.x <= 1f 
                           && viewportPos.y >= 0f && viewportPos.y <= 1f 
                           && viewportPos.z > 0f; // z > 0 means in front of camera

            if (!isOnScreen) continue;

            // Get or add EnemyStatusEffects so we can blind them
            EnemyStatusEffects status = enemyObj.GetComponent<EnemyStatusEffects>();
            if (status == null) status = enemyObj.GetComponentInChildren<EnemyStatusEffects>();
            if (status == null) status = enemyObj.AddComponent<EnemyStatusEffects>();

            status.Blind(currentBlindDuration);
            blindedCount++;

            // Level 5: Also deal shockwave damage
            if (level >= 5)
            {
                EnemyStats enemyStats = enemyObj.GetComponent<EnemyStats>();
                if (enemyStats == null) enemyStats = enemyObj.GetComponentInChildren<EnemyStats>();

                if (enemyStats != null)
                {
                    enemyStats.TakeDamage(shockwaveDamage);
                }
            }
        }

        Debug.Log($"Bioluminescent Flare! Blinded {blindedCount} on-screen enemies for {currentBlindDuration}s.");
        yield return null;
    }
}
