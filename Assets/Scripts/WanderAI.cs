using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class WanderAI : MonoBehaviour
{
    [Header("Wander Settings")]
    public float swimSpeed = 2f;
    [Tooltip("How long the fish waits before picking a new destination")]
    public Vector2 waitTimeRange = new Vector2(1f, 3f);

    [Header("Procedural Boundaries")]
    [Tooltip("The bottom-left corner of the roaming area")]
    public Vector2 minBounds;
    [Tooltip("The top-right corner of the roaming area")]
    public Vector2 maxBounds;

    [Header("Sprite Settings")]
    [Tooltip("Check this if your original art asset faces left instead of right")]
    public bool facesLeftNatively = true; 

    [Header("Steering Settings")]
    public float maxForce = 5f;
    [Tooltip("How far away this fish wants to stay from others")]
    public float separationRadius = 1.5f;
    [Tooltip("How strongly this fish pushes away from others")]
    public float separationWeight = 1.5f;
    [Tooltip("How strongly this fish pulls towards its target")]
    public float seekWeight = 1f;
    [Tooltip("How strongly this fish avoids going out of bounds")]
    public float boundsWeight = 2f;

    private Vector2 targetPosition;
    private SpriteRenderer spriteRenderer;
    private bool isWaiting = false;

    // Craig Reynolds Physics variables
    private Vector2 velocity;
    private Vector2 acceleration;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        PickNewDestination();
    }

    private void Update()
    {
        if (isWaiting) 
        {
            // Decelerate smoothly when waiting
            velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 3f);
            transform.position += (Vector3)velocity * Time.deltaTime;
            return;
        }

        // 1. Calculate Forces
        Vector2 seekForce = Seek(targetPosition) * seekWeight;
        Vector2 separateForce = Separate() * separationWeight;
        Vector2 boundsForce = AvoidBounds() * boundsWeight;

        // 2. Apply Forces
        ApplyForce(seekForce);
        ApplyForce(separateForce);
        ApplyForce(boundsForce);

        // 3. Integration
        velocity += acceleration * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, swimSpeed);
        transform.position += (Vector3)velocity * Time.deltaTime;

        // Reset acceleration for next frame
        acceleration = Vector2.zero;

        // 4. Handle Sprite Flipping based on Velocity direction
        if (velocity.x > 0.1f) 
            spriteRenderer.flipX = facesLeftNatively; // Moving Right
        else if (velocity.x < -0.1f) 
            spriteRenderer.flipX = !facesLeftNatively; // Moving Left

        // Check if we reached the destination (use a larger radius for steering so it doesn't overshoot infinitely)
        if (Vector2.Distance(transform.position, targetPosition) < 0.5f)
        {
            StartCoroutine(WaitAndPickNewSpot());
        }
    }

    private void ApplyForce(Vector2 force)
    {
        // acceleration = force / mass. Assuming mass = 1.
        acceleration += force;
    }

    private Vector2 Seek(Vector2 target)
    {
        Vector2 desiredVelocity = (target - (Vector2)transform.position).normalized * swimSpeed;
        Vector2 steering = desiredVelocity - velocity;
        return Vector2.ClampMagnitude(steering, maxForce);
    }

    private Vector2 Separate()
    {
        Vector2 steer = Vector2.zero;
        int count = 0;
        
        // Find all colliders within separation radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        foreach (Collider2D col in colliders)
        {
            // Ignore self and ensure it has a WanderAI so we separate from other wanderers
            if (col.gameObject != gameObject && col.GetComponent<WanderAI>() != null)
            {
                Vector2 diff = (Vector2)transform.position - (Vector2)col.transform.position;
                float dist = diff.magnitude;
                
                if (dist > 0)
                {
                    diff.Normalize();
                    diff /= dist; // Weight by distance (closer = stronger push)
                    steer += diff;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            steer /= count;
        }

        if (steer.magnitude > 0)
        {
            // Implement Reynolds: Steering = Desired - Velocity
            steer = steer.normalized * swimSpeed;
            steer -= velocity;
            steer = Vector2.ClampMagnitude(steer, maxForce);
        }

        return steer;
    }

    private Vector2 AvoidBounds()
    {
        Vector2 desired = Vector2.zero;
        bool outOfBounds = false;
        
        // Push inwards if crossing bounds
        if (transform.position.x < minBounds.x) { desired.x = swimSpeed; outOfBounds = true; }
        else if (transform.position.x > maxBounds.x) { desired.x = -swimSpeed; outOfBounds = true; }

        if (transform.position.y < minBounds.y) { desired.y = swimSpeed; outOfBounds = true; }
        else if (transform.position.y > maxBounds.y) { desired.y = -swimSpeed; outOfBounds = true; }

        if (outOfBounds)
        {
            // Calculate a steering force towards the inside of the bounds
            desired = desired.normalized * swimSpeed;
            Vector2 steer = desired - velocity;
            return Vector2.ClampMagnitude(steer, maxForce);
        }

        return Vector2.zero;
    }

    private void PickNewDestination()
    {
        float randomX = Random.Range(minBounds.x, maxBounds.x);
        float randomY = Random.Range(minBounds.y, maxBounds.y);
        targetPosition = new Vector2(randomX, randomY);
    }

    private IEnumerator WaitAndPickNewSpot()
    {
        isWaiting = true;
        
        // Pick a random wait time to make the movement feel organic, not robotic
        float waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
        yield return new WaitForSeconds(waitTime);
        
        PickNewDestination();
        isWaiting = false;
    }
}