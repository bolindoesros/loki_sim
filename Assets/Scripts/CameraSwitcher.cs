using UnityEngine;
using UnityEngine.Perception.GroundTruth;

public class PerceptionCameraSwitcher : MonoBehaviour
{
    /// <summary>
    /// Switch between multiple Cameras (including PerceptionCameras) using a hotkey.
    /// It only sets active/inactive the Camera component, not the entire GameObject.
    /// For PerceptionCameras, it enables/disables the visualization, lets the Camera component always be enabled, but modifies its target display accordingly.
    /// </summary>

    [Header("Camera List")]
    [SerializeField] Camera[] _cameras;

    [Header("Hotkey")]
    [SerializeField] KeyCode _switchKey = KeyCode.Tab;

    int _currentCameraIndex = 0;
    readonly int _mainDisplay = 0;
    readonly int _unusedDisplay = 1;

    void Start()
    {
        if (_cameras == null || _cameras.Length == 0)
        {
            Debug.LogWarning("CameraSwitcher: No cameras assigned!");
            return;
        }

        // Deactivate all cameras
        for (int i = 0; i < _cameras.Length; i++)
        {
            DeactivateCamera(_cameras[i]);
        }

        // Activate the first valid camera
        ActivateCamera(_currentCameraIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(_switchKey))
        {
            if (_cameras == null || _cameras.Length == 0)
                return;

            // Deactivate current camera
            if (_cameras[_currentCameraIndex] != null)
            {
                DeactivateCamera(_cameras[_currentCameraIndex]);
            }

            // Move to next camera
            _currentCameraIndex = (_currentCameraIndex + 1) % _cameras.Length;
            ActivateCamera(_currentCameraIndex);
        }
    }

    void ActivateCamera(int index)
    {
        // Check if camera is valid
        if (_cameras[index] == null || !_cameras[index].gameObject.activeInHierarchy)
        {
            // Try next camera recursively
            int nextIndex = (index + 1) % _cameras.Length;
            if (nextIndex != _currentCameraIndex) // Prevent infinite loop
            {
                _currentCameraIndex = nextIndex;
                ActivateCamera(nextIndex);
            }
            else
            {
                Debug.LogWarning("PerceptionCameraSwitcher: No valid cameras found!");
            }
            return;
        }

        // Activate the selected camera
        Camera cam = _cameras[index];
        cam.targetDisplay = _mainDisplay;
        cam.enabled = true;

        // If it's a PerceptionCamera, enable visualization
        if (cam.TryGetComponent<PerceptionCamera>(out var pCam))
        {
            pCam.SetVisualizationActive(true);
            pCam.enabled = true;
        }
    }

    void DeactivateCamera(Camera cam)
    {
        if (cam == null)
            return;

        cam.targetDisplay = _unusedDisplay;
        if (cam.TryGetComponent<PerceptionCamera>(out var pCam))
        {
            // Keep component enabled so it keeps capturing but disable visualization
            pCam.SetVisualizationActive(false);
            pCam.enabled = true;
        }
        else
        {
            cam.enabled = false;
        }
    }
}
