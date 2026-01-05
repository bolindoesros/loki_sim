using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Find the root canvas to calculate scale correctly
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Draggable Window must be a child of a Canvas!");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Moves this window to the top of the draw order so it's in front of others
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Divide delta by canvas scale factor to keep movement 1:1 with mouse
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}