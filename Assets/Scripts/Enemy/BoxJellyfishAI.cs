using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyStats))]
public class BoxJellyfishAI : MonoBehaviour
{
    [Header("Jellyfish Settings")]
    public float pulseForce = 2f;
    public float sinkSpeed = 0.5f;
    public float pulseInterval = 2f;
    public float chaseSpeed = 1.5f;

    [Header("Venom Settings")]
    [Tooltip("Prefab for the Venom AoE. If null, one will be created via code.")]
    public GameObject venomZonePrefab;
    public float venomDropInterval = 1.5f;
    public float venomDuration = 4f;

    [Header("Procedural Boundaries")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private EnemyStats stats;
    private Transform player;
    private bool isProvoked = false;
    private float nextPulseTime;
    private float nextVenomTime;
    private Rigidbody2D rb;

    private void Start()
    {
        stats = GetComponent<EnemyStats>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        nextPulseTime = Time.time + pulseInterval;
    }

    private void Update()
    {
        // Skip all AI while blinded by a skill effect
        EnemyStatusEffects statusEffects = GetComponent<EnemyStatusEffects>();
        if (statusEffects != null && statusEffects.isBlinded) return;

        // Check if we took damage -> become provoked
        if (!isProvoked && stats.currentHP < stats.maxHP)
        {
            isProvoked = true;
        }

        if (!isProvoked)
        {
            PassiveDrift();
        }
        else
        {
            ActiveChaseAndVenom();
        }
    }

    private void PassiveDrift()
    {
        // Slowly sink
        transform.position += Vector3.down * sinkSpeed * Time.deltaTime;

        // Bounce up if hitting the bottom boundary
        if (useBounds && transform.position.y <= minBounds.y)
        {
            Vector3 pos = transform.position;
            pos.y = minBounds.y;
            transform.position = pos;
            
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * pulseForce, ForceMode2D.Impulse);
            nextPulseTime = Time.time + pulseInterval;
        }
        // Periodically pulse upwards
        else if (Time.time >= nextPulseTime)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * pulseForce, ForceMode2D.Impulse);
            nextPulseTime = Time.time + pulseInterval;
        }
    }

    private void ActiveChaseAndVenom()
    {
        if (player == null) return;

        // Move towards player slowly
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * chaseSpeed * Time.deltaTime;

        // Drop venom periodically
        if (Time.time >= nextVenomTime)
        {
            DropVenom();
            nextVenomTime = Time.time + venomDropInterval;
        }
    }

    private void DropVenom()
    {
        GameObject venom;
        
        if (venomZonePrefab != null)
        {
            venom = Instantiate(venomZonePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // Fallback: Create a temporary invisible venom zone
            venom = new GameObject("VenomZone");
            venom.transform.position = transform.position;
            
            CircleCollider2D col = venom.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.5f;

            // Deal heavy damage
            EnemyHitbox hitbox = venom.AddComponent<EnemyHitbox>();
            hitbox.damageAmount = 25f; // "Heavy damage"
            
            // Paralyze player
            PlayerParalysis paralysis = venom.AddComponent<PlayerParalysis>();
            paralysis.paralysisDuration = 3f;
            paralysis.slowFactor = 0.7f;
        }

        // Destroy venom after duration
        Destroy(venom, venomDuration);
    }
}
