using UnityEngine;

/// <summary>
/// Venom Glands — Passive Skill
/// When the player hits an enemy with a tail whip, poison is applied.
/// Higher levels increase DPS, duration, and max stacks.
/// </summary>
public class VenomGlands : MonoBehaviour
{
    private int level = 1;
    private bool synergiesUnlocked = false;

    /// <summary>
    /// Called by SkillManager when the skill is acquired or leveled up.
    /// </summary>
    public void SetLevel(int newLevel)
    {
        level = newLevel;
        if (level >= 5) synergiesUnlocked = true;
    }

    /// <summary>
    /// Get the poison DPS for the current level.
    /// </summary>
    public float GetDPS()
    {
        switch (level)
        {
            case 1: return 5f;
            case 2: return 5f;
            case 3: return 8f;
            case 4: return 8f;
            case 5: return 10f;
            default: return 5f;
        }
    }

    /// <summary>
    /// Get the poison duration in seconds for the current level.
    /// </summary>
    public float GetDuration()
    {
        float baseDuration = 5f;
        if (level >= 2) baseDuration += 5f; // Level 2: +5 second
        return baseDuration;
    }

    /// <summary>
    /// Get the maximum number of poison stacks for the current level.
    /// </summary>
    public int GetMaxStacks()
    {
        if (level >= 4) return 2; // Level 4: Can stack twice
        return 1;
    }
}
