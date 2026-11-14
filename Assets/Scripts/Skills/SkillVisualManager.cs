using UnityEngine;

/// <summary>
/// Manages the skill visual PNG overlays displayed in front of the player character.
/// Each acquired skill gets its own child SpriteRenderer with a higher sorting order
/// so it renders on top of the player sprite.
/// Passive overlays are always visible; active overlays only show during skill activation.
/// </summary>
public class SkillVisualManager : MonoBehaviour
{
    public static SkillVisualManager Instance;

    [Header("References")]
    [Tooltip("Child transform of the Player — all skill overlays are parented here")]
    public Transform overlayParent;

    [Header("Settings")]
    [Tooltip("Base sorting order for overlays (should be higher than the player sprite)")]
    public int baseSortingOrder = 1;

    private SpriteRenderer playerSpriteRenderer;

    private void Awake()
    {
        Instance = this;

        // Get the player's main SpriteRenderer for flip mirroring
        playerSpriteRenderer = GetComponentInParent<SpriteRenderer>();
        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        // Mirror overlay flipping to match the player's facing direction
        if (playerSpriteRenderer != null && overlayParent != null)
        {
            Vector3 scale = overlayParent.localScale;
            scale.x = playerSpriteRenderer.flipX ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            overlayParent.localScale = scale;
        }
    }

    /// <summary>
    /// Creates an overlay sprite for a skill. Returns the GameObject so the caller
    /// can enable/disable it (for active skills).
    /// </summary>
    public GameObject CreateOverlay(SkillData skillData, int sortingOffset)
    {
        if (overlayParent == null || skillData.visualOverlay == null) return null;

        GameObject overlayObj = new GameObject($"Overlay_{skillData.skillName}");
        overlayObj.transform.SetParent(overlayParent);
        overlayObj.transform.localPosition = Vector3.zero;
        overlayObj.transform.localRotation = Quaternion.identity;
        overlayObj.transform.localScale = Vector3.one;

        SpriteRenderer sr = overlayObj.AddComponent<SpriteRenderer>();
        sr.sprite = skillData.visualOverlay;
        sr.sortingLayerName = playerSpriteRenderer != null ? playerSpriteRenderer.sortingLayerName : "Default";
        sr.sortingOrder = baseSortingOrder + sortingOffset;

        // Passive skills: overlay starts enabled (always visible)
        // Active skills: overlay starts disabled (shown only during activation)
        overlayObj.SetActive(skillData.skillType == SkillType.Passive);

        return overlayObj;
    }
}
