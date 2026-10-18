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
    public bool soundToggle;
    public bool narrationToggle;

    void Start()
    {
        if (soundToggle) state = AudioManager.SoundEnabled;
        if (narrationToggle) state = AudioManager.NarrationEnabled;
        GetComponent<Button>().onClick.AddListener(Toggle);
        Apply();
    }

    void Toggle()
    {
        state = !state;
        Apply();
        if (AudioManager.Instance == null) return;
        if (soundToggle) AudioManager.Instance.SetSound(state);
        if (narrationToggle) AudioManager.Instance.SetNarration(state);
    }

    void Apply()
    {
        if (icon != null)
            icon.sprite = state ? iconOn : iconOff;
    }
}
