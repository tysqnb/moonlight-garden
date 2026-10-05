using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Scene 3: swipe across the reeds to call the breeze.</summary>
public class SwipeDetect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform reeds;
    public RectTransform shoot;
    public Image wind;
    public Canvas canvas;
    public float minDistance = 80f;
    public float minSpeed = 200f;

    Vector2 startPos;
    float startTime;
    bool blown;

    public void OnPointerDown(PointerEventData e)
    {
        startPos = e.position;
        startTime = Time.time;
    }

    public void OnPointerUp(PointerEventData e)
    {
        float dt = Mathf.Max(0.04f, Time.time - startTime);
        float distance = Vector2.Distance(e.position, startPos);
        if (distance < minDistance || distance / dt < minSpeed) return;

        blown = true;
        ProgressManager.Light(4);
        StopAllCoroutines();
        StartCoroutine(Blow());
    }

    IEnumerator Blow()
    {
        if (wind != null) wind.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2.2f;
            float bend = Mathf.Sin(Mathf.Min(t, 1f) * Mathf.PI) * 14f;
            if (reeds != null) reeds.localRotation = Quaternion.Euler(0f, 0f, -bend);
            if (shoot != null)
            {
                float grow = 1f + 0.22f * Mathf.Sin(Mathf.Min(t, 1f) * Mathf.PI);
                shoot.localScale = new Vector3(grow, grow, 1f);
            }
            if (wind != null)
            {
                Color c = wind.color;
                c.a = 0.75f * Mathf.Sin(Mathf.Min(t, 1f) * Mathf.PI);
                wind.color = c;
            }
            yield return null;
        }

        if (reeds != null) reeds.localRotation = Quaternion.identity;
        if (shoot != null) shoot.localScale = new Vector3(1.18f, 1.18f, 1f);
        if (wind != null)
        {
            Color c = wind.color;
            c.a = 0.35f;
            wind.color = c;
        }
    }
}
