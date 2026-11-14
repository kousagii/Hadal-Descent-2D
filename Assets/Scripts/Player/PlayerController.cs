using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [HideInInspector] public float baseMoveSpeed;

    [Header("Player Boundaries")]
    [Tooltip("The lowest X and Y the player can go")]
    public Vector2 minBounds;
    [Tooltip("The highest X and Y the player can go")]
    public Vector2 maxBounds;

    [Header("Attack Settings")]
    public float attackDuration = 0.9f;
    public float attackCooldown = 0.5f;

    [Header("Attack Audio")]
    [Tooltip("Sound played once when a tail-whip attack starts.")]
    public AudioClip attackSound;
    [Range(0f, 1f)] public float attackVolume = 1f;

    [Header("References")]
    [Tooltip("Drag the child TailWhipHitbox GameObject here")]
    public GameObject tailWhipHitbox;
    
    // Reference to the Animator
    private Animator animator; 

    private AudioSource attackAudioSource;
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;
    private bool isAttacking = false;

    // Camouflage skill: set by Camouflage.cs, checked during attack to break stealth
    [HideInInspector] public bool isCamouflaged = false;

    private void Start()
    {
        attackAudioSource = GetComponent<AudioSource>();
        if (attackAudioSource == null)
        {
            attackAudioSource = gameObject.AddComponent<AudioSource>();
            attackAudioSource.playOnAwake = false;
            attackAudioSource.spatialBlend = 0f;
        }

        baseMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Grab the Animator component attached to the player
        animator = GetComponent<Animator>(); 

        // Set gravity to 0 so the fish floats naturally
        rb.gravityScale = 0f;
        
        // Freeze Z rotation so the fish doesn't spin wildly
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; 

        // Ensure the hitbox is turned off when the game starts
        if (tailWhipHitbox != null)
        {
            tailWhipHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        // --- 1. HANDLE ATTACK INPUT ---
        bool attackPressed = false;
        
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame) attackPressed = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) attackPressed = true;

        if (attackPressed && !isAttacking)
        {
            StartCoroutine(PerformTailWhip());
        }

        // --- 2. HANDLE MOVEMENT INPUT ---
        movement = Vector2.zero;
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) movement.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) movement.y -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) movement.x += 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) movement.x -= 1;
        }

        // --- 3. FLIP SPRITE & HITBOX ---
        if (movement.x > 0)
        {
            spriteRenderer.flipX = false; // Face Right
            
            // Shift the hitbox to the LEFT (the tail side when facing right)
            if (tailWhipHitbox != null)
            {
                Vector3 hitboxPos = tailWhipHitbox.transform.localPosition;
                hitboxPos.x = -Mathf.Abs(hitboxPos.x); 
                tailWhipHitbox.transform.localPosition = hitboxPos;
            }
        }
        else if (movement.x < 0)
        {
            spriteRenderer.flipX = true;  // Face Left
            
            // Shift the hitbox to the RIGHT (the tail side when facing left)
            if (tailWhipHitbox != null)
            {
                Vector3 hitboxPos = tailWhipHitbox.transform.localPosition;
                hitboxPos.x = Mathf.Abs(hitboxPos.x); 
                tailWhipHitbox.transform.localPosition = hitboxPos;
            }
        }
    }

    private void FixedUpdate()
    {
        // Move the fish using physics
        rb.linearVelocity = movement.normalized * moveSpeed; 

        // Clamp the player's position so they cannot leave the boundaries
        Vector2 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
        
        // Apply the clamped position back to the player
        rb.position = clampedPosition;
    }

    private IEnumerator PerformTailWhip()
    {
        isAttacking = true;

        if (attackSound != null && attackAudioSource != null)
        {
            attackAudioSource.PlayOneShot(attackSound, attackVolume);
        }

        // If camouflaged, attacking breaks the stealth
        if (isCamouflaged)
        {
            Camouflage camo = GetComponent<Camouflage>();
            if (camo != null) camo.BreakCamo();
        }

        // Tell the Animator to play the Attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Turn on the red hitbox sprite and collider
        if (tailWhipHitbox != null) tailWhipHitbox.SetActive(true);

        // Keep it active for a fraction of a second (the "whip" motion)
        yield return new WaitForSeconds(attackDuration);

        // Turn it back off
        if (tailWhipHitbox != null) tailWhipHitbox.SetActive(false);

        // Wait for the remainder of the cooldown before allowing another attack
        yield return new WaitForSeconds(attackCooldown);
        
        isAttacking = false;
    }
}