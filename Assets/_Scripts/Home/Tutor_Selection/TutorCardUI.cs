using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LayoutElement))]
[RequireComponent(typeof(CanvasGroup))]
public class TutorCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private LayoutElement layoutElement;
    private CanvasGroup canvasGroup;

    [Header("Width Settings")]
    public float normalWidth;
    public float hoveredWidth;
    public float animSpeed;

    private float targetWidth;

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
        canvasGroup = GetComponent<CanvasGroup>();
        targetWidth = normalWidth;
        layoutElement.preferredWidth = normalWidth;
    }

    private void Update()
    {
        if (!Mathf.Approximately(layoutElement.preferredWidth, targetWidth))
        {
            layoutElement.preferredWidth = Mathf.Lerp(layoutElement.preferredWidth, targetWidth, Time.deltaTime * animSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetWidth = hoveredWidth;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetWidth();
    }

    public void ResetWidth()
    {
        targetWidth = normalWidth;
    }

    // Set alpha instantly or use for external fading control
    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null) canvasGroup.alpha = alpha;
    }
}