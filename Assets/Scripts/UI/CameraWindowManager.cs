using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraWindowManager : MonoBehaviour
{
    [SerializeField] Vector2Int resolution = new(640, 480);
    [SerializeField] GameObject displayWindowPrefab;

    GameObject displayWindow;
    Camera targetCamera;

    void Start()
    {
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
        {
            Debug.LogError("CameraWindowManager: No Camera component found on the GameObject!");
            return;
        }

        if (displayWindowPrefab == null)
        {
            Debug.LogError($"CameraWindowManager: null display window prefab!");
            return;
        }

        // Instantiate the prefab under Canvas
        Transform canvasTransform = CanvasUtils.GetCanvasTransformInActiveScene();
        if (canvasTransform == null)
        {
            Debug.LogError("CameraWindowManager: No Canvas found in the scene!");
            return;
        }
        displayWindow = Instantiate(displayWindowPrefab, canvasTransform);

        // Position the window in the bottom right corner
        if (displayWindow.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.anchoredPosition = new Vector2(-20, 20);
        }

        // Find the RawImage component in the prefab
        RawImage rawImage = displayWindow.GetComponentInChildren<RawImage>();
        if (rawImage == null)
        {
            Debug.LogError("Prefab does not contain a RawImage component!");
            return;
        }

        // Create or get the RenderTexture and assign it to the camera
        RenderTexture rt;
        if (targetCamera.targetTexture == null)
        {
            // Create the RenderTexture at runtime
            rt = new(resolution.x, resolution.y, 24)
            {
                name = "RuntimeRT_" + targetCamera.name,
                filterMode = FilterMode.Bilinear
            };
            rt.Create();
            targetCamera.targetTexture = rt;
        }
        else
        {
            rt = targetCamera.targetTexture;
        }

        // Assign the RenderTexture to the RawImage
        rawImage.texture = rt;

        // Get the button of name "Close button" and add listener to it (there could be multiple buttons)
        Button[] buttons = displayWindow.GetComponentsInChildren<Button>();
        bool closeButtonFound = false;
        foreach (var button in buttons)
        {
            if (button.name == "Close button")
            {
                button.onClick.AddListener(() => ToggleDisplayWindow(false));
                closeButtonFound = true;
                break;
            }
        }
        if (!closeButtonFound) Debug.LogWarning("No button named 'Close button' found in the display window prefab.");

        // If this is Perception Camera, assign the RawImage and disable visualizations
        //if (TryGetComponent<PerceptionCamera>(out var pCam))
        //{
        //    pCam.outputView = rawImage;
        //    pCam.SetVisualizationActive(false);
        //}

        // Adjust aspect ratio
        if (displayWindow.TryGetComponent<AspectRatioFitter>(out var aspectFitter))
        {
            aspectFitter.aspectRatio = (float)resolution.x / resolution.y;
        }

        displayWindow.name = targetCamera.name + " panel";
        displayWindow.SetActive(false); // Default inactive
        Debug.Log($"Successfully linked {targetCamera.name} to {displayWindow.name}");
    }

    /// <summary>
    /// For UI events.
    /// </summary>
    /// <param name="isOn"></param>
    public void ToggleDisplayWindow(bool isOn)
    {
        if (displayWindow == null)
        {
            Debug.Log("CameraWindowManager: displayWindow is null!");
            return;
        }
        displayWindow.SetActive(isOn);
        //if (TryGetComponent<PerceptionCamera>(out var pCam))
        //    pCam.SetVisualizationActive(isOn);
    }

    void OnDestroy()
    {
        if (targetCamera != null && targetCamera.targetTexture != null)
        {
            RenderTexture rt = targetCamera.targetTexture;
            targetCamera.targetTexture = null;
            rt.Release();
            Destroy(rt);
        }
    }
}