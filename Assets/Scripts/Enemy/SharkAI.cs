using UnityEngine;
using System.Collections;

public class SharkAI : MonoBehaviour
{
    [Header("Shark Stats")]
    public float patrolSpeed = 2f;
    public float lungeSpeed = 12f;
    public float detectionRadius = 8f;
    
    [Header("Procedural Boundaries")]
    public bool useBounds = false; // Enabled automatically by the Spawner
    public Vector2 minBounds;
    public Vector2 maxBounds;
    
    [Header("Attack Settings")]
    public float warningTime = 0.8f;
    public SpriteRenderer spriteRenderer;

    [Header("UI Elements")]
    [Tooltip("Drag the AttackWarningArrow here")]
    public GameObject warningArrow; 
    [Tooltip("Drag the OverheadHealthBarCanvas here")]
    public Transform healthBarCanvas;
    
    [Header("Health Bar Positioning")]
    public float healthBarHeightOffset = 1.8f;

    private Transform player;
    private bool isAttacking = false;
    private Vector2 moveDirection = Vector2.left; 
    private Color originalColor;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (warningArrow != null) warningArrow.SetActive(false); 

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

        if (player == null || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            StartCoroutine(LungeAttack());
        }
        else
        {
            // Move horizontally while patrolling
            transform.Translate(moveDirection * patrolSpeed * Time.deltaTime, Space.World);
            HandlePatrolFlipping();
            CheckBoundaries(); // NEW: Check if we hit the wall
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

    // --- NEW: Boundary Bouncing Logic ---
    private void CheckBoundaries()
    {
        if (!useBounds) return;

        if (transform.position.x <= minBounds.x && moveDirection == Vector2.left)
        {
            moveDirection = Vector2.right;
        }
        else if (transform.position.x >= maxBounds.x && moveDirection == Vector2.right)
        {
            moveDirection = Vector2.left;
        }
    }

    private void HandlePatrolFlipping()
    {
        transform.rotation = Quaternion.identity;
        spriteRenderer.flipY = false;
        spriteRenderer.flipX = (moveDirection.x > 0);
    }

    private void AimHeadAtTarget(Vector3 direction)
    {
        spriteRenderer.flipX = false;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += 180f; // Offset for left-facing native sprite
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (direction.x < 0) spriteRenderer.flipY = false; 
        else spriteRenderer.flipY = true; 
    }

    private IEnumerator LungeAttack()
    {
        isAttacking = true;

        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        AimHeadAtTarget(directionToPlayer);

        if (warningArrow != null)
        {
            warningArrow.SetActive(true);
            float arrowAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            warningArrow.transform.rotation = Quaternion.Euler(0, 0, arrowAngle);
        }

        yield return new WaitForSeconds(warningTime);

        if (spriteRenderer != null) spriteRenderer.color = originalColor; 
        if (warningArrow != null) warningArrow.SetActive(false); 
        
        float lungeDuration = 0.5f;
        float timer = 0f;

        while (timer < lungeDuration)
        {
            transform.position += directionToPlayer * lungeSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        
        // Resume patrol away from the lunge target
        moveDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;
        
        isAttacking = false;
    }
}