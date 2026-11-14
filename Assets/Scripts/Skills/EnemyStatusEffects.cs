using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles status effects (blind, poison) on enemies.
/// Add this component to enemy prefabs (Shark, Tuna, etc.).
/// Uses lazy initialization so it works even when added via AddComponent at runtime.
/// </summary>
public class EnemyStatusEffects : MonoBehaviour
{
    [Header("Status Flags")]
    public bool isBlinded = false;
    public bool isMarked = false;
    public float markDamageMultiplier = 1f;

    // Poison tracking
    private List<Coroutine> poisonCoroutines = new List<Coroutine>();
    private int currentPoisonStacks = 0;

    private EnemyStats _enemyStats;
    private EnemyStats enemyStats
    {
        get
        {
            if (_enemyStats == null)
                _enemyStats = GetComponent<EnemyStats>();
            return _enemyStats;
        }
    }

    /// <summary>
    /// Blinds this enemy for the given duration. AI scripts check isBlinded to pause behaviour.
    /// </summary>
    public void Blind(float duration)
    {
        if (isBlinded) return; // Already blinded, don't stack blind
        StartCoroutine(BlindRoutine(duration));
    }

    private IEnumerator BlindRoutine(float duration)
    {
        isBlinded = true;
        yield return new WaitForSeconds(duration);
        isBlinded = false;
    }

    /// <summary>
    /// Marks the enemy, causing them to take bonus damage.
    /// </summary>
    public void Mark(float duration, float damageMultiplier)
    {
        markDamageMultiplier = damageMultiplier;
        if (!isMarked) 
            StartCoroutine(MarkRoutine(duration));
    }

    private IEnumerator MarkRoutine(float duration)
    {
        isMarked = true;
        yield return new WaitForSeconds(duration);
        isMarked = false;
        markDamageMultiplier = 1f;
    }

    /// <summary>
    /// Applies a poison DoT stack to this enemy.
    /// </summary>
    /// <param name="dps">Damage per second</param>
    /// <param name="duration">How long the poison lasts</param>
    /// <param name="maxStacks">Maximum number of concurrent poison stacks allowed</param>
    public void ApplyPoison(float dps, float duration, int maxStacks)
    {
        if (currentPoisonStacks >= maxStacks) return;

        Coroutine c = StartCoroutine(PoisonRoutine(dps, duration));
        poisonCoroutines.Add(c);
    }

    private IEnumerator PoisonRoutine(float dps, float duration)
    {
        currentPoisonStacks++;
        float elapsed = 0f;
        float tickInterval = 0.5f; // Deal damage every half second

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;

            if (enemyStats != null)
            {
                enemyStats.TakeDamage(dps * tickInterval);
            }
        }

        currentPoisonStacks--;
    }
}
