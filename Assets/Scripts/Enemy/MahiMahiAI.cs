using UnityEngine;
using System.Collections;

public class MahiMahiAI : MonoBehaviour
{
    [Header("Mahi-mahi Stats")]
    public float patrolSpeed = 4f;
    public float diveSpeed = 15f;
    public float returnSpeed = 6f;
    [Tooltip("How long it pauses at the bottom before returning")]
    public float recoveryDuration = 1.5f;
    
    [Header("Detection Settings")]
    [Tooltip("How close the player needs to be on the X axis to trigger a dive")]
    public float attackRangeX = 5f;
    [Tooltip("Distance below the max bounds it considers the surface")]
    public float surfaceOffset = 1f;

    [Header("Procedural Boundaries")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Drag the healthbar canvas here")]
    public Transform healthBarCanvas;
    public float healthBarHeightOffset = 1.8f;

    private Transform player;
    private bool isAttacking = false;
    private bool isReturning = false;
    private Vector2 moveDirection = Vector2.left;
    private float surfaceY;
    private EnemyHitbox enemyHitbox;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        enemyHitbox = GetComponentInChildren<EnemyHitbox>();

        // Start near the surface
        if (useBounds) surfaceY = maxBounds.y - surfaceOffset;
        else surfaceY = transform.position.y; // Fallback if no 
        
        if (healthBarCanvas == null) 
        {
            healthBarCanvas = transform.Find("OverheadHealthBarCanvas");
        }
    }

    private void Update()
    {
        // Skip all AI while blinded by a skill effect
        EnemyStatusEffects status = GetComponent<EnemyStatusEffects>();
        if (status != null && status.isBlinded) return;

        if (player == null) return;

        if (isAttacking) return;

        if (isReturning)
        {
            ReturnToSurface();
            return;
        }

        // Patrol logic at surface
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, surfaceY, Time.deltaTime * 2f);
        transform.position = pos;

        transform.Translate(moveDirection * patrolSpeed * Time.deltaTime, Space.World);
        HandlePatrolFlipping();
        CheckBoundaries();

        // Check if player is camouflaged
        PlayerController pController = player.GetComponent<PlayerController>();
        if (pController != null && pController.isCamouflaged) return;

        // Check if player is directly below
        float distX = Mathf.Abs(player.position.x - transform.position.x);
        float distY = transform.position.y - player.position.y;

        if (distX <= attackRangeX && distY > 0)
        {
            StartCoroutine(DiveBombAttack());
        }
    }

    private void LateUpdate()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.rotation = Quaternion.identity;
            Vector3 currentScale = healthBarCanvas.localScale;
            healthBarCanvas.localScale = new Vector3(Mathf.Abs(currentScale.x), Mathf.Abs(currentScale.y), 1f);
            healthBarCanvas.position = transform.position + Vector3.up * healthBarHeightOffset;
        }
    }

    private void CheckBoundaries()
    {
        if (!useBounds) return;

        if (transform.position.x <= minBounds.x && moveDirection == Vector2.left)
            moveDirection = Vector2.right;
        else if (transform.position.x >= maxBounds.x && moveDirection == Vector2.right)
            moveDirection = Vector2.left;
    }

    private void HandlePatrolFlipping()
    {
        transform.rotation = Quaternion.identity;
        spriteRenderer.flipY = false;
        spriteRenderer.flipX = (moveDirection.x > 0);
    }

    private void AimAtTarget(Vector3 direction)
    {
        spriteRenderer.flipX = false;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += 180f; // Offset for left-facing native sprite
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (direction.x < 0) spriteRenderer.flipY = false; 
        else spriteRenderer.flipY = true; 
    }

    private IEnumerator DiveBombAttack()
    {
        isAttacking = true;
        
        // Slight hover/pause before diving
        yield return new WaitForSeconds(0.3f);

        Vector3 targetPos = player.position;
        Vector3 startPos = transform.position;

        // Dive downwards
        float t = 0;
        while (t < 1f)
        {
            EnemyStatusEffects status = GetComponent<EnemyStatusEffects>();
            if (status != null && status.isBlinded) 
            {
                // Pause mid-air!
                yield return new WaitUntil(() => !status.isBlinded);
            }

            t += Time.deltaTime * (diveSpeed / Vector2.Distance(startPos, targetPos));
            
            // Quadratic ease-in for a gravity-like acceleration
            float easeT = t * t; 
            
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, easeT);
            AimAtTarget((newPos - transform.position).normalized);
            transform.position = newPos;

            // Break early if we reached the bottom or past the player
            if (transform.position.y <= targetPos.y) break;

            yield return null;
        }

        // Disable damage so player can attack it safely
        if (enemyHitbox != null) enemyHitbox.enabled = false;

        // Wait at the bottom
        yield return new WaitForSeconds(recoveryDuration);

        // Re-enable damage for the return trip (or next attack)
        if (enemyHitbox != null) enemyHitbox.enabled = true;

        isAttacking = false;
        isReturning = true;
    }

    private void ReturnToSurface()
    {
        // Calculate point on the surface to return to
        Vector3 targetSurfacePos = new Vector3(transform.position.x, surfaceY, transform.position.z);
        Vector3 direction = (targetSurfacePos - transform.position).normalized;

        AimAtTarget(direction);

        transform.position = Vector3.MoveTowards(transform.position, targetSurfacePos, returnSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetSurfacePos) < 0.5f)
        {
            isReturning = false;
            // Resume patrolling in a random direction
            moveDirection = Random.value > 0.5f ? Vector2.right : Vector2.left;
        }
    }
}
