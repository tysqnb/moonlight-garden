using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires a button to a navigation action at runtime, so the link survives
/// being saved into a scene (editor-time listeners do not).
/// 0 = back, 1 = forward, 2 = read again
/// </summary>
[RequireComponent(typeof(Button))]
public class NavButton : MonoBehaviour
{
    public ScreenController controller;
    public int action = 1;

    void Start()
    {
        Button button = GetComponent<Button>();
        if (controller == null) return;

        if (action == 0) button.onClick.AddListener(controller.Back);
        else if (action == 2) button.onClick.AddListener(controller.ReadAgain);
        else button.onClick.AddListener(controller.Next);
    }
}
