using UnityEngine;
using UnityEngine.UI;

/// <summary>The script card replays that page's spoken line when tapped.</summary>
[RequireComponent(typeof(Button))]
public class NarrationReplay : MonoBehaviour
{
    public int pageIndex;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayNarration(pageIndex);
        });
    }
}
