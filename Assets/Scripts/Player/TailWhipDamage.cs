using UnityEngine;

public class TailWhipDamage : MonoBehaviour
{
    [Tooltip("How much damage this attack does")]
    public float damageAmount = 25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Find the stats script on the main body of the enemy:
            EnemyStats enemy = other.GetComponent<EnemyStats>() ?? other.GetComponentInParent<EnemyStats>();
            
            // If it's missing on the immediate object, look upwards in case it's a sub-collider
            if (enemy == null) enemy = other.GetComponentInParent<EnemyStats>();

            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log($"Tail whipped {other.gameObject.name} for {damageAmount} damage!");

                // Apply Venom Glands poison if the player has the skill
                if (SkillManager.Instance != null)
                {
                    VenomGlands venom = SkillManager.Instance.GetSkillBehaviour<VenomGlands>();
                    if (venom != null)
                    {
                        EnemyStatusEffects statusEffects = enemy.GetComponent<EnemyStatusEffects>();
                        if (statusEffects == null) statusEffects = enemy.gameObject.AddComponent<EnemyStatusEffects>();
                        statusEffects.ApplyPoison(venom.GetDPS(), venom.GetDuration(), venom.GetMaxStacks());
                        Debug.Log($"Applied poison: {venom.GetDPS()} DPS for {venom.GetDuration()}s!");
                    }
                }
            }
        }
    }
}