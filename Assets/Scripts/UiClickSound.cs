using UnityEngine;
using UnityEngine.UI;

/// <summary>Adds the same gentle wooden click to every button.</summary>
[RequireComponent(typeof(Button))]
public class UiClickSound : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        });
    }
}
