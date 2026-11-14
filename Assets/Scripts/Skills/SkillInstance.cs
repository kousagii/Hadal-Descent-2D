[System.Serializable]
public class SkillInstance
{
    public SkillData data;
    public int level = 1; // 1–5
    public bool synergiesUnlocked = false; // Set true at level 5 — hook for future synergy system

    public bool IsMaxLevel => level >= 5;
    public string CurrentDescription => data.levelDescriptions[level - 1];
}
