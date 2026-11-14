using UnityEngine;
using System.Collections;

public class RedSnapperAI : MonoBehaviour
{
    [Header("Territory Settings")]
    public float territoryRadius = 8f;
    public float chaseSpeed = 6f;
    public float returnSpeed = 3f;
    
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Visuals")]
    [Tooltip("Drag a child object representing the territory here. It will be detached at start so it stays at the anchor point.")]
    public Transform territoryVisual;

    private Transform player;
    private Vector3 anchorPoint;
    private bool isChasing = false;
    private bool isReturning = false;
    private EnemyHitbox enemyHitbox;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Set the anchor point to wherever it was placed in the level
        anchorPoint = transform.position;

        // If a visual is provided, detach it so it stays at the anchor point instead of following the fish
        if (territoryVisual != null)
        {
            territoryVisual.SetParent(null);
            territoryVisual.position = anchorPoint;
        }

        enemyHitbox = GetComponentInChildren<EnemyHitbox>();
    }

    private void OnDestroy()
    {
        // Clean up the detached visual if the Red Snapper is destroyed
        if (territoryVisual != null)
        {
            Destroy(territoryVisual.gameObject);
        }
    }

    private void Update()
    {
        // Skip all AI while blinded by a skill effect
        EnemyStatusEffects status = GetComponent<EnemyStatusEffects>();
        if (status != null && status.isBlinded) return;

        if (player == null) return;
        
        // We need to check the distance from the original anchor point instead.
        float distanceFromAnchorToPlayer = Vector2.Distance(anchorPoint, player.position);

        // Check if player is camouflaged
        bool playerVisible = true;
        PlayerController pController = player.GetComponent<PlayerController>();
        if (pController != null && pController.isCamouflaged) playerVisible = false;

        // If it is currently returning home, it ignores the player until it arrives!
        // This is the "Vulnerable Retreat" window.
        if (isReturning)
        {
            ReturnToAnchor();
            // Check if we arrived
            if (Vector3.Distance(transform.position, anchorPoint) <= 0.1f)
            {
                isReturning = false; // We are home, can chase again
            }
            return;
        }

        if (distanceFromAnchorToPlayer <= territoryRadius && playerVisible)
        {
            isChasing = true;
            if (enemyHitbox != null) enemyHitbox.enabled = true; // Danger when chasing
            ChasePlayer();
        }
        else if (isChasing)
        {
            // Player just escaped! Start the vulnerable retreat
            isChasing = false;
            isReturning = true;
            if (enemyHitbox != null) enemyHitbox.enabled = false; // Vulnerable when retreating
            ReturnToAnchor();
        }
        else
        {
            // Idle at anchor
            if (enemyHitbox != null) enemyHitbox.enabled = false;
            ReturnToAnchor();
        }
    }

    private void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        AimAtTarget(direction);
        
        transform.position += direction * chaseSpeed * Time.deltaTime;
    }

    private void ReturnToAnchor()
    {
        if (Vector3.Distance(transform.position, anchorPoint) > 0.1f)
        {
            Vector3 direction = (anchorPoint - transform.position).normalized;
            AimAtTarget(direction);
            
            transform.position = Vector3.MoveTowards(transform.position, anchorPoint, returnSpeed * Time.deltaTime);
        }
    }

    private void AimAtTarget(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;

        spriteRenderer.flipX = false;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += 180f; // Offset for left-facing native sprite
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (direction.x < 0) spriteRenderer.flipY = false; 
        else spriteRenderer.flipY = true; 
    }

    // Optional: Draw the territory in the editor for easy level design
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? anchorPoint : transform.position, territoryRadius);
    }
}
