using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel; // Create a dark semi-transparent panel for your pause menu
    public Button pauseButton;    // The pause icon on your HUD
    public Button resumeButton;   // A button inside the pause panel to resume
    public Button bestiaryButton; // The book icon on your HUD
    
    [Header("Navigation Buttons")]
    public Button retryButton;    // Optional retry button on pause menu
    public Button mainMenuButton; // Optional main menu button on pause menu
    
    [Header("Settings")]
    public string mainMenuSceneName = "StartScene";

    private bool isPaused = false;

    private void Start()
    {
        // Make sure the menu is hidden at the start
        if (pausePanel != null) pausePanel.SetActive(false);

        // Hook up the buttons
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (resumeButton != null) resumeButton.onClick.AddListener(TogglePause);
        
        if (retryButton != null) retryButton.onClick.AddListener(RetryGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMenu);
        
        // Hook up the Bestiary button
        if (bestiaryButton != null)
        {
            bestiaryButton.onClick.AddListener(() => 
            {
                if (BestiaryUI.Instance != null) 
                    BestiaryUI.Instance.ToggleBestiary();
            });
        }
    }

    private void Update()
    {
        // Optional: Also allow pausing with the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        // Don't pause if the bestiary is currently open!
        if (BestiaryUI.Instance != null && BestiaryUI.Instance.bestiaryPanel.activeSelf) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
    }

    private void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
