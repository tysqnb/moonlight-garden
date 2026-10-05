using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Scene 4: press and hold the bud until the ring fills and it opens.</summary>
public class PressHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image ringFill;
    public Image budImage;
    public Image glow;
    public Sprite budClosed;
    public Sprite budOpen;
    public float holdSeconds = 3f;

    bool holding;
    bool opened;
    float t;

    void Start()
    {
        if (budImage != null) budImage.sprite = budClosed;
        if (ringFill != null) ringFill.fillAmount = 0f;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!opened) holding = true;
    }

    public void OnPointerUp(PointerEventData e)
    {
        holding = false;
    }

    void Update()
    {
        if (holding && !opened)
        {
            t = Mathf.Min(1f, t + Time.deltaTime / holdSeconds);
            if (ringFill != null) ringFill.fillAmount = t;

            if (t >= 1f)
            {
                opened = true;
                holding = false;
                if (budImage != null) budImage.sprite = budOpen;
                ProgressManager.Light(5);
            }
        }

        if (glow != null)
        {
            Color c = glow.color;
            c.a = 0.55f * t;
            glow.color = c;
        }
    }
}
