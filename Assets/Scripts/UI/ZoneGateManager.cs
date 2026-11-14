using UnityEngine;
using System.Collections.Generic;

public class ZoneGateManager : MonoBehaviour
{
    [Header("Gate Settings")]
    [Tooltip("The Y position where the boss boundary is (e.g., -100 for 200m if multiplier is 2)")]
    public float gateY = -100f;
    [Tooltip("How far to bounce the player back up if they are rejected")]
    public float bounceBackDistance = 5f;
    
    [Header("Bestiary Check")]
    public string requiredZoneName = "Sunlight Zone";

    [Header("Messages")]
    public string deniedTitle = "Access Denied";
    [TextArea] public string deniedMessage = "Complete the Sunlight Zone Bestiary before proceeding beyond this depth.";

    public string warningTitle = "Warning";
    [TextArea] public string warningMessage = "Before proceeding, review your Bestiary carefully. The knowledge you have gained about the creatures of the Sunlight Zone may be the key to overcoming the challenges ahead and defeating the boss.";

    private bool hasPassedGate = false;
    private bool hasShownWarning = false;

    private void Update()
    {
        // Don't check if we already passed the gate
        if (hasPassedGate) return;

        // Ensure we have a reference to the player (assuming this script is ON the player)
        Transform player = transform; 
        // If attached to an empty manager object, we need to find the player:
        if (!player.CompareTag("Player"))
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        // Check if player crossed the boundary
        if (player.position.y < gateY)
        {
            CheckGate(player);
        }
    }

    private void CheckGate(Transform player)
    {
        if (BestiaryUI.Instance == null || PopupManager.Instance == null) return;

        bool isZoneComplete = BestiaryUI.Instance.IsZoneComplete(requiredZoneName);

        if (!isZoneComplete)
        {
            // ACCESS DENIED
            // Push player back up so they aren't stuck inside the trigger zone
            player.position = new Vector3(player.position.x, gateY + bounceBackDistance, player.position.z);
            
            // Stop their movement velocity if they have a Rigidbody
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            PopupManager.Instance.ShowPopup(deniedTitle, deniedMessage);
        }
        else
        {
            // ACCESS GRANTED - Show Choice Warning
            if (!hasShownWarning)
            {
                // Stop player movement
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;

                PopupManager.Instance.ShowChoicePopup(
                    warningTitle, 
                    warningMessage, 
                    onProceed: () => 
                    {
                        hasShownWarning = true;
                        hasPassedGate = true;
                        
                        // Start Quiz Boss!
                        if (QuizBossManager.Instance != null)
                        {
                            QuizBossManager.Instance.StartQuiz();
                        }
                        else
                        {
                            Debug.LogError("QuizBossManager is missing from the scene!");
                        }
                    },
                    onClose: () =>
                    {
                        // Player backed out - bounce them up
                        player.position = new Vector3(player.position.x, gateY + bounceBackDistance, player.position.z);
                    }
                );
            }
        }
    }

    // Optional: Draw a line in the editor to see where the gate is
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-50, gateY, 0), new Vector3(50, gateY, 0));
    }

    /// <summary>
    /// Called when the player loses the boss quiz and is thrown back to the surface.
    /// Re-locks the gate.
    /// </summary>
    public void ResetGate()
    {
        hasPassedGate = false;
        hasShownWarning = false;
    }
}
