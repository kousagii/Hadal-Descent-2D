using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Component on each of the 3 skill cards in the selection UI.
/// Holds references to its own icon, texts, and select button.
/// </summary>
public class SkillCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text levelText;     // "NEW!" or "Lv.2 → Lv.3"
    public TMP_Text typeText;      // "Active" or "Passive"
    public TMP_Text descriptionText;
    public Button selectButton;

    private SkillData assignedSkill;
    private System.Action<SkillData> onSelectCallback;

    /// <summary>
    /// Populate this card with skill info.
    /// </summary>
    public void Setup(SkillData skill, int currentLevel, System.Action<SkillData> callback)
    {
        assignedSkill = skill;
        onSelectCallback = callback;

        if (iconImage != null && skill.icon != null)
            iconImage.sprite = skill.icon;

        if (nameText != null)
            nameText.text = skill.skillName;

        if (typeText != null)
            typeText.text = skill.skillType == SkillType.Active ? "Active" : "Passive";

        if (levelText != null)
        {
            if (currentLevel == 0)
                levelText.text = "NEW!";
            else
                levelText.text = $"Lv.{currentLevel} → Lv.{currentLevel + 1}";
        }

        if (descriptionText != null)
        {
            // Show the description for the level the player WILL have after selecting
            int nextLevel = currentLevel == 0 ? 1 : currentLevel + 1;
            int descIndex = Mathf.Clamp(nextLevel - 1, 0, skill.levelDescriptions.Length - 1);
            descriptionText.text = skill.levelDescriptions[descIndex];
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelectClicked);
        }
    }

    private void OnSelectClicked()
    {
        onSelectCallback?.Invoke(assignedSkill);
    }
}
