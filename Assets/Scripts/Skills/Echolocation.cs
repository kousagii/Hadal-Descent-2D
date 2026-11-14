using UnityEngine;
using System.Collections;

/// <summary>
/// Echolocation (Radar Pulse) — Active Skill
/// Emits a 360-degree radar pulse.
/// Reveals enemies/discoverables on minimap and marks enemies for bonus damage.
/// </summary>
public class Echolocation : MonoBehaviour
{
    [Header("Radar Pulse Settings")]
    public float baseRadius = 30f;
    public float baseCooldown = 15f;
    public float markDuration = 5f;
    
    [Header("Visuals")]
    public float pulseDuration = 3f;

    private int level = 1;
    
    // Dynamic Stats
    private float currentRadius;
    private float currentCooldown;
    private float markDamageMultiplier;
    private bool causesStun;

    private void Awake()
    {
        RecalculateStats();
    }

    public void SetLevel(int newLevel)
    {
        level = newLevel;
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        // Level 1 Defaults
        currentRadius = baseRadius;
        currentCooldown = baseCooldown;
        markDamageMultiplier = 1.15f; // 15% bonus damage
        causesStun = false;

        // Level 2: Cooldown reduced by 15%
        if (level >= 2) currentCooldown = baseCooldown * 0.85f;
        
        // Level 3: Pulse radius increased by 30%
        if (level >= 3) currentRadius = baseRadius * 1.30f;
        
        // Level 4: Bonus damage against marked enemies increased to 30%
        if (level >= 4) markDamageMultiplier = 1.30f;
        
        // Level 5: Stuns enemies for 1.5 seconds
        if (level >= 5) causesStun = true;
    }

    public float GetCooldown()
    {
        return currentCooldown; 
    }

    public void Activate()
    {
        StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        Vector2 origin = transform.position;
        
        // 1. Perform a CircleCast to find everything in the radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, currentRadius);
        
        int markedCount = 0;
        int revealedCount = 0;

        foreach (var hit in hits)
        {
            // Reveal Minimap Icons
            MinimapIcon icon = hit.GetComponent<MinimapIcon>();
            if (icon == null) icon = hit.GetComponentInChildren<MinimapIcon>();
            if (icon == null) icon = hit.GetComponentInParent<MinimapIcon>();
            
            // If the collider and the icon are on completely different child objects (siblings), check the root!
            if (icon == null) icon = hit.transform.root.GetComponentInChildren<MinimapIcon>();

            if (icon != null)
            {
                icon.Reveal(markDuration);
                revealedCount++;
            }

            // Mark & Stun Enemies
            EnemyStats enemyStats = hit.GetComponent<EnemyStats>();
            if (enemyStats == null) enemyStats = hit.GetComponentInParent<EnemyStats>();

            if (enemyStats != null)
            {
                EnemyStatusEffects status = enemyStats.GetComponent<EnemyStatusEffects>();
                if (status == null) status = enemyStats.gameObject.AddComponent<EnemyStatusEffects>();

                status.Mark(markDuration, markDamageMultiplier);
                
                if (causesStun)
                {
                    // Using blind as a stun as it stops their movement!
                    status.Blind(1.5f);
                }
                markedCount++;
            }
        }

        Debug.Log($"Echolocation Pulse! Revealed {revealedCount} icons. Marked {markedCount} enemies.");

        // 2. Simple Visual Feedback (Draw a circle for debugging)
        // In the future, you could instantiate a cool expanding ring particle effect here!
        float timer = 0f;
        while (timer < pulseDuration)
        {
            // Expanding debug circle
            float currentDrawRadius = Mathf.Lerp(0, currentRadius, timer / pulseDuration);
            DrawDebugCircle(origin, currentDrawRadius, Color.cyan);
            
            timer += Time.deltaTime;
            yield return null;
        }

        // Tell SkillManager we finished
        if (SkillManager.Instance != null)
            SkillManager.Instance.OnActiveSkillEnded("Echolocation");
    }

    private void DrawDebugCircle(Vector2 center, float radius, Color color)
    {
        int segments = 36;
        float angle = 0f;
        Vector2 lastPos = center + new Vector2(radius, 0);

        for (int i = 1; i <= segments; i++)
        {
            angle += 360f / segments;
            Vector2 newPos = center + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            Debug.DrawLine(lastPos, newPos, color, 0.05f);
            lastPos = newPos;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, baseRadius);
    }
}
