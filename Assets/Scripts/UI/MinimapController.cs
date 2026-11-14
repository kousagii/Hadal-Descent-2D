using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the minimap displayed in the bottom-right corner of the screen.
/// Uses a secondary orthographic camera rendering to a RenderTexture,
/// displayed via a RawImage in the HUD Canvas.
/// </summary>
public class MinimapController : MonoBehaviour
{
    [Header("Minimap Camera")]
    [Tooltip("The secondary orthographic camera used for the minimap")]
    public Camera minimapCamera;

    [Header("Follow Target")]
    [Tooltip("The player transform — minimap camera follows this")]
    public Transform playerTransform;

    [Header("Settings")]
    [Tooltip("How high above the player the minimap camera sits (Z offset for 2D)")]
    public float cameraZOffset = -10f;

    [Tooltip("The orthographic size of the minimap camera (how zoomed out it is)")]
    public float minimapZoom = 20f;

    private void Start()
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = minimapZoom;
        }
    }

    private void LateUpdate()
    {
        if (minimapCamera != null && playerTransform != null)
        {
            // Follow the player's position
            minimapCamera.transform.position = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y,
                cameraZOffset
            );
        }
    }
}
