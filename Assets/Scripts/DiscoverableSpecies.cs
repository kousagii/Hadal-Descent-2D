using UnityEngine;

public class DiscoverableSpecies : MonoBehaviour
{
    [Header("Discovery Rewards")]
    public string speciesID; // We will use this later for your Bestiary Journal!
    public float expReward = 20f;
    private bool hasBeenDiscovered = false;

    [Header("Fleeing Behavior")]
    public bool fleesWhenFound = false;
    public Transform safeZone; // Drag your Anemone transform here in the inspector
    public float fleeSpeed = 5f;

    [Header("Dynamic Difficulty Scaling")]
    public bool scaleWithLevel = true;
    [Tooltip("e.g. 0.1 = +10% EXP per level above 1")]
    public float expScaleFactor = 0.1f;

    private void Start()
    {
        if (scaleWithLevel)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                PlayerStats stats = playerObj.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    int levelDiff = Mathf.Max(0, stats.currentLevel - 1);
                    expReward = expReward * (1f + (levelDiff * expScaleFactor));
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasBeenDiscovered)
        {
            // 1. Mark this specific fish as discovered so it can start fleeing
            hasBeenDiscovered = true;

            // 2. Check if this species has been seen globally
            bool isNewDiscovery = true;
            if (BestiaryUI.Instance != null && !string.IsNullOrEmpty(speciesID))
            {
                isNewDiscovery = BestiaryUI.Instance.UnlockSpecies(speciesID);
            }

            // 3. Only give rewards and show popup if it's the very first time!
            if (isNewDiscovery)
            {
                PlayerStats stats = other.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.GainEXP(expReward);
                    Debug.Log($"Discovered {gameObject.name}! Gained {expReward} EXP.");
                }

                // Show a UI popup notification and link it to the specific species
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification("Species added to bestiary!", speciesID);
                }
            }

            // 4. Optional: Trigger a happy little "Discovery!" sound effect here
        }
    }

    private void Update()
    {
        // 4. If it's a fleeing fish (like the Clownfish) and has been discovered, make it swim away!
        if (hasBeenDiscovered && fleesWhenFound && safeZone != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, safeZone.position, fleeSpeed * Time.deltaTime);
            
            // Look at the safe zone
            if (safeZone.position.x > transform.position.x)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public void ResetDiscovery()
    {
        hasBeenDiscovered = false;
    }
}