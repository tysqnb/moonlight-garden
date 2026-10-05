using UnityEngine;
using UnityEngine.UI;

/// <summary>Tapping a lantern jumps to that page.</summary>
[RequireComponent(typeof(Button))]
public class LanternButton : MonoBehaviour
{
    public ScreenController controller;
    public int index;

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null && controller != null)
            button.onClick.AddListener(() => controller.JumpTo(index));
    }
}
