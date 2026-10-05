using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Scene 2: drag the dew drop onto the seed.</summary>
public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform drop;
    public RectTransform target;
    public Canvas canvas;
    public Image soil;
    public GameObject shoot;
    public float snapDistance = 150f;
    public Color soilWatered = new Color(0.55f, 0.46f, 0.34f);

    Vector2 home;
    Vector2 grabOffset;
    bool solved;

    void Start()
    {
        if (drop != null) home = drop.anchoredPosition;
        if (shoot != null) shoot.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (solved) return;
        drop.localScale = new Vector3(1.12f, 1.12f, 1f);

        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)drop.parent, e.position, null, out local))
            grabOffset = local - drop.anchoredPosition;
    }

    public void OnDrag(PointerEventData e)
    {
        if (solved) return;

        // Follow the pointer exactly, whatever the canvas scale factor is.
        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)drop.parent, e.position, null, out local))
            drop.anchoredPosition = local - grabOffset;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (solved) return;
        drop.localScale = Vector3.one;

        if (Vector2.Distance(drop.anchoredPosition, target.anchoredPosition) < snapDistance)
        {
            solved = true;
            drop.anchoredPosition = target.anchoredPosition;
            if (soil != null) soil.color = soilWatered;
            if (shoot != null) shoot.SetActive(true);
            ProgressManager.Light(3);
        }
        else
        {
            drop.anchoredPosition = home;
        }
    }
}
