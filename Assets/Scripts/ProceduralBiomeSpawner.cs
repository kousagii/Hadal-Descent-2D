using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 1. We create a custom data container to hold all the settings for ONE species
[System.Serializable]
public class BiomeEntityData
{
    public string speciesName; // Just a label to keep your Inspector organized
    public GameObject prefab;
    
    [Tooltip("How many of this specific species to spawn")]
    public int spawnCount = 1;

    [Tooltip("Minimum distance between this spawn and other existing enemies/fish to prevent clumping")]
    public float minimumSpacing = 5f;

    [Tooltip("How long to wait before spawning a replacement when one is killed")]
    public float respawnCooldown = 10f;
    
    [Header("Custom Roaming Boundaries")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Dynamic Difficulty Scaling")]
    public bool scaleWithLevel = false;
    [Tooltip("How many EXTRA spawns are added per player level. (e.g. 0.5 = 1 extra every 2 levels)")]
    public float extraSpawnsPerLevel = 0.5f;
    [Tooltip("How much the respawn cooldown decreases per player level")]
    public float cooldownReductionPerLevel = 0.5f;
    public float minRespawnCooldown = 2f;
    
    // Internal tracking
    [HideInInspector] public int activeRoutines = 0;
}

public class ProceduralBiomeSpawner : MonoBehaviour
{
    [Header("Biome Ecosystem")]
    [Tooltip("Add your passive fish here and define their specific territories")]
    public List<BiomeEntityData> discoverableSpeciesList;
    
    [Tooltip("Add your hostile fish here and define their hunting grounds")]
    public List<BiomeEntityData> enemySpeciesList;

    private PlayerStats playerStats;

    private void Start()
    {
        // Try to find the player stats to hook into level ups
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerStats = playerObj.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.OnLevelUp += HandleLevelUp;
            }
        }

