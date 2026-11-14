using UnityEngine;
using TMPro;

public class FishDialogue : MonoBehaviour
{
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    [TextArea(3, 10)]
    public string message;

    // --- NEW EXP SETTINGS ---
    [Header("Rewards")]
    public float expReward = 20f;
    private bool hasMetPlayer = false; // Prevents infinite EXP farming
    // ------------------------

    private bool isDialogueActive = false;
    private MonoBehaviour playerMovementScript;
    private Rigidbody2D playerRigidbody;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDialogueActive)
        {
            playerMovementScript = other.GetComponent("PlayerMovement") as MonoBehaviour;
            playerRigidbody = other.GetComponent<Rigidbody2D>();

            if (playerMovementScript != null) playerMovementScript.enabled = false;
            if (playerRigidbody != null) playerRigidbody.linearVelocity = Vector2.zero; 

            dialogueText.text = message;
            dialogueBox.SetActive(true);
            isDialogueActive = true;

            // --- NEW: Give EXP if this is the first meeting ---
            if (!hasMetPlayer)
            {
                PlayerStats stats = other.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.GainEXP(expReward);
                    hasMetPlayer = true; // Flips the switch so they don't get EXP again
                }
            }
            // --------------------------------------------------
        }
    }

    private void Update()
    {
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)))
        {
            dialogueBox.SetActive(false);
            isDialogueActive = false;
            if (playerMovementScript != null) playerMovementScript.enabled = true;
        }
    }
}