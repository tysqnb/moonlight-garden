using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Makes every tappable control respond immediately to touch.</summary>
public class UiPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    Vector3 homeScale = Vector3.one;

    void Awake()
    {
        homeScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = homeScale * 0.96f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = homeScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = homeScale;
    }
}
