using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// In-game button for an active skill. Shows the skill icon, keybind label,
/// and a cooldown overlay. Clicking the button also activates the skill.
/// </summary>
public class ActiveSkillButtonUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text keybindText;       // "K", "L", or ";"
    public Image cooldownOverlay;       // A filled image for cooldown display (radial fill)
    public TMP_Text cooldownText;       // Optional: shows remaining seconds
    public Button activateButton;

    private SkillData assignedSkill;
    private float cooldownTimer = 0f;
    private float maxCooldown = 0f;
    private System.Action onActivate;

    /// <summary>
    /// Initialize this button for a specific active skill.
    /// </summary>
    public void Setup(SkillData skill, string keybindLabel, System.Action activateCallback)
    {
        assignedSkill = skill;
        onActivate = activateCallback;

        if (iconImage != null && skill.icon != null)
            iconImage.sprite = skill.icon;

        if (keybindText != null)
            keybindText.text = keybindLabel;

        maxCooldown = skill.cooldown;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.gameObject.SetActive(false);
        }

        if (cooldownText != null)
            cooldownText.text = "";

        if (activateButton != null)
        {
            activateButton.onClick.RemoveAllListeners();
            activateButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        if (cooldownTimer <= 0f)
        {
            onActivate?.Invoke();
        }
    }

    /// <summary>
    /// Start the cooldown visual. Called by SkillManager when the skill is activated.
    /// </summary>
    public void StartCooldown(float duration)
    {
        maxCooldown = duration;
        cooldownTimer = duration;

        if (cooldownOverlay != null)
            cooldownOverlay.gameObject.SetActive(true);
    }

    /// <summary>
    /// Update the cooldown to reflect a new max cooldown (e.g., Flare level 2 reduces by 10%).
    /// </summary>
    public void UpdateMaxCooldown(float newMax)
    {
        maxCooldown = newMax;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = cooldownTimer / maxCooldown;

            if (cooldownText != null)
                cooldownText.text = $"{Mathf.CeilToInt(cooldownTimer)}s";

            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                if (cooldownOverlay != null)
                {
                    cooldownOverlay.fillAmount = 0f;
                    cooldownOverlay.gameObject.SetActive(false);
                }
                if (cooldownText != null)
                    cooldownText.text = "";
            }
        }
    }
}
