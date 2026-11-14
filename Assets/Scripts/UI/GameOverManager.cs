using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI Panels")]
    public GameObject gameOverPanel;

    [Header("UI Text Fields")]
    public TMP_Text timeSurvivedText;
    public TMP_Text levelReachedText;
    public TMP_Text deepestDepthText;

    [Header("Buttons")]
    public Button retryButton;
    public Button mainMenuButton;

    [Header("Settings")]
    public string mainMenuSceneName = "StartScene";

    // Trackers
    private float timeSurvived = 0f;
    private float deepestDepth = 0f;
    private bool isGameOver = false;

    private UnderwaterDepth2D depthManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        depthManager = FindObjectOfType<UnderwaterDepth2D>();

        // Hook up buttons
        if (retryButton != null) retryButton.onClick.AddListener(RetryGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMenu);
    }

    private void Update()
    {
        if (isGameOver) return;

        // Track time
        timeSurvived += Time.deltaTime;

        // Track deepest depth
        if (depthManager != null && depthManager.player != null)
        {
            float currentDepth = Mathf.Max(0, (depthManager.surfaceY - depthManager.player.position.y)) * depthManager.depthMultiplier;
            if (currentDepth > deepestDepth)
            {
                deepestDepth = currentDepth;
            }
        }
    }

    /// <summary>
    /// Called by PlayerStats when HP reaches 0.
    /// </summary>
    public void TriggerGameOver(int finalLevel)
    {
        if (isGameOver) return;
        isGameOver = true;

        // Freeze game
        Time.timeScale = 0f;

        // Update Text
        if (timeSurvivedText != null)
        {
            int minutes = Mathf.FloorToInt(timeSurvived / 60F);
            int seconds = Mathf.FloorToInt(timeSurvived - minutes * 60);
            timeSurvivedText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (levelReachedText != null)
        {
            levelReachedText.text = $"Level: {finalLevel}";
        }

        if (deepestDepthText != null)
        {
            deepestDepthText.text = $"Deepest Dive: {deepestDepth:F0}m";
        }

        // Show the panel
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void RetryGame()
    {
        // Unfreeze time
        Time.timeScale = 1f;
        // Reload current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToMenu()
    {
        // Unfreeze time
        Time.timeScale = 1f;
        // Load main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
