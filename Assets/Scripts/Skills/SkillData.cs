using UnityEngine;

public enum SkillType { Passive, Active }

[CreateAssetMenu(fileName = "NewSkill", menuName = "Hadal/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    [TextArea] public string[] levelDescriptions; // 5 entries, one per level
    public SkillType skillType;
    public Sprite icon; 

    [Header("Active Skill Settings")]
    [Tooltip("Cooldown in seconds — only relevant for Active skills")]
    public float cooldown = 5f;

    [Header("Visual Overlay")]
    [Tooltip("The PNG sprite from Images/Skills visual/ to overlay on the player")]
    public Sprite visualOverlay;
}
