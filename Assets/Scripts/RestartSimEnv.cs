using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartSimEnv : MonoBehaviour
{
    [SerializeField] GameObject simEnvPrefab;

    public void RestartScene()
    {
        GameObject[] simEnvs = GameObject.FindGameObjectsWithTag("SimEnv");
        if (simEnvs.Length == 1)
        {
            GameObject simEnv = simEnvs[0];
            Transform simEnvTransform = simEnv.transform;
            Destroy(simEnv);
            Instantiate(simEnvPrefab, simEnvTransform.position, simEnvTransform.rotation);
        }
        else
        {
            Debug.LogWarning("No SimEnv or multiple SimEnv objects found in the scene. LaunchActuator may not function correctly.");
        }
    }
}
