using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI References")]
    [Tooltip("Drag the Notification Panel here (e.g. a small banner at top of screen)")]
    public GameObject notificationPanel;
    [Tooltip("Drag the Text component here")]
    public TMP_Text notificationText;
    [Tooltip("Drag the 'View' Button UI element here")]
    public Button viewButton;
    
    private string currentSpeciesID = "";

    [Header("Settings")]
    public float displayTime = 3f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (notificationPanel != null) notificationPanel.SetActive(false);
    }

    private void Start()
    {
        if (viewButton != null)
        {
            viewButton.onClick.AddListener(OnViewButtonClicked);
        }
    }

    public void ShowNotification(string message, string speciesID = "")
    {
        currentSpeciesID = speciesID;
        
        if (viewButton != null)
        {
            // Only show the View button if we actually have a species to look at
            viewButton.gameObject.SetActive(!string.IsNullOrEmpty(speciesID));
        }

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        StopAllCoroutines();
        StartCoroutine(DisplayRoutine(message));
    }

    private void OnViewButtonClicked()
    {
        if (!string.IsNullOrEmpty(currentSpeciesID) && BestiaryUI.Instance != null)
        {
            BestiaryUI.Instance.OpenToEntry(currentSpeciesID);
            
            // Hide the notification banner when they click View
            if (notificationPanel != null) notificationPanel.SetActive(false);
        }
    }

    private IEnumerator DisplayRoutine(string message)
    {
        if (notificationPanel != null) notificationPanel.SetActive(true);
        if (notificationText != null) notificationText.text = message;

        yield return new WaitForSeconds(displayTime);

        if (notificationPanel != null) notificationPanel.SetActive(false);
    }
}
