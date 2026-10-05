using UnityEngine;
using UnityEngine.UI;

/// <summary>Narration and sound toggles: swap the icon and remember the state.</summary>
[RequireComponent(typeof(Button))]
public class ToggleIcon : MonoBehaviour
{
    public Image icon;
    public Sprite iconOn;
    public Sprite iconOff;
    public bool state = true;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Toggle);
        Apply();
    }

    void Toggle()
    {
        state = !state;
        Apply();
    }

    void Apply()
    {
        if (icon != null)
            icon.sprite = state ? iconOn : iconOff;
    }
}
