using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("This should ALWAYS be your Player")]
    public Transform target; 
    private Transform focusTarget; // NEW: The fish we are currently talking to
    private Camera cam;

    [Header("Settings")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Zoom Settings")]
    public float defaultZoom = 5f; 
    public float focusZoom = 3.5f; 
    private float targetZoom;

    [Header("Camera Boundaries")]
    public bool useBounds = true;
    public Vector2 minBounds; 
    public Vector2 maxBounds;

    private void Start()
    {
        cam = GetComponent<Camera>(); 
        
        if (cam != null)
        {
            defaultZoom = cam.orthographicSize;
            targetZoom = defaultZoom;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // --- 1. FIND THE LOOK POSITION (NEW) ---
        // Start by assuming we just look at the player
        Vector3 pointToLookAt = target.position; 

        // If we are interacting with a fish, find the exact middle between Player and Fish!
        if (focusTarget != null)
        {
            pointToLookAt = (target.position + focusTarget.position) / 2f;
        }

        // --- 2. HANDLE POSITION ---
        Vector3 desiredPosition = pointToLookAt + offset;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // --- 3. HANDLE ZOOM ---
        if (cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, smoothSpeed * Time.deltaTime);
        }
    }

    // --- METHODS FOR FISH INTERACTION ---

    public void FocusOn(Transform newTarget)
    {
        focusTarget = newTarget; // Set the fish as the focus point
        targetZoom = focusZoom;  // Zoom in
    }

    public void ResetFocus()
    {
        focusTarget = null;      // Clear the focus point so we only look at the player
        targetZoom = defaultZoom; // Zoom out
    }
}