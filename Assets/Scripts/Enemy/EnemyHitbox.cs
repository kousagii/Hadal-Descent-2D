using UnityEngine;
using System.Collections;

public class EnemyHitbox : MonoBehaviour
{
    [Tooltip("How much HP the player loses when touching this hitbox")]
    public float damageAmount = 15f;
    
    [Tooltip("How long before this hitbox can damage the player again")]
    public float damageCooldown = 1f;

    [Header("Dynamic Difficulty Scaling")]
    public bool scaleWithLevel = true;
    [Tooltip("e.g. 0.1 = +10% Damage per level above 1")]
    public float damageScaleFactor = 0.1f;

    private bool canDamage = true;

    private void Start()
    {
        if (scaleWithLevel)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                PlayerStats stats = playerObj.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    int levelDiff = Mathf.Max(0, stats.currentLevel - 1);
                    damageAmount = damageAmount * (1f + (levelDiff * damageScaleFactor));
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    // This ensures that if the player stays inside the hitbox, 
    // they continue to take damage every time the cooldown finishes!
    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void TryDealDamage(Collider2D other)
    {
        if (canDamage && other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damageAmount);
                StartCoroutine(DamageCooldownRoutine());
            }
        }
    }

    private IEnumerator DamageCooldownRoutine()
    {
        canDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDamage = true;
    }
}
