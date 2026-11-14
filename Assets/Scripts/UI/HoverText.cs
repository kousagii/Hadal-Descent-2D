using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text buttonText;

    private Color normalColor;
    private Color hoverColor;

    private void Awake()
    {
        if (buttonText == null)
        {
            Debug.LogError($"HoverText on '{gameObject.name}' is missing its Button Text reference! Please drag the text component into the script in the Inspector.", this);
            return;
        }

        // Remember the color you set in the Inspector!
        normalColor = buttonText.color;

        if (gameObject.name.Contains("Start"))
            hoverColor = Color.green;
        else if (gameObject.name.Contains("Exit"))
            hoverColor = Color.red;
        else
            hoverColor = Color.yellow;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonText != null) buttonText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null) buttonText.color = normalColor;
    }
}