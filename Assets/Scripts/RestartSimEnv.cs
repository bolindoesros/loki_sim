using Cinemachine;
using System.Collections;
using UnityEngine;
using UnitySensors.Sensor.TF;

public class RestartSimEnv : MonoBehaviour
{
    [SerializeField] GameObject simEnvPrefab;

    public void RestartScene()
    {
        // Start the coroutine to handle the delayed destruction
        StartCoroutine(RestartSceneRoutine());
    }

    private IEnumerator RestartSceneRoutine()
    {
        GameObject[] simEnvs = GameObject.FindGameObjectsWithTag("SimEnv");
        SceneCfgLoader sceneCfgLoader = FindFirstObjectByType<SceneCfgLoader>();
        if (simEnvs.Length == 1)
        {
            GameObject simEnv = simEnvs[0];
            Transform simEnvTransform = simEnv.transform;
            
            // Mark the old environment for destruction
            Destroy(simEnv);
            
            // Instantiate the new environment
            Instantiate(simEnvPrefab, simEnvTransform.position, simEnvTransform.rotation);
        }
        else
        {
            Debug.LogWarning("No SimEnv or multiple SimEnv objects found in the scene. LaunchActuator may not function correctly.");
        }

        // Reload the scene
        if (sceneCfgLoader != null)
        {
            sceneCfgLoader.LoadScene();

            RobotCfgLoader robot = FindFirstObjectByType<RobotCfgLoader>();
            CinemachineFreeLook cinemachineCamera = FindFirstObjectByType<CinemachineFreeLook>();

            if (robot != null &&  cinemachineCamera != null)
            {
                cinemachineCamera.Follow = robot.transform;
                cinemachineCamera.LookAt = robot.transform;
            }
        }

        // WAIT for the end of the frame so Unity can actually destroy the objects
        yield return new WaitForEndOfFrame();

        // Now it is safe to search the scene and refresh the children
        var tfLinks = FindObjectsByType<TFLink>(FindObjectsSortMode.None);
        foreach (var tfLink in tfLinks)
        {
            tfLink.RefreshChildren();
        }
    }
}
