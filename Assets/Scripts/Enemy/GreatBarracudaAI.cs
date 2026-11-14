using UnityEngine;
using System.Collections;

public class GreatBarracudaAI : MonoBehaviour
{
    [Header("Barracuda Stats")]
    public float lungeSpeed = 20f;
    public float detectionRadius = 10f;
    
    [Header("Attack Settings")]
    public float warningTime = 0.5f;
    public float lungeDuration = 0.5f;
    public float attackCooldown = 2f;
    
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    [Tooltip("Drag the AttackWarningArrow here")]
    public GameObject warningArrow;


    [Header("Drag the healthbar canvas here")]
    public Transform healthBarCanvas;
    public float healthBarHeightOffset = 1.8f;

    private Transform player;
    private bool isAttacking = false;
    private bool isOnCooldown = false;
    private Color originalColor;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
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

        if (player == null || isAttacking || isOnCooldown) return;

        // Check if player is camouflaged
        PlayerController pController = player.GetComponent<PlayerController>();
        if (pController != null && pController.isCamouflaged) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            StartCoroutine(LungeAttack());
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


    private void AimHeadAtTarget(Vector3 direction)
    {
        spriteRenderer.flipX = false;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += 180f; // Offset for left-facing native sprite (matches SharkAI)
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (direction.x < 0) spriteRenderer.flipY = false; 
        else spriteRenderer.flipY = true; 
    }

    private IEnumerator LungeAttack()
    {
        isAttacking = true;

        if (spriteRenderer != null) spriteRenderer.color = Color.yellow; // Shiny warning
        
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
        
        float timer = 0f;

        // The actual lunge
        while (timer < lungeDuration)
        {
            EnemyStatusEffects status = GetComponent<EnemyStatusEffects>();
            if (status != null && status.isBlinded) 
            {
                // Pause mid-air!
                yield return new WaitUntil(() => !status.isBlinded);
            }

            transform.position += directionToPlayer * lungeSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
        
        // Cooldown before it can lunge again
        isOnCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnCooldown = false;
    }
}
