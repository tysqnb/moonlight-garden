using UnityEngine;
using UnityEngine.UI;

/// <summary>Scene 1: tap the firefly, it warms up and the second lantern lights.</summary>
[RequireComponent(typeof(Button))]
public class TapToWake : MonoBehaviour
{
    public RectTransform firefly;
    public Image glow;
    public GameObject cheerText;

    float t = -1f;
    bool awake;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Wake);
        if (glow != null) glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, 0f);
        if (cheerText != null) cheerText.SetActive(false);
    }

    void Wake()
    {
        if (awake) return;
        awake = true;
        t = 0f;
        ProgressManager.Light(2);
        if (cheerText != null) cheerText.SetActive(true);
    }

    void Update()
    {
        if (t < 0f || t >= 1f) return;
        t = Mathf.Min(1f, t + Time.deltaTime * 1.4f);

        if (firefly != null)
        {
            float s = 1f + 0.16f * Mathf.Sin(t * Mathf.PI);
            firefly.localScale = new Vector3(s, s, 1f);
        }
        if (glow != null)
        {
            Color c = glow.color;
            c.a = 0.85f * t;
            glow.color = c;
        }
    }
}
