using UnityEngine;
using System.Collections;

public class TunaJousting : MonoBehaviour
{
    [Header("Jousting Settings")]
    public float swimSpeed = 15f; // Very fast!
    [Tooltip("If true, the Tuna will track the player's vertical position before charging")]
    public bool trackPlayerY = true;
    public float trackingSpeed = 5f;
    
    [Header("Despawn Settings")]
    [Tooltip("How far off-screen before it resets its position")]
    public float despawnX = 35f;

    [Header("Respawn & Warning")]
    public float respawnCooldown = 3f;
    public float warningDuration = 1.5f;
    [Tooltip("Drag your WarningSign PREFAB here (from the Project folder, not the Hierarchy)")]
    public GameObject warningSignPrefab;
    [Tooltip("How close the player needs to be vertically to trigger the warning and charge")]
    public float triggerDistanceY = 10f;

    [Header("Randomization")]
    [Tooltip("If true, the Tuna will randomly pick left or right side when it spawns")]
    public bool randomizeSpawnSide = true;

    private int directionMultiplier;
    private bool isCharging = false;
    private Transform player;
    private float startX;
    private GameObject warningSign; // Runtime instance, created from the prefab

    private void Start()
    {
        // 50/50 chance to flip the side it attacks from
        if (randomizeSpawnSide && Random.value > 0.5f)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        directionMultiplier = transform.localScale.x < 0 ? 1 : -1;
        startX = -despawnX * directionMultiplier;
        
        // Find the player
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        // Instantiate this Tuna's own personal warning sign at the scene root
        // so it isn't destroyed when the Tuna is destroyed
        if (warningSignPrefab != null)
        {
            warningSign = Instantiate(warningSignPrefab, Vector3.zero, Quaternion.identity);
            warningSign.SetActive(false);
        }
        
        // Move tuna to its starting point off screen
        transform.position = new Vector3(startX, transform.position.y, transform.position.z);
        
        StartCoroutine(JoustRoutine());
    }

    private void OnDestroy()
    {
        // Clean up this Tuna's warning sign when it is defeated
        if (warningSign != null)
        {
            Destroy(warningSign);
        }
    }

    private IEnumerator JoustRoutine()
    {
        // Loop continuously until the Tuna is destroyed (e.g. defeated by player)
        while (true) 
        {
            // Wait until the player is nearby vertically
            while (player != null && Mathf.Abs(player.position.y - transform.position.y) > triggerDistanceY)
            {
                yield return new WaitForSeconds(0.5f);
            }

            // 1. Show Warning Sign at the edge of the screen
            if (warningSign != null)
            {
                warningSign.SetActive(true);
                float timer = 0f;
                
                while (timer < warningDuration)
                {
                    EnemyStatusEffects status = GetComponent<EnemyStatusEffects>();
                    if (status != null && status.isBlinded)
                    {
                        // Pause the warning until the blind wears off!
                        yield return new WaitUntil(() => !status.isBlinded);
                    }

                    // Optionally track the player's Y position while the warning is flashing
                    if (trackPlayerY && player != null)
                    {
                        float newY = Mathf.Lerp(transform.position.y, player.position.y, Time.deltaTime * trackingSpeed);
                        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                    }

                    if (Camera.main != null)
                    {
                        // Calculate the edge of the screen based on the camera
                        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
                        float buffer = 1.5f; // Keep it slightly away from the exact edge so it's fully visible
                        
                        // If directionMultiplier is 1 (starts left, moves right), show on the left edge.
                        // If directionMultiplier is -1 (starts right, moves left), show on the right edge.
                        float edgeX = Camera.main.transform.position.x - (directionMultiplier * (halfWidth - buffer));
                        
                        warningSign.transform.position = new Vector3(edgeX, transform.position.y, transform.position.z - 1f);
                    }
                    else
                    {
                        Debug.LogWarning("Tuna Warning Sign cannot find Camera.main! Make sure your main camera is tagged as 'MainCamera' in the Inspector.");
                    }
                    
                    timer += Time.deltaTime;
                    yield return null; // Wait for the next frame
                }
                
                warningSign.SetActive(false);
            }
            else
            {
                // Fallback just in case you haven't assigned a warning sign yet
                yield return new WaitForSeconds(warningDuration);
            }

            // 2. Charge
            isCharging = true;
            while (isCharging)
            {
                EnemyStatusEffects status = GetComponent<EnemyStatusEffects>();
                if (status != null && status.isBlinded)
                {
                    // Pause mid-air!
                    yield return new WaitUntil(() => !status.isBlinded);
                }

                transform.Translate(Vector2.right * directionMultiplier * swimSpeed * Time.deltaTime);
                
                // Once it reaches far off screen, stop charging
                if (Mathf.Abs(transform.position.x) > despawnX)
                {
                    isCharging = false; 
                }
                yield return null;
            }

            // 3. Reset Position to starting side and wait for cooldown
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
            yield return new WaitForSeconds(respawnCooldown);
        }
    }

    public bool IsWarningActive()
    {
        return warningSign != null && warningSign.activeSelf;
    }
}