using UnityEngine;
using UnityEngine.UI;

public class UIRawImageController : MonoBehaviour
{
    private RectTransform rectTransform;
    private UIDraggableWindow dragScript;

    // Storage for the "Floating" state
    private Vector2 savedAnchorMin;
    private Vector2 savedAnchorMax;
    private Vector2 savedSizeDelta;
    private Vector2 savedAnchoredPosition;
    private Vector2 savedPivot;

    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalSizeDelta;
    private Vector2 originalAnchoredPosition;
    private Vector2 originalPivot;

    private bool isFullScreen = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        dragScript = GetComponent<UIDraggableWindow>();

        originalAnchorMin = rectTransform.anchorMin;
        originalAnchorMax = rectTransform.anchorMax;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalSizeDelta = rectTransform.sizeDelta;
        originalPivot = rectTransform.pivot;
    }

    public void ReturnToOriginalPosition()
    {
        rectTransform.anchorMin = originalAnchorMin;
        rectTransform.anchorMax = originalAnchorMax;
        rectTransform.anchoredPosition = originalAnchoredPosition;
        rectTransform.sizeDelta = originalSizeDelta;
        rectTransform.pivot = originalPivot;

        if (dragScript != null) dragScript.enabled = true;
        isFullScreen = false;
    }

    public void ToggleFullScreen()
    {
        if (!isFullScreen)
        {
            // 1. SAVE: Store every property that defines the floating window
            savedAnchorMin = rectTransform.anchorMin;
            savedAnchorMax = rectTransform.anchorMax;
            savedAnchoredPosition = rectTransform.anchoredPosition;
            savedSizeDelta = rectTransform.sizeDelta;
            savedPivot = rectTransform.pivot;

            // 2. APPLY FULLSCREEN: Stretch to all corners
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            // Force pivot to center for fullscreen stability
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            if (dragScript != null) dragScript.enabled = false;
            isFullScreen = true;
        }
        else
        {
            // 3. RESTORE: Put everything back exactly as it was
            rectTransform.pivot = savedPivot;
            rectTransform.anchorMin = savedAnchorMin;
            rectTransform.anchorMax = savedAnchorMax;
            rectTransform.sizeDelta = savedSizeDelta;
            rectTransform.anchoredPosition = savedAnchoredPosition;

            if (dragScript != null) dragScript.enabled = true;
            isFullScreen = false;
        }
    }

    public void CloseWindow() => gameObject.SetActive(false);
}