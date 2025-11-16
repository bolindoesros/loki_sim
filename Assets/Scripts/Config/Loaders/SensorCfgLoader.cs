using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnitySensors.Sensor;
using UnitySensors.Sensor.Camera;
using UnitySensors.ROS.Publisher;
using UnitySensors.ROS.Utils.Namespacing;

public class SensorCfgLoader : MonoBehaviour
{
    [Header("Sensor Prefabs by Type")]
    [SerializeField] GameObject cameraPrefab;
    [SerializeField] GameObject imuPrefab;
    //[SerializeField] GameObject lidarPrefab;
    //[SerializeField] GameObject gnssPrefab;

    Dictionary<string, GameObject> prefabMap;

    private void Awake()
    {
        prefabMap = new Dictionary<string, GameObject>
        {
            { "camera", cameraPrefab },
            { "imu", imuPrefab },
            //{ "lidar", lidarPrefab },
            //{ "gnss", gnssPrefab }
        };
    }

    public void LoadSensorConfigs(List<SensorCfg> sensorCfgs)
    {
        foreach (SensorCfg cfg in sensorCfgs)
        {
            if (!prefabMap.TryGetValue(cfg.type, out var prefab))
            {
                Debug.LogWarning($"SensorLoader: Unknown sensor type '{cfg.type}'");
                continue;
            }

            GameObject instance = Instantiate(prefab, transform);
            instance.name = cfg.name;

            var nsManager = instance.GetComponent<NamespaceManager>();
            if (nsManager != null)
            {
                nsManager.CurrentNamespace = cfg.rosNamespace;
            }

            // There can be multiple publishers (such as in the case of camera)
            var publishers = instance.GetComponentsInChildren<RosMsgPublisher>();
            foreach (var pub in publishers)
            {
                pub.Frequency = cfg.frequency;
            }
            if (publishers.Length == 1)
            {
                publishers[0].TopicName = cfg.rosTopic;
            }

            var sensors = instance.GetComponentsInChildren<UnitySensor>();
            foreach (var sensor in sensors)
            {
                sensor.frequency = cfg.frequency;
            }

            var phySensors = instance.GetComponentsInChildren<UnityPhysicsSensor>();
            foreach (var phySensor in phySensors)
            {
                phySensor.frequency = cfg.frequency;
            }

            // Assign properties automatically
            ApplySensorSpecificConfig(instance, cfg);

            instance.SetActive(cfg.enabled);
        }
    }

    private void ApplySensorSpecificConfig(GameObject instance, SensorCfg cfg)
    {
        switch (cfg)
        {
            case CameraCfg camCfg:
                instance.transform.localPosition = camCfg.origin.position;
                instance.transform.localRotation = Quaternion.Euler(camCfg.origin.rotation);

                var cams = instance.GetComponentsInChildren<CameraSensor>();
                foreach (var cam in cams)
                {
                    cam.Fov = camCfg.cameraSettings.fovDegrees;
                    cam.Resolution = new Vector2Int(camCfg.cameraSettings.width, camCfg.cameraSettings.height);
                }
                break;

            // Other sensors will be added later

            default:
                Debug.LogWarning($"SensorLoader: No handler for sensor type {cfg.type}");
                break;
        }
    }
}
