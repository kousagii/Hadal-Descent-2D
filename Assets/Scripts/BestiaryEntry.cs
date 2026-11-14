using UnityEngine;

[System.Serializable]
public class BestiaryEntry
{
    [Tooltip("Must match the speciesID exactly (e.g. 'Clownfish', 'GreatWhiteShark')")]
    public string speciesID;
    
    [Tooltip("If true, this entry is visible immediately")]
    public bool unlockedByDefault = false;

    [Tooltip("The actual display name in the UI")]
    public string displayName;

    [Tooltip("The zone this species belongs to (e.g. 'Sunlight Zone')")]
    public string zone = "Sunlight Zone";

    [Header("Visuals")]
    [Tooltip("The in-game sprite of the organism (shown as silhouette when locked)")]
    public Sprite gameSprite;

    [Tooltip("Real-world photo of this fish")]
    public Sprite realPhoto; 

    [Header("Info Text")]
    [TextArea(3, 5)]
    [Tooltip("Description of this marine organism")]
    public string description;
    
    [TextArea(3, 5)]
    [Tooltip("Trivia about this organism")]
    public string trivia;
}
