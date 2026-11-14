using UnityEngine;

public class FishCameraFocus : MonoBehaviour
{
    private CameraFollow cameraScript;

    private void Start()
    {
        // Automatically find the CameraFollow script on the Main Camera
        cameraScript = Camera.main.GetComponent<CameraFollow>();
    }

    // When the submarine enters the fish's trigger area
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure your player has the tag "Player" in the inspector!
        if (other.CompareTag("Player"))
        {
            if (cameraScript != null)
            {
                // Tell the camera to focus on THIS fish
                cameraScript.FocusOn(transform);
            }
        }
    }

    // When the submarine leaves the fish's trigger area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (cameraScript != null)
            {
                // Tell the camera to go back to the player
                cameraScript.ResetFocus();
            }
        }
    }
}