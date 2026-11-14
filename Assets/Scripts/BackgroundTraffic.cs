using UnityEngine;
using System.Collections;

public class BackgroundTraffic : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    [Tooltip("Wait time in seconds between each appearance")]
    public float respawnDelay = 5f;

    [Header("Spawn Boundaries")]
    public float spawnBoundaryX = 25f;
    public Vector2 spawnRangeY = new Vector2(-40f, -15f);
    private bool movingRight = true;
    private bool isCycling = false;
    private Vector3 originalScale;

    private void Start()
    {
        // Remember the original size/scale of the submarine
        originalScale = transform.localScale;
        
        SetVisible(false);
        StartCoroutine(SpawnCycle());
    }

    private void Update()
    {
        if (!isCycling && gameObject.activeSelf)
        {
            float directionMultiplier = movingRight ? 1 : -1;
            transform.Translate(Vector2.right * directionMultiplier * speed * Time.deltaTime);

            CheckIfOutOfBounds();
        }
    }

    private IEnumerator SpawnCycle()
    {
        while (true)
        {
            isCycling = true;
            SetVisible(false);

            yield return new WaitForSeconds(respawnDelay);

            SetupNewRun();

            SetVisible(true);
            isCycling = false;

            yield return new WaitUntil(() => isCycling == true); 
        }
    }

    private void SetupNewRun()
    {
        movingRight = Random.value > 0.5f;
        float randomY = Random.Range(spawnRangeY.x, spawnRangeY.y);
        float startX;

        // Flip the entire game object scale so attached particles flip too!
        Vector3 newScale = originalScale;

        if (movingRight)
        {
            startX = -spawnBoundaryX; 
            newScale.x = Mathf.Abs(originalScale.x); // Face Right
        }
        else
        {
            startX = spawnBoundaryX; 
            newScale.x = -Mathf.Abs(originalScale.x); // Face Left
        }

        transform.localScale = newScale;
        transform.position = new Vector3(startX, randomY, transform.position.z);
    }

    private void CheckIfOutOfBounds()
    {
        if (movingRight && transform.position.x > spawnBoundaryX) isCycling = true;
        else if (!movingRight && transform.position.x < -spawnBoundaryX) isCycling = true;
    }

    private void SetVisible(bool visible)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = visible;
        }
    }
}