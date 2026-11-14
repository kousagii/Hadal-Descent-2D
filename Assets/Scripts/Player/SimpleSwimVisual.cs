using UnityEngine;

public class SimpleSwimVisual : MonoBehaviour
{
    [Header("Swimming Visuals")]
    [Tooltip("How fast the fish breathes/bobs")]
    public float swimSpeed = 3f;
    [Tooltip("How much the fish stretches/squashes")]
    public float swimAmount = 0.05f;

    private Vector3 originalScale;

    private void Start()
    {
        // Remember the starting size of the sprite
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Calculate a gentle sine wave
        float wave = Mathf.Sin(Time.time * swimSpeed) * swimAmount;

        // Apply it to the Y axis to make the fish look like it is breathing/swimming
        transform.localScale = new Vector3(
            originalScale.x, 
            originalScale.y + wave, 
            originalScale.z
        );
    }
}