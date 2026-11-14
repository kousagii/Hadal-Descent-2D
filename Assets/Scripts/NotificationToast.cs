using System.Collections;
using UnityEngine;
using TMPro;

public class NotificationToast : MonoBehaviour
{
    [Header("Movement & Timing")]
    [SerializeField] private float moveSpeed = 40f;       // How fast it drifts up
    [SerializeField] private float displayDuration = 4f;  // Solid visibility time (4 seconds)
    [SerializeField] private float fadeDuration = 1f;     // Fade out time (1 second)

    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private Vector2 startingPosition;
    private Color startingColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        
        // Save original positions so we can reset it perfectly next time
        startingPosition = rectTransform.anchoredPosition;
        startingColor = textMesh.color;
    }

    // This runs automatically whenever you activate the GameObject
    void OnEnable()
    {
        // Reset everything back to the beginning state
        rectTransform.anchoredPosition = startingPosition;
        
        Color resetColor = startingColor;
        resetColor.a = 1f; // Start fully visible
        textMesh.color = resetColor;

        StopAllCoroutines();
        StartCoroutine(ToastSequence());
    }

    IEnumerator ToastSequence()
    {
        float timer = 0f;

        // --- PHASE 1: Stay fully visible for 4 seconds ---
        while (timer < displayDuration)
        {
            timer += Time.deltaTime;
            
            // Continues to drift upwards slowly
            rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;
            yield return null;
        }

        // --- PHASE 2: Fade out smoothly over 1 second ---
        timer = 0f; // Reset timer for the fade phase
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration; 

            // Keep drifting upwards
            rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

            // Fade from 1 (visible) down to 0 (invisible)
            Color newColor = startingColor;
            newColor.a = Mathf.Lerp(1f, 0f, progress); 
            textMesh.color = newColor;

            yield return null;
        }

        // Hard safety check: ensure it is fully invisible and turn it off
        Color finalColor = startingColor;
        finalColor.a = 0f;
        textMesh.color = finalColor;
        
        gameObject.SetActive(false);
    }
}