using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>A short burst of paper stars at the point of success.</summary>
public class FeedbackBurst : MonoBehaviour
{
    public Sprite star;
    public int count = 8;
    public float radius = 150f;

    public void Play(RectTransform parent, Vector2 centre)
    {
        StartCoroutine(Burst(parent, centre));
    }

    IEnumerator Burst(RectTransform parent, Vector2 centre)
    {
        if (star == null || parent == null) yield break;
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("feedback_star_" + i, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = centre;
            rt.sizeDelta = new Vector2(32f, 32f);
            var image = go.GetComponent<Image>();
            image.sprite = star;
            image.raycastTarget = false;
            image.color = new Color(1f, 0.88f, 0.38f, 0.95f);

            float angle = (Mathf.PI * 2f * i / count) + Random.Range(-0.12f, 0.12f);
            Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            float life = 0f;
            while (life < 0.75f)
            {
                life += Time.deltaTime;
                rt.anchoredPosition = centre + velocity * life
                    + new Vector2(0f, 100f * life * life);
                rt.localScale = Vector3.one * (1f - life / 0.9f);
                Color c = image.color;
                c.a = Mathf.Clamp01(1f - life / 0.75f);
                image.color = c;
                yield return null;
            }
            Destroy(go);
        }
    }
}
