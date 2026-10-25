using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LayoutElement))]
public class TutorCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private LayoutElement layoutElement;

    [Header("Width Settings")]
    public float normalWidth;
    public float hoveredWidth; // Width when zoomed in
    public float animSpeed;     // Speed of the smooth transition

    private float targetWidth;

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
        targetWidth = normalWidth;
        layoutElement.preferredWidth = normalWidth;
    }

    private void Update()
    {
        // Smoothly lerp towards the target width every frame
        if (!Mathf.Approximately(layoutElement.preferredWidth, targetWidth))
        {
            layoutElement.preferredWidth = Mathf.Lerp(layoutElement.preferredWidth, targetWidth, Time.deltaTime * animSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetWidth = hoveredWidth;
        // Removed SetAsLastSibling() to prevent reshuffling!
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetWidth();
    }

    public void ResetWidth()
    {
        targetWidth = normalWidth;
    }
}