using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Content")]
    public string introTitle = "Welcome to Hadal";
    [TextArea(3, 10)]
    public List<string> introPages = new List<string>
    {
        "Welcome to the dark depths of the ocean. Your goal is to explore, survive, and uncover the mysteries of the deep.",
        "Controls:\n- WASD or Arrows to Move\n- Left Click to Attack\n- Tab to open the Bestiary\n- Keys 1, 2, 3 to use Skills",
        "You are currently in the Sunlight Zone. Explore this area and document every species you find before descending deeper!"
    };

    private void Start()
    {
        // Delay the popup slightly so it doesn't trigger before PopupManager is ready
        Invoke("ShowIntro", 0.5f);
    }

    private void ShowIntro()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(introTitle, introPages);
        }
        else
        {
            Debug.LogWarning("PopupManager missing! Cannot show intro tutorial.");
        }
    }
}
