using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class BestiaryUI : MonoBehaviour
{
    public static BestiaryUI Instance;

    [Header("UI References")]
    [Tooltip("Drag the Bestiary Canvas or main panel here")]
    public GameObject bestiaryPanel;

    [Header("Bestiary Databases")]
    public List<BestiaryEntry> creatureDatabase;
    public List<BestiaryEntry> skillDatabase;
    public List<BestiaryEntry> zoneDatabase;
    
    private List<BestiaryEntry> activeDatabase;

    [Header("UI Details Panel")]
    public GameObject detailsPanel;
    public TMP_Text nameText;

    [Header("Scroll View Settings")]
    [Tooltip("Prefab for the zone header text")]
    public GameObject headerPrefab;
    [Tooltip("Prefab for the list item in the scroll view")]
    public GameObject listItemPrefab;
    [Tooltip("The Content object inside the Scroll View")]
    public Transform scrollContentParent;

    [Header("Photo Display")]
    [Tooltip("Real-world photo slot")]
    public Image realPhotoImage;
    [Tooltip("Game sprite slot")]
    public Image gameSpriteImage;

    [Header("Info Text")]
    public TMP_Text descriptionText;
    public TMP_Text triviaText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSfx;
    public AudioClip closeSfx;
    public AudioClip turnPageSfx;

    [Header("Pagination & Controls")]
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    [Header("Tab Buttons")]
    public Button creaturesTabBtn;
    public Button skillsTabBtn;
    public Button zonesTabBtn;

    private int currentIndex = 0;
    private bool isBestiaryOpen = false;
    
    // Global tracking of discovered species
    private HashSet<string> discoveredSpecies = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ensure the bestiary is closed when the game starts
        if (bestiaryPanel != null) bestiaryPanel.SetActive(false);

        // Hook up buttons automatically
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        if (closeButton != null) closeButton.onClick.AddListener(CloseBestiary);

        // Hook up tab buttons
        if (creaturesTabBtn != null) creaturesTabBtn.onClick.AddListener(() => SwitchTab(creatureDatabase));
        if (skillsTabBtn != null) skillsTabBtn.onClick.AddListener(() => SwitchTab(skillDatabase));
        if (zonesTabBtn != null) zonesTabBtn.onClick.AddListener(() => SwitchTab(zoneDatabase));

        // Default tab
        activeDatabase = creatureDatabase;
    }

    private void Update()
    {
        // Toggle Bestiary with the Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleBestiary();
        }

        // Manual click detection for scroll view items
        // (Bypasses Unity's GraphicRaycaster coordinate mismatch from Canvas Scaler)
        if (isBestiaryOpen && Input.GetMouseButtonDown(0) && scrollContentParent != null)
        {
            Canvas canvas = scrollContentParent.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
                RectTransform contentRT = scrollContentParent as RectTransform;
                
                if (contentRT != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    contentRT, Input.mousePosition, cam, out Vector2 localPoint))
                {
                    foreach (Transform child in scrollContentParent)
                    {
                        RectTransform childRT = child.GetComponent<RectTransform>();
                        Button childBtn = child.GetComponent<Button>();
                        if (childRT != null && childBtn != null)
                        {
                            // Convert the click from Content-local to child-local
                            Vector2 childLocal = localPoint - (Vector2)childRT.localPosition;
                            if (childRT.rect.Contains(childLocal))
                            {
                                childBtn.onClick.Invoke();
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public void ToggleBestiary()
    {
        isBestiaryOpen = !isBestiaryOpen;
        
        if (bestiaryPanel != null)
        {
            bestiaryPanel.SetActive(isBestiaryOpen);
        }

        if (isBestiaryOpen)
        {
            // Ensure we have an active database
            if (activeDatabase == null) activeDatabase = creatureDatabase;
            PopulateScrollView(); // Populate the list every time we open it
            ShowCurrentPage();
            if (audioSource != null && openSfx != null) audioSource.PlayOneShot(openSfx);
        }
        else
        {
            if (audioSource != null && closeSfx != null) audioSource.PlayOneShot(closeSfx);
        }

        // Pause the game while looking at the bestiary!
        Time.timeScale = isBestiaryOpen ? 0f : 1f;
    }

    public void SwitchTab(List<BestiaryEntry> newDatabase)
    {
        if (newDatabase == null) return;
        activeDatabase = newDatabase;
        currentIndex = 0;
        PopulateScrollView();
        ShowCurrentPage();
        if (audioSource != null && turnPageSfx != null) audioSource.PlayOneShot(turnPageSfx);
    }

    public void CloseBestiary()
    {
        if (isBestiaryOpen) ToggleBestiary();
    }

    public void OpenToEntry(string speciesID)
    {
        // 1. Open the UI and pause
        isBestiaryOpen = true;
        if (bestiaryPanel != null) bestiaryPanel.SetActive(true);
        Time.timeScale = 0f; 

        // 2. Find the index across databases
        int index = -1;
        if (creatureDatabase != null) index = creatureDatabase.FindIndex(x => x.speciesID == speciesID);
        
        if (index != -1) { activeDatabase = creatureDatabase; currentIndex = index; }
        else 
        {
            if (skillDatabase != null) index = skillDatabase.FindIndex(x => x.speciesID == speciesID);
            if (index != -1) { activeDatabase = skillDatabase; currentIndex = index; }
            else 
            {
                if (zoneDatabase != null) index = zoneDatabase.FindIndex(x => x.speciesID == speciesID);
                if (index != -1) { activeDatabase = zoneDatabase; currentIndex = index; }
            }
        }
        
        if (index != -1)
        {
            PopulateScrollView(); 
            ShowCurrentPage();
        }
        else
        {
            Debug.LogWarning("No Bestiary entry found in any database for: " + speciesID);
        }
    }

    public void NextPage()
    {
        if (activeDatabase == null || activeDatabase.Count == 0) return;
        currentIndex++;
        if (currentIndex >= activeDatabase.Count) currentIndex = 0; // Wrap around to start
        ShowCurrentPage();
        if (audioSource != null && turnPageSfx != null) audioSource.PlayOneShot(turnPageSfx);
    }

    public void PrevPage()
    {
        if (activeDatabase == null || activeDatabase.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = activeDatabase.Count - 1; // Wrap around to end
        ShowCurrentPage();
        if (audioSource != null && turnPageSfx != null) audioSource.PlayOneShot(turnPageSfx);
    }

    private void PopulateScrollView()
    {
        if (listItemPrefab == null || scrollContentParent == null) return;

        // 1. Clear out any old buttons so we don't duplicate them
        foreach (Transform child in scrollContentParent)
        {
            Destroy(child.gameObject);
        }

        string currentZone = "";

        // 2. Loop through the database and create a button for each entry
        if (activeDatabase == null) return;

        for (int i = 0; i < activeDatabase.Count; i++)
        {
            BestiaryEntry entry = activeDatabase[i];
            int indexToSelect = i; // Save the index for the button click

            // Spawn Header if we entered a new zone
            if (!string.IsNullOrEmpty(entry.zone) && entry.zone != currentZone)
            {
                currentZone = entry.zone;
                if (headerPrefab != null)
                {
                    GameObject headerObj = Instantiate(headerPrefab, scrollContentParent);
                    TMP_Text headerText = headerObj.GetComponentInChildren<TMP_Text>();
                    if (headerText != null) headerText.text = currentZone;
                }
            }

            // Spawn the prefab inside the Content object
            GameObject newItem = Instantiate(listItemPrefab, scrollContentParent);
            
            // Find the child named "FishIcon" for the sprite
            Transform iconTransform = newItem.transform.Find("FishIcon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = entry.gameSprite;
                    // Silhouette Logic!
                    bool isUnlocked = entry.unlockedByDefault || discoveredSpecies.Contains(entry.speciesID);
                    if (isUnlocked)
                    {
                        iconImage.color = Color.white; // Normal color
                    }
                    else
                    {
                        iconImage.color = Color.black; // Blackened silhouette
                    }
                }
            }

            // Make the button click show the details on the right side
            Button btn = newItem.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => 
                {
                    currentIndex = indexToSelect;
                    ShowCurrentPage();
                    if (audioSource != null && turnPageSfx != null) audioSource.PlayOneShot(turnPageSfx);
                });
            }
        }
    }

    private void ShowCurrentPage()
    {
        if (activeDatabase == null || activeDatabase.Count == 0 || currentIndex < 0 || currentIndex >= activeDatabase.Count) return;

        BestiaryEntry entry = activeDatabase[currentIndex];
        if (detailsPanel != null) detailsPanel.SetActive(true);
        
        // Check if unlocked
        bool isUnlocked = entry.unlockedByDefault || discoveredSpecies.Contains(entry.speciesID);

        if (isUnlocked)
        {
            // ─── UNLOCKED: Show everything ───
            if (nameText != null) nameText.text = entry.displayName;
            
            // Photos
            ShowSinglePhoto(realPhotoImage, entry.realPhoto, true);
            ShowSinglePhoto(gameSpriteImage, entry.gameSprite, true, false);

            // Text info
            if (descriptionText != null) descriptionText.text = entry.description;
            if (triviaText != null) triviaText.text = entry.trivia;
        }
        else
        {
            // ─── LOCKED: Blackened silhouette + ??? ───
            if (nameText != null) nameText.text = "???";

            // Real-world photo — hidden when locked
            ShowSinglePhoto(realPhotoImage, null, false);
            
            // Game sprite — blackened silhouette
            ShowSinglePhoto(gameSpriteImage, entry.gameSprite, true, true);

            // Text info — all mystery
            if (descriptionText != null) descriptionText.text = "Discover this species to learn more!";
            if (triviaText != null) triviaText.text = "???";
        }
    }

    /// <summary>
    /// Helper to show/hide a photo slot.
    /// </summary>
    private void ShowSinglePhoto(Image imageSlot, Sprite photo, bool show, bool isSilhouette = false)
    {
        if (imageSlot == null) return;

        if (show && photo != null)
        {
            imageSlot.sprite = photo;
            imageSlot.preserveAspect = true; 
            imageSlot.gameObject.SetActive(true);
            imageSlot.color = isSilhouette ? Color.black : Color.white;
        }
        else
        {
            imageSlot.gameObject.SetActive(false);
        }
    }

    public bool UnlockSpecies(string speciesID)
    {
        if (string.IsNullOrEmpty(speciesID)) return false;

        if (!discoveredSpecies.Contains(speciesID))
        {
            discoveredSpecies.Add(speciesID);
            return true; // Newly unlocked!
        }
        
        return false; // Already unlocked previously
    }

    /// <summary>
    /// Checks if every creature belonging to the specified zone has been discovered.
    /// </summary>
    public bool IsZoneComplete(string zoneName)
    {
        if (creatureDatabase == null) return true;

        bool hasCreaturesInZone = false;

        foreach (var entry in creatureDatabase)
        {
            // Only check entries that belong to this zone
            if (entry.zone == zoneName)
            {
                hasCreaturesInZone = true;

                // If it's not unlocked by default and not in the discovered list, zone is incomplete
                if (!entry.unlockedByDefault && !discoveredSpecies.Contains(entry.speciesID))
                {
                    return false;
                }
            }
        }

        // If no creatures exist for this zone, technically it's "complete" (or you can return false)
        return hasCreaturesInZone;
    }

    /// <summary>
    /// Forgets all discovered species for a specific zone. Used as a penalty for losing the quiz boss.
    /// </summary>
    public void WipeZoneDiscoveries(string zoneName)
    {
        if (creatureDatabase == null) return;

        foreach (var entry in creatureDatabase)
        {
            if (entry.zone == zoneName)
            {
                if (discoveredSpecies.Contains(entry.speciesID))
                {
                    discoveredSpecies.Remove(entry.speciesID);
                }
            }
        }
        
        Debug.LogWarning($"Bestiary memory for {zoneName} has been completely wiped!");
    }
}
