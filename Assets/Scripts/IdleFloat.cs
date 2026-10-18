using UnityEngine;

/// <summary>Small, slow movement that makes paper objects feel alive.</summary>
public class IdleFloat : MonoBehaviour
{
    public float amplitude = 7f;
    public float speed = 0.8f;
    public float rotation = 1.2f;
    public float phase;

    RectTransform rt;
    Vector2 home;
    float baseAngle;

    void Start()
    {
        rt = transform as RectTransform;
        if (rt != null) home = rt.anchoredPosition;
        baseAngle = transform.localEulerAngles.z;
    }

    void Update()
    {
        if (rt == null) return;
        float t = Time.time * speed + phase;
        rt.anchoredPosition = home + new Vector2(Mathf.Sin(t * 0.73f) * amplitude * 0.3f,
                                                 Mathf.Sin(t) * amplitude);
        if (Mathf.Abs(rotation) > 0.01f)
            transform.localRotation = Quaternion.Euler(0f, 0f, baseAngle + Mathf.Sin(t * 0.8f) * rotation);
    }
}
