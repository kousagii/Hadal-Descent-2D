using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Full-screen overlay panel that shows 3 random skill cards on level up.
/// Game is paused (Time.timeScale = 0) while this panel is visible.
/// </summary>
public class SkillSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The full overlay panel GameObject")]
    public GameObject panel;

    [Tooltip("The 3 skill card slots")]
    public SkillCardUI[] skillCards; // Drag 3 SkillCardUI objects here

    /// <summary>
    /// Show the selection UI with the given candidates.
    /// </summary>
    /// <param name="candidates">List of SkillData to offer (up to 3)</param>
    /// <param name="getLevel">Function to get the player's current level for a given skill (0 = not owned)</param>
    /// <param name="onSelect">Callback when the player selects a skill</param>
    public void Show(List<SkillData> candidates, System.Func<SkillData, int> getLevel, System.Action<SkillData> onSelect)
    {
        if (panel != null)
            panel.SetActive(true);

        // Pause the game
        Time.timeScale = 0f;

        for (int i = 0; i < skillCards.Length; i++)
        {
            if (i < candidates.Count)
            {
                skillCards[i].gameObject.SetActive(true);
                int currentLevel = getLevel(candidates[i]);
                skillCards[i].Setup(candidates[i], currentLevel, (selected) =>
                {
                    onSelect?.Invoke(selected);
                    Hide();
                });
            }
            else
            {
                // Fewer than 3 candidates — hide extra card slots
                skillCards[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Hide the selection UI and resume the game.
    /// </summary>
    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);

        // Resume the game
        Time.timeScale = 1f;
    }
}
