using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Player Boundaries")]
    [Tooltip("The lowest X and Y the player can go")]
    public Vector2 minBounds;
    [Tooltip("The highest X and Y the player can go")]
    public Vector2 maxBounds;

    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set gravity to 0 so the fish floats naturally
        rb.gravityScale = 0f;
        
        // Freeze Z rotation so the fish doesn't spin wildly
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; 
    }

    private void Update()
    {
        // Reset movement every frame
        movement = Vector2.zero;

        // Check for WASD or Arrow Keys using the New Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) movement.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) movement.y -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) movement.x += 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) movement.x -= 1;
        }

        // Flip the sprite to face the direction of movement
        if (movement.x > 0)
        {
            spriteRenderer.flipX = false; // Face Right
        }
        else if (movement.x < 0)
        {
            spriteRenderer.flipX = true;  // Face Left
        }
    }

    private void FixedUpdate()
    {
        // 1. Move the fish using physics
        rb.linearVelocity = movement.normalized * moveSpeed; 

        // 2. Clamp the player's position so they cannot leave the boundaries
        Vector2 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
        
        // Apply the clamped position back to the player
        rb.position = clampedPosition;
    }
}