using UnityEngine;
using System.Collections;

public class PlayerParalysis : MonoBehaviour
{
    [Tooltip("How long the paralysis lasts")]
    public float paralysisDuration = 3f;
    [Tooltip("Percentage to reduce speed by. 0.7 = 70% reduction")]
    public float slowFactor = 0.7f;
    
    // Optional: How long before this can paralyze again?
    public float cooldown = 1f;
    private bool canParalyze = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryParalyze(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryParalyze(other);
    }

    private void TryParalyze(Collider2D other)
    {
        if (canParalyze && other.CompareTag("Player"))
        {
            PlayerController pController = other.GetComponent<PlayerController>();
            if (pController != null)
            {
                // Run the coroutine ON the player so it doesn't get cancelled if the VenomZone is destroyed!
                pController.StartCoroutine(ParalyzeRoutine(pController));
                StartCoroutine(CooldownRoutine());
            }
        }
    }

    private IEnumerator ParalyzeRoutine(PlayerController pController)
    {
        // Don't apply if already paralyzed to prevent infinite slowing loops
        if (pController.moveSpeed < pController.baseMoveSpeed * 0.9f)
        {
            yield break;
        }

        // Calculate the slowed speed based on their TRUE base speed
        float slowedSpeed = pController.baseMoveSpeed * (1f - slowFactor);
        
        // Apply slowed speed
        pController.moveSpeed = slowedSpeed;
        
        // Wait for the duration
        yield return new WaitForSeconds(paralysisDuration);
        
        // Always restore safely back to the true base speed
        pController.moveSpeed = pController.baseMoveSpeed;
    }

    private IEnumerator CooldownRoutine()
    {
        canParalyze = false;
        yield return new WaitForSeconds(cooldown);
        canParalyze = true;
    }
}
