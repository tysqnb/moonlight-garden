using UnityEngine;
using UnityEngine.UI;

/// <summary>Keeps the home-page read-to-me chip in step with the audio state.</summary>
[RequireComponent(typeof(Button))]
public class NarrationChip : MonoBehaviour
{
    public Text label;
    public Image icon;
    public Sprite iconOn;
    public Sprite iconOff;

    bool shown;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Toggle);
        Refresh();
    }

    void Update()
    {
        if (shown != AudioManager.NarrationEnabled) Refresh();
    }

    void Toggle()
    {
        AudioManager.NarrationEnabled = !AudioManager.NarrationEnabled;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetNarration(AudioManager.NarrationEnabled);
        Refresh();
    }

    void Refresh()
    {
        shown = AudioManager.NarrationEnabled;
        if (label != null) label.text = shown ? "Read to me: on" : "Read to me: off";
        if (icon != null) icon.sprite = shown ? iconOn : iconOff;
    }
}
