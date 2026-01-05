using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject sensorsLayoutPrefab; // The VerticalLayoutGroup container
    [SerializeField] GameObject sensorControlPrefab; // The row with Name, Toggle, and Button
    [SerializeField] string sensorUIEnableButtonName = "Toggle Sensor UI";

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        Transform canvasTransform = CanvasUtils.GetCanvasTransformInActiveScene();
        if (canvasTransform == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return;
        }

        GameObject sensorsLayout = Instantiate(sensorsLayoutPrefab, canvasTransform);
        sensorsLayout.name = transform.root.name + " sensors UI";
        if (sensorsLayout.TryGetComponent<RectTransform>(out var rectTransform))
        {
            // Set anchor to bottom-left corner
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0);

            // Position near bottom-left corner (adjust offset as needed)
            rectTransform.anchoredPosition = new Vector2(10, 70);
        }
        Transform layoutParent = sensorsLayout.transform;

        // Create sensor controls for each child sensor
        foreach (Transform sensor in transform)
        {
            CreateSensorControl(sensor, layoutParent);
        }

        // Disable layout by default
        sensorsLayout.SetActive(false);

        // Find game object with the specified name to toggle UI visibility
        GameObject toggleButtonObject = GameObject.Find(sensorUIEnableButtonName);
        if (toggleButtonObject == null)
        {
            Debug.LogWarning($"No GameObject named '{sensorUIEnableButtonName}' found to toggle sensor UI.");
            return;
        }
        else
        {
            if (toggleButtonObject.TryGetComponent<Toggle>(out var toggle))
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = sensorsLayout.activeSelf;
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    sensorsLayout.SetActive(isOn);
                });
            }
            else
            {
                Debug.LogWarning($"GameObject '{sensorUIEnableButtonName}' does not have a Toggle component.");
            }
        }
    }

    void CreateSensorControl(Transform sensor, Transform parent)
    {
        GameObject sensorControl = Instantiate(sensorControlPrefab, parent);
        sensorControl.name = sensor.name;

        // This is rather hard-coded so make sure that the prefab structure matches
        TextMeshProUGUI sensorNameText = sensorControl.GetComponentInChildren<TextMeshProUGUI>();
        Toggle enableToggle = sensorControl.GetComponentInChildren<Toggle>();
        Button viewButton = sensorControl.GetComponentInChildren<Button>();
        if (sensorNameText == null || enableToggle == null || viewButton == null)
        {
            Debug.LogWarning("Sensor control prefab is missing required components: TextMeshProUGUI, Toggle and Button.");
            return;
        }

        // Setup Text
        sensorNameText.text = sensor.name;

        // Setup Toggle (Enable/Disable sensor GameObject)
        enableToggle.isOn = sensor.gameObject.activeSelf;
        enableToggle.onValueChanged.AddListener((isOn) =>
        {
            sensor.gameObject.SetActive(isOn);
        });

        // Setup Button (Check for CameraWindowManager)
        CameraWindowManager[] cameraWindows = sensor.GetComponentsInChildren<CameraWindowManager>();
        viewButton.gameObject.SetActive(cameraWindows.Length > 0);
        foreach (var cameraWindow in cameraWindows)
        {
            viewButton.onClick.AddListener(() => cameraWindow.ToggleDisplayWindow(true));
        }
    }
}