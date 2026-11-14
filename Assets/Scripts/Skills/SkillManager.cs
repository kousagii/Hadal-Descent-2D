using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Core skill system manager. Singleton attached to the Player.
/// Handles skill pool, level-up selection, runtime behaviour, cooldowns, input, and visuals.
/// </summary>
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("Skill Pool")]
    [Tooltip("Drag all 5 SkillData assets here")]
    public List<SkillData> allSkills;

    [Header("UI References")]
    [Tooltip("The pick-3 skill selection UI")]
    public SkillSelectionUI selectionUI;

    [Tooltip("Parent transform with HorizontalLayoutGroup for active skill buttons")]
    public Transform skillButtonParent;

    [Tooltip("Prefab for the active skill button")]
    public GameObject skillButtonPrefab;

    [Header("Visual Overlay")]
    [Tooltip("Reference to the SkillVisualManager on the player")]
    public SkillVisualManager visualManager;

    // Runtime state
    private List<SkillInstance> acquiredSkills = new List<SkillInstance>();
    private Dictionary<SkillData, float> cooldownTimers = new Dictionary<SkillData, float>();
    private Dictionary<SkillData, ActiveSkillButtonUI> activeButtons = new Dictionary<SkillData, ActiveSkillButtonUI>();
    private Dictionary<SkillData, GameObject> skillOverlays = new Dictionary<SkillData, GameObject>();

    // Fixed keybinds for active skills (assigned in order of acquisition)
    private KeyCode[] activeSkillKeys = { KeyCode.K, KeyCode.L, KeyCode.Semicolon };
    private string[] activeSkillKeyLabels = { "K", "L", ";" };
    private Dictionary<SkillData, KeyCode> assignedKeys = new Dictionary<SkillData, KeyCode>();
    private int nextKeyIndex = 0;

    // Skill behaviour references (added as components on the player)
    private BioluminescentFlare flare;
    private Chemosynthesis chemosynthesis;
    private Camouflage camouflage;
    private VenomGlands venomGlands;
    private Echolocation echolocation;

    // Reference to PlayerStats for listening to level up
    private PlayerStats playerStats;

    // Queue for pending level-ups (handles multi-level from one big XP gain)
    private int pendingLevelUps = 0;
    private bool isShowingSelection = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();

        if (playerStats != null)
            playerStats.OnLevelUp += OnPlayerLevelUp;

        // Hide selection UI at start
        if (selectionUI != null)
            selectionUI.Hide();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnLevelUp -= OnPlayerLevelUp;
    }

    private void Update()
    {
        // Update cooldown timers
        var keys = new List<SkillData>(cooldownTimers.Keys);
        foreach (var key in keys)
        {
            if (cooldownTimers[key] > 0f)
            {
                cooldownTimers[key] -= Time.deltaTime;
                if (cooldownTimers[key] < 0f) cooldownTimers[key] = 0f;
            }
        }

        // Handle active skill input
        foreach (var skill in acquiredSkills)
        {
            if (skill.data.skillType == SkillType.Active && assignedKeys.ContainsKey(skill.data))
            {
                KeyCode key = assignedKeys[skill.data];
                if (Input.GetKeyDown(key) && !IsOnCooldown(skill.data))
                {
                    ActivateSkill(skill);
                }
            }
        }
    }

    // ─────────────────────── Level Up ───────────────────────

    private void OnPlayerLevelUp()
    {
        pendingLevelUps++;

        if (!isShowingSelection)
            ShowNextSelection();
    }

    private void ShowNextSelection()
    {
        if (pendingLevelUps <= 0)
        {
            isShowingSelection = false;
            return;
        }

        pendingLevelUps--;
        isShowingSelection = true;

        // Build candidate list
        List<SkillData> candidates = new List<SkillData>();
        foreach (var skill in allSkills)
        {
            SkillInstance existing = GetSkillInstance(skill);
            if (existing == null || !existing.IsMaxLevel)
                candidates.Add(skill);
        }

        if (candidates.Count == 0)
        {
            // All skills maxed — skip selection
            isShowingSelection = false;
            return;
        }

        // Shuffle and pick up to 3
        Shuffle(candidates);
        List<SkillData> selected = candidates.Take(Mathf.Min(3, candidates.Count)).ToList();

        // Show UI
        selectionUI.Show(selected, GetSkillLevel, OnSkillSelected);
    }

    private void OnSkillSelected(SkillData skill)
    {
        AcquireOrUpgrade(skill);
        isShowingSelection = false;

        // Check for more pending level-ups
        if (pendingLevelUps > 0)
        {
            // Small delay so the UI has time to hide
            Invoke(nameof(ShowNextSelection), 0.1f);
        }
    }

    // ─────────────────────── Acquire / Upgrade ───────────────────────

    private void AcquireOrUpgrade(SkillData skill)
    {
        SkillInstance existing = GetSkillInstance(skill);

        if (existing != null)
        {
            // Upgrade existing skill
            existing.level++;
            if (existing.level >= 5) existing.synergiesUnlocked = true;
            UpdateSkillBehaviour(skill, existing.level);
        }
        else
        {
            // New acquisition
            SkillInstance newSkill = new SkillInstance { data = skill, level = 1 };
            acquiredSkills.Add(newSkill);
            SetupSkillBehaviour(skill, 1);

            // Create visual overlay
            if (visualManager != null)
            {
                int sortingOffset = acquiredSkills.Count;
                GameObject overlay = visualManager.CreateOverlay(skill, sortingOffset);
                if (overlay != null)
                    skillOverlays[skill] = overlay;
            }

            // For active skills: assign keybind and create UI button
            if (skill.skillType == SkillType.Active && nextKeyIndex < activeSkillKeys.Length)
            {
                assignedKeys[skill] = activeSkillKeys[nextKeyIndex];
                cooldownTimers[skill] = 0f;

                // Create button UI
                if (skillButtonPrefab != null && skillButtonParent != null)
                {
                    GameObject btnObj = Instantiate(skillButtonPrefab, skillButtonParent);
                    ActiveSkillButtonUI btn = btnObj.GetComponent<ActiveSkillButtonUI>();
                    if (btn != null)
                    {
                        btn.Setup(skill, activeSkillKeyLabels[nextKeyIndex], () =>
                        {
                            if (!IsOnCooldown(skill))
                            {
                                SkillInstance si = GetSkillInstance(skill);
                                if (si != null) ActivateSkill(si);
                            }
                        });
                        activeButtons[skill] = btn;
                    }
                }

                nextKeyIndex++;
            }
        }
    }

    // ─────────────────────── Skill Behaviours ───────────────────────

    private void SetupSkillBehaviour(SkillData skill, int level)
    {
        string sName = skill.skillName.ToLower();

        if (sName.Contains("bioluminescent") || sName.Contains("flare"))
        {
            flare = gameObject.AddComponent<BioluminescentFlare>();
            flare.SetLevel(level);
        }
        else if (sName.Contains("chemosynthesis") || sName.Contains("chemo"))
        {
            chemosynthesis = gameObject.AddComponent<Chemosynthesis>();
            chemosynthesis.SetLevel(level);
        }
        else if (sName.Contains("camouflage") || sName.Contains("camo"))
        {
            camouflage = gameObject.AddComponent<Camouflage>();
            camouflage.SetLevel(level);
        }
        else if (sName.Contains("venom") || sName.Contains("gland"))
        {
            venomGlands = gameObject.AddComponent<VenomGlands>();
            venomGlands.SetLevel(level);
        }
        else if (sName.Contains("echolocation") || sName.Contains("echo"))
        {
            echolocation = gameObject.AddComponent<Echolocation>();
            echolocation.SetLevel(level);
        }
    }

    private void UpdateSkillBehaviour(SkillData skill, int level)
    {
        string sName = skill.skillName.ToLower();

        if (sName.Contains("bioluminescent") || sName.Contains("flare"))
        {
            if (flare != null) flare.SetLevel(level);
            if (activeButtons.ContainsKey(skill))
                activeButtons[skill].UpdateMaxCooldown(flare.GetCooldown());
        }
        else if (sName.Contains("chemosynthesis") || sName.Contains("chemo"))
        {
            if (chemosynthesis != null) chemosynthesis.SetLevel(level);
        }
        else if (sName.Contains("camouflage") || sName.Contains("camo"))
        {
            if (camouflage != null) camouflage.SetLevel(level);
        }
        else if (sName.Contains("venom") || sName.Contains("gland"))
        {
            if (venomGlands != null) venomGlands.SetLevel(level);
        }
        else if (sName.Contains("echolocation") || sName.Contains("echo"))
        {
            if (echolocation != null) echolocation.SetLevel(level);
        }
    }

    // ─────────────────────── Activation ───────────────────────

    private void ActivateSkill(SkillInstance skill)
    {
        float cooldown = 0f;

        switch (skill.data.skillName)
        {
            case "Bioluminescent Flare":
                if (flare != null)
                {
                    flare.Activate();
                    cooldown = flare.GetCooldown();
                    ShowActiveOverlayBriefly(skill.data, 0.5f);
                }
                break;

            case "Camouflage":
                if (camouflage != null)
                {
                    camouflage.Activate();
                    cooldown = camouflage.GetCooldown();
                    ShowActiveOverlay(skill.data);
                }
                break;

            case "Echolocation":
                if (echolocation != null)
                {
                    echolocation.Activate();
                    cooldown = echolocation.GetCooldown();
                    ShowActiveOverlayBriefly(skill.data, 1f);
                }
                break;
        }

        // Start cooldown
        if (cooldown > 0f)
        {
            cooldownTimers[skill.data] = cooldown;
            if (activeButtons.ContainsKey(skill.data))
                activeButtons[skill.data].StartCooldown(cooldown);
        }
    }

    // ─────────────────────── Visual Overlay Helpers ───────────────────────

    /// <summary>
    /// Show an active skill overlay briefly (for instant-effect skills like Flare/Echolocation).
    /// </summary>
    private void ShowActiveOverlayBriefly(SkillData skill, float duration)
    {
        if (skillOverlays.ContainsKey(skill) && skillOverlays[skill] != null)
        {
            skillOverlays[skill].SetActive(true);
            StartCoroutine(HideOverlayAfter(skill, duration));
        }
    }

    /// <summary>
    /// Show an active skill overlay (stays until manually hidden, e.g., Camouflage).
    /// </summary>
    private void ShowActiveOverlay(SkillData skill)
    {
        if (skillOverlays.ContainsKey(skill) && skillOverlays[skill] != null)
            skillOverlays[skill].SetActive(true);
    }

    /// <summary>
    /// Called by skill behaviours (e.g., Camouflage.EndCamo) when an active skill ends.
    /// </summary>
    public void OnActiveSkillEnded(string skillName)
    {
        foreach (var pair in skillOverlays)
        {
            if (pair.Key.skillName == skillName && pair.Value != null)
            {
                pair.Value.SetActive(false);
                break;
            }
        }
    }

    private System.Collections.IEnumerator HideOverlayAfter(SkillData skill, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (skillOverlays.ContainsKey(skill) && skillOverlays[skill] != null)
            skillOverlays[skill].SetActive(false);
    }

    // ─────────────────────── Public Accessors ───────────────────────

    /// <summary>
    /// Get the player's current level for a given skill. Returns 0 if not owned.
    /// </summary>
    public int GetSkillLevel(SkillData skill)
    {
        SkillInstance instance = GetSkillInstance(skill);
        return instance != null ? instance.level : 0;
    }

    /// <summary>
    /// Get the SkillInstance for a given SkillData. Returns null if not owned.
    /// </summary>
    public SkillInstance GetSkillInstance(SkillData skill)
    {
        foreach (var s in acquiredSkills)
        {
            if (s.data == skill) return s;
        }
        return null;
    }

    /// <summary>
    /// Get a specific skill behaviour component by type.
    /// Used by TailWhipDamage to check for VenomGlands.
    /// </summary>
    public T GetSkillBehaviour<T>() where T : MonoBehaviour
    {
        return GetComponent<T>();
    }

    /// <summary>
    /// Check if a skill is currently on cooldown.
    /// </summary>
    public bool IsOnCooldown(SkillData skill)
    {
        return cooldownTimers.ContainsKey(skill) && cooldownTimers[skill] > 0f;
    }

    // ─────────────────────── Utilities ───────────────────────

    /// <summary>
    /// Fisher-Yates shuffle.
    /// </summary>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
