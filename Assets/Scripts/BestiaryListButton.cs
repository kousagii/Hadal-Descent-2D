using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BestiaryListButton : MonoBehaviour
{
    [Tooltip("Type the exact speciesID matching your Bestiary Database asset entry here (case-sensitive)")]
    public string targetSpeciesID;

    private void Start()
    {
        // Automatically find the Button component on this Fish item and listen for a click
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        if (!string.IsNullOrEmpty(targetSpeciesID))
        {
            // Tell the main BestiaryUI script to jump directly to this fish's data!
            BestiaryUI.Instance.OpenToEntry(targetSpeciesID);
        }
    }
}