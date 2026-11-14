using UnityEngine;
using System.Collections;

/// <summary>
/// Types of entities that can appear on the minimap.
/// Echolocation reveals different types at different levels.
/// </summary>
public enum MinimapIconType
{
    Enemy,          // Sharks, Tuna, etc.
    Discoverable,   // DiscoverableSpecies
    Hazard,         // Environmental hazards
    LockedSpecies   // Species not yet unlocked in bestiary
}

/// <summary>
/// Attach to enemies, discoverables, and hazards.
/// The iconObject child should be on a "Minimap" layer only visible to the minimap camera.
/// By default the icon is hidden; Echolocation's Reveal() temporarily shows it.
/// </summary>
public class MinimapIcon : MonoBehaviour
{
    [Header("Minimap Settings")]
    [Tooltip("The child GameObject with a SpriteRenderer on the 'Minimap' layer")]
    public GameObject iconObject;

    [Tooltip("What type of entity this is (determines which Echolocation level reveals it)")]
    public MinimapIconType iconType = MinimapIconType.Enemy;

    private Coroutine revealCoroutine;

    private void Start()
    {
        // Icons start hidden — revealed by Echolocation pings
        if (iconObject != null)
            iconObject.SetActive(false);
    }

    /// <summary>
    /// Temporarily reveal this icon on the minimap for the given duration.
    /// </summary>
    public void Reveal(float duration)
    {
        if (iconObject == null) return;

        // If already revealed, restart the timer
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);

        revealCoroutine = StartCoroutine(RevealRoutine(duration));
    }

    private IEnumerator RevealRoutine(float duration)
    {
        iconObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        iconObject.SetActive(false);
        revealCoroutine = null;
    }
}
