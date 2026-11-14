using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("UI References")]
    public GameObject popupPanel;
    public TMP_Text titleText;
    public TMP_Text bodyText;
    
    [Header("Controls")]
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;
    public Button proceedButton; // Added for the choice popup

    private List<string> currentPages;
    private int currentPageIndex = 0;
    private System.Action onCloseCallback;
    private System.Action onProceedCallback;
    private bool isChoiceMode = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePopup);
        
        if (proceedButton != null) proceedButton.onClick.AddListener(ProceedPopup);
    }

    /// <summary>
    /// Shows a multi-page popup message and pauses the game.
    /// </summary>
    public void ShowPopup(string title, List<string> pages, System.Action onClose = null)
    {
        if (pages == null || pages.Count == 0) return;

        isChoiceMode = false;
        titleText.text = title;
        currentPages = pages;
        currentPageIndex = 0;
        onCloseCallback = onClose;

        popupPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game while reading

        UpdatePageDisplay();
    }

    /// <summary>
    /// Shows a single-page popup message.
    /// </summary>
    public void ShowPopup(string title, string singlePage, System.Action onClose = null)
    {
        ShowPopup(title, new List<string> { singlePage }, onClose);
    }

    /// <summary>
    /// Shows a choice popup with "Proceed" and "Close" buttons.
    /// </summary>
    public void ShowChoicePopup(string title, string message, System.Action onProceed, System.Action onClose = null)
    {
        isChoiceMode = true;
        titleText.text = title;
        currentPages = new List<string> { message };
        currentPageIndex = 0;
        
        onCloseCallback = onClose;
        onProceedCallback = onProceed;

        popupPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game

        UpdatePageDisplay();
    }

    private void NextPage()
    {
        if (currentPageIndex < currentPages.Count - 1)
        {
            currentPageIndex++;
            UpdatePageDisplay();
        }
    }

    private void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageDisplay();
        }
    }

    private void UpdatePageDisplay()
    {
        bodyText.text = currentPages[currentPageIndex];

        if (isChoiceMode)
        {
            // Choice Mode: Hide Next/Prev, Show Close and Proceed
            if (nextButton != null) nextButton.gameObject.SetActive(false);
            if (prevButton != null) prevButton.gameObject.SetActive(false);
            if (closeButton != null) closeButton.gameObject.SetActive(true);
            if (proceedButton != null) proceedButton.gameObject.SetActive(true);
        }
        else
        {
            // Normal Mode
            if (proceedButton != null) proceedButton.gameObject.SetActive(false);

            if (prevButton != null)
                prevButton.gameObject.SetActive(currentPageIndex > 0);

            bool isLastPage = (currentPageIndex == currentPages.Count - 1);
            
            if (nextButton != null)
                nextButton.gameObject.SetActive(!isLastPage);
                
            if (closeButton != null)
                closeButton.gameObject.SetActive(isLastPage);
        }
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
        Time.timeScale = 1f; // Unpause game

        if (onCloseCallback != null)
        {
            onCloseCallback.Invoke();
            onCloseCallback = null; 
        }
    }

    private void ProceedPopup()
    {
        popupPanel.SetActive(false);
        Time.timeScale = 1f; // Unpause game

        if (onProceedCallback != null)
        {
            onProceedCallback.Invoke();
            onProceedCallback = null;
        }
    }
}
