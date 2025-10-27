using UnityEngine;
using UnityEngine.Perception.GroundTruth;

public class PerceptionCameraSwitcher : MonoBehaviour
{
    /// <summary>
    /// Switch between multiple Cameras (including PerceptionCameras) using a hotkey.
    /// It only sets active/inactive the Camera component, not the entire GameObject.
    /// For PerceptionCameras, it enables/disables the visualization, lets the Camera component always be enabled, but modifies its depth accordingly.
    /// </summary>

    [Header("Camera List")]
    [SerializeField] Camera[] m_Cameras;

    [Header("Hotkey")]
    [SerializeField] KeyCode m_SwitchKey = KeyCode.Tab;

    int m_CurrentCameraIndex = 0;
    readonly int m_CameraDepthActive = 5;
    readonly int m_CameraDepthInactive = -5;

    void Start()
    {
        if (m_Cameras == null || m_Cameras.Length == 0)
        {
            Debug.LogWarning("PerceptionCameraSwitcher: No cameras assigned!");
            return;
        }

        // Deactivate all cameras
        for (int i = 0; i < m_Cameras.Length; i++)
        {
            DeactivateCamera(m_Cameras[i]);
        }

        // Activate the first valid camera
        ActivateCamera(m_CurrentCameraIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(m_SwitchKey))
        {
            if (m_Cameras == null || m_Cameras.Length == 0)
                return;

            // Deactivate current camera
            if (m_Cameras[m_CurrentCameraIndex] != null)
            {
                DeactivateCamera(m_Cameras[m_CurrentCameraIndex]);
            }

            // Move to next camera
            m_CurrentCameraIndex = (m_CurrentCameraIndex + 1) % m_Cameras.Length;
            ActivateCamera(m_CurrentCameraIndex);
        }
    }

    void ActivateCamera(int index)
    {
        // Check if camera is valid
        if (m_Cameras[index] == null || !m_Cameras[index].gameObject.activeInHierarchy)
        {
            // Try next camera recursively
            int nextIndex = (index + 1) % m_Cameras.Length;
            if (nextIndex != m_CurrentCameraIndex) // Prevent infinite loop
            {
                m_CurrentCameraIndex = nextIndex;
                ActivateCamera(nextIndex);
            }
            else
            {
                Debug.LogWarning("PerceptionCameraSwitcher: No valid cameras found!");
            }
            return;
        }

        // Activate the selected camera
        Camera cam = m_Cameras[index];
        cam.depth = m_CameraDepthActive;
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

        cam.depth = m_CameraDepthInactive;
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
