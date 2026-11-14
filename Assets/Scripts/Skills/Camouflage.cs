using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Camouflage — Active Skill
/// Makes the player completely undetectable by enemies for a duration.
/// Attacking during camo breaks it immediately.
/// Higher levels increase duration and grant movement speed buffs.
/// </summary>
public class Camouflage : MonoBehaviour
{
    private int level = 1;
    private bool synergiesUnlocked = false;
    private bool isActive = false;

    private PlayerController playerController;
    private float originalMoveSpeed;
    private bool speedBoosted = false;

    // Track modified enemies so we can restore them
    private Dictionary<SharkAI, float> originalSharkRadii = new Dictionary<SharkAI, float>();

    private Coroutine camoCoroutine;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();
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
    /// Get the duration in seconds for the current level.
    /// Level 1: 5s, Level 2: 8s, Level 3: 13s, Level 4: 13s, Level 5: 13s
    /// </summary>
    private float GetDuration()
    {
        float duration = 5f; // Level 1: 5 seconds base
        if (level >= 2) duration += 3f; // Level 2: +3 seconds (8s total)
        if (level >= 3) duration += 5f; // Level 3: +5 seconds (13s total)
        return duration;
    }

    /// <summary>
    /// Get the movement speed multiplier for the current level.
    /// </summary>
    private float GetSpeedMultiplier()
    {
        if (level >= 5) return 1.5f; // Level 5: +50% speed
        if (level >= 4) return 1.2f; // Level 4: +20% speed
        return 1f; // No speed buff below level 4
    }

    public float GetCooldown()
    {
        return 10f;
    }

    public bool IsActive => isActive;

    /// <summary>
    /// Activate camouflage. Called by SkillManager.
    /// </summary>
    public void Activate()
    {
        if (isActive) return;
        camoCoroutine = StartCoroutine(CamoRoutine());
    }

    /// <summary>
    /// Called when the player attacks during camouflage — breaks camo immediately.
    /// </summary>
    public void BreakCamo()
    {
        if (!isActive) return;

        if (camoCoroutine != null)
        {
            StopCoroutine(camoCoroutine);
            camoCoroutine = null;
        }

        EndCamo();
    }

    private IEnumerator CamoRoutine()
    {
        isActive = true;

        if (playerController != null)
            playerController.isCamouflaged = true;

        // Set ALL shark detection radii to 0 — enemies can't detect player at all
        SharkAI[] sharks = FindObjectsByType<SharkAI>(FindObjectsSortMode.None);
        foreach (var shark in sharks)
        {
            if (shark == null) continue;
            originalSharkRadii[shark] = shark.detectionRadius;
            shark.detectionRadius = 0f;
        }

        // Level 4+: Apply speed boost
        speedBoosted = false;
        float speedMult = GetSpeedMultiplier();
        if (speedMult > 1f && playerController != null)
        {
            originalMoveSpeed = playerController.moveSpeed;
            playerController.moveSpeed *= speedMult;
            speedBoosted = true;
        }

        Debug.Log($"Camouflage activated! Duration: {GetDuration()}s, Speed: x{speedMult}");

        // Wait for duration
        yield return new WaitForSeconds(GetDuration());

        EndCamo();
    }

    private void EndCamo()
    {
        isActive = false;

        if (playerController != null)
            playerController.isCamouflaged = false;

        // Restore all shark detection radii
        foreach (var pair in originalSharkRadii)
        {
            if (pair.Key != null)
                pair.Key.detectionRadius = pair.Value;
        }
        originalSharkRadii.Clear();

        // Restore speed if it was boosted
        if (speedBoosted && playerController != null)
        {
            playerController.moveSpeed = originalMoveSpeed;
            speedBoosted = false;
        }

        Debug.Log("Camouflage ended.");

        // Tell SkillManager to hide the visual overlay
        if (SkillManager.Instance != null)
            SkillManager.Instance.OnActiveSkillEnded("Camouflage");
    }
}
