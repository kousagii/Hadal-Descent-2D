using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnderwaterDepth2D : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Image waterOverlay;
    
    [Header("UI Text")]
    [Tooltip("Drag a UI Text here to display the current depth and zone")]
    public TMP_Text depthText;

    [Header("Depth Tracking")]
    [Tooltip("The Y position of the water surface")]
    public float surfaceY = 0f;
    [Tooltip("The Y position where it reaches maximum darkness (Set to -200 for the boss border)")]
    public float maxDepthY = -200f; 
    [Tooltip("Multiplier for the displayed depth. E.g. 2 means Y=-100 shows as 200m")]
    public float depthMultiplier = 2f;

    [Header("Color Settings")]
    public Color surfaceColor = new Color(0.2f, 0.6f, 0.8f, 0.1f);
    public Color deepColor = new Color(0.01f, 0.05f, 0.15f, 0.9f);

    private void Update()
    {
        if (player == null) return;

        // Calculate depth based on player Y position and scale it by the multiplier
        float currentDepthMeters = Mathf.Max(0, (surfaceY - player.position.y)) * depthMultiplier;

        if (depthText != null)
        {
            // Determine the zone based on depth
            string zoneName = "Sunlight Zone";
            if (currentDepthMeters >= 6000) zoneName = "Hadal Zone";
            else if (currentDepthMeters >= 4000) zoneName = "Abyssal Zone";
            else if (currentDepthMeters >= 1000) zoneName = "Midnight Zone";
            else if (currentDepthMeters >= 200) zoneName = "Twilight Zone";

            // Update UI Text
            depthText.text = $"{zoneName} - {currentDepthMeters:F0}m";
        }

        if (waterOverlay != null)
        {
            float depthPercent = Mathf.InverseLerp(surfaceY, maxDepthY, player.position.y);
            depthPercent = Mathf.Clamp01(depthPercent);
            waterOverlay.color = Color.Lerp(surfaceColor, deepColor, depthPercent);
        }
    }
}