        GenerateBiome();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnLevelUp -= HandleLevelUp;
        }
    }

    private void HandleLevelUp()
    {
        // Re-evaluate spawn counts when player levels up to maintain flow state
        GenerateBiome();
    }

    public void GenerateBiome()
    {
        // 1. Spawn/Update Discoverable Species
        foreach (BiomeEntityData entityData in discoverableSpeciesList)
        {
            UpdateEntityGroup(entityData, true);
        }

        // 2. Spawn/Update Enemies
        foreach (BiomeEntityData entityData in enemySpeciesList)
        {
            UpdateEntityGroup(entityData, false);
        }
    }

    private void UpdateEntityGroup(BiomeEntityData data, bool isPassive)
    {
        if (data.prefab == null) return;

        int targetSpawnCount = data.spawnCount;

        // Apply Level Scaling
        if (data.scaleWithLevel && playerStats != null)
        {
            int levelDifference = Mathf.Max(0, playerStats.currentLevel - 1);
            targetSpawnCount += Mathf.FloorToInt(levelDifference * data.extraSpawnsPerLevel);
        }

        // Add missing routines if the target increased
        while (data.activeRoutines < targetSpawnCount)
        {
            data.activeRoutines++;
            StartCoroutine(MaintainEntity(data, isPassive));
        }
    }

    private IEnumerator MaintainEntity(BiomeEntityData data, bool isPassive)
    {
        while (true)
        {
            // 1. Spawn a new entity to fill this slot
            GameObject newEntity = SpawnSingleEntity(data, isPassive);

            // 2. Check if this is a formation (parent with EnemyStats children)
            //    e.g., "Tuna School Formation" has multiple child tunas with EnemyStats
            EnemyStats[] childEnemies = newEntity.GetComponentsInChildren<EnemyStats>();

            if (childEnemies.Length > 1 || 
                (childEnemies.Length == 1 && childEnemies[0].gameObject != newEntity))
            {
                // --- FORMATION MODE ---
                // Wait until ALL children with EnemyStats are destroyed
                while (newEntity != null)
                {
                    bool allChildrenDead = true;
                    
                    // Re-check children each tick (destroyed children return null)
                    EnemyStats[] remaining = newEntity.GetComponentsInChildren<EnemyStats>();
                    if (remaining.Length > 0)
                    {
                        allChildrenDead = false;
                    }

                    if (allChildrenDead)
                    {
                        // All children are dead — destroy the empty parent shell
                        Destroy(newEntity);
                        break;
                    }

                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                // --- SINGLE ENTITY MODE (original behaviour) ---
                // Wait until the entity itself is destroyed
                while (newEntity != null)
                {
                    yield return new WaitForSeconds(1f);
                }
            }

            // 3. The entity/formation was defeated! Calculate dynamic cooldown and wait
            float currentCooldown = data.respawnCooldown;
            if (data.scaleWithLevel && playerStats != null)
            {
                int levelDiff = Mathf.Max(0, playerStats.currentLevel - 1);
                currentCooldown -= (levelDiff * data.cooldownReductionPerLevel);
                currentCooldown = Mathf.Max(currentCooldown, data.minRespawnCooldown);
            }

            yield return new WaitForSeconds(currentCooldown);
            
            // 4. The loop restarts and it spawns a brand new replacement!
        }
    }

    private GameObject SpawnSingleEntity(BiomeEntityData data, bool isPassive)
    {
        Vector2 spawnPosition = Vector2.zero;
        
        // Try up to 15 times to find a position that isn't clumping with others
        for (int attempt = 0; attempt < 15; attempt++)
        {
            float randomX = Random.Range(data.minBounds.x, data.maxBounds.x);
            float randomY = Random.Range(data.minBounds.y, data.maxBounds.y);
            spawnPosition = new Vector2(randomX, randomY);

            // If spacing is 0, we don't care about clumping
            if (data.minimumSpacing <= 0f) break;

            // Check if there's anything nearby
            Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(spawnPosition, data.minimumSpacing);
            bool isTooClose = false;

            foreach (var hit in nearbyObjects)
            {
                // If it hit another fish/enemy, it's too close!
                if (hit.GetComponent<EnemyStats>() != null || hit.GetComponentInParent<EnemyStats>() != null ||
                    hit.GetComponent<WanderAI>() != null || hit.GetComponentInParent<WanderAI>() != null)
                {
                    isTooClose = true;
                    break;
                }
            }

            // If we found a clear spot, break out of the loop and use this position
            if (!isTooClose)
            {
                break;
            }
        }

        // Instantiate the entity at the chosen (or fallback) position
        GameObject newEntity = Instantiate(data.prefab, spawnPosition, Quaternion.identity);

        // Pass the custom boundaries into the movement scripts so they stay in their zone
        if (isPassive)
        {
            WanderAI wanderScript = newEntity.GetComponent<WanderAI>();
            if (wanderScript != null)
            {
                wanderScript.minBounds = data.minBounds;
                wanderScript.maxBounds = data.maxBounds;
            }
        }
        else
        {
            SharkAI sharkScript = newEntity.GetComponent<SharkAI>();
            if (sharkScript != null)
            {
                sharkScript.minBounds = data.minBounds;
                sharkScript.maxBounds = data.maxBounds;
                sharkScript.useBounds = true;
            }
            
            MahiMahiAI mahiScript = newEntity.GetComponent<MahiMahiAI>();
            if (mahiScript != null)
            {
                mahiScript.minBounds = data.minBounds;
                mahiScript.maxBounds = data.maxBounds;
                mahiScript.useBounds = true;
            }

            BoxJellyfishAI jellyScript = newEntity.GetComponent<BoxJellyfishAI>();
            if (jellyScript != null)
            {
                jellyScript.minBounds = data.minBounds;
                jellyScript.maxBounds = data.maxBounds;
                jellyScript.useBounds = true;
            }

            TunaJousting tunaScript = newEntity.GetComponent<TunaJousting>();
        }

        return newEntity;
    }
}