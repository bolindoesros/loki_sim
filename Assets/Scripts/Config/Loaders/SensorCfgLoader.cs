using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnitySensors.Sensor;
using UnitySensors.Sensor.Camera;
using UnitySensors.ROS.Publisher;
using UnitySensors.ROS.Utils.Namespacing;
using MDS.ROS.Sensors;
using UnitySensors.Sensor.TF;
using UnitySensors.ROS.Publisher.Sensor;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class SensorCfgLoader : MonoBehaviour
{
    [Header("Sensor Prefabs by Type")]
    [SerializeField] GameObject cameraPrefab;
    [SerializeField] GameObject imuPrefab;
    [SerializeField] GameObject dvlPrefab;
    //[SerializeField] GameObject lidarPrefab;
    //[SerializeField] GameObject gnssPrefab;

    Dictionary<string, GameObject> prefabMap;

    void Awake()
    {
        prefabMap = new Dictionary<string, GameObject>
        {
            { "camera", cameraPrefab },
            { "imu", imuPrefab },
            { "dvl", dvlPrefab },
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

            // Check if a child GameObject with the same name already exists
            Transform existingChild = transform.Find(cfg.name);
            GameObject instance;
            if (existingChild != null)
            {
                Debug.Log($"SensorLoader: Sensor '{cfg.name}' already exists, skipping instantiation.");
                instance = existingChild.gameObject;
            }
            else
            {
                // Instantiate sensor prefab and assign names
                instance = Instantiate(prefab, transform);
                instance.name = cfg.name;
            }

            // Set transform
            instance.transform.position = transform.root.TransformPoint(FLU.ConvertToRUF(cfg.pose.position));
            instance.transform.rotation = transform.root.rotation * Quaternion.Euler(FLU.ConvertToRUF(cfg.pose.rotation));

            // Set namespace
            var nsManager = instance.GetComponent<NamespaceManager>();
            if (nsManager != null)
            {
                nsManager.CurrentNamespace = cfg.rosNamespace;
            }

            // Set frame id for TFLink
            var tfLink = instance.GetComponentInChildren<TFLink>();
            if (tfLink != null)
            {
                tfLink.FrameId = cfg.frameId;
            }

            // Set frequency for all publishers
            var publishers = instance.GetComponentsInChildren<RosMsgPublisher>();
            foreach (var pub in publishers)
            {
                pub.Frequency = cfg.frequency;
            }
            // Set topic name if only one publisher exists (for cameras which have multiple publishers, ignore)
            if (publishers.Length == 1)
            {
                publishers[0].TopicName = cfg.rosTopic;
            }

            // Set sensor frequency
            var sensors = instance.GetComponentsInChildren<UnitySensor>();
            foreach (var sensor in sensors)
            {
                if (sensor is not TFLink) sensor.frequency = cfg.frequency;
            }

            var phySensors = instance.GetComponentsInChildren<UnityPhysicsSensor>();
            foreach (var phySensor in phySensors)
            {
                phySensor.frequency = cfg.frequency;
            }

            // Assign properties automatically
            ApplySensorSpecificConfig(instance, cfg);

            // Enable/disable sensor
            instance.SetActive(cfg.enabled);
        }
    }

    void ApplySensorSpecificConfig(GameObject instance, SensorCfg cfg)
    {
        switch (cfg)
        {
            case CameraCfg camCfg:               
                var cams = instance.GetComponentsInChildren<CameraSensor>();
                foreach (var cam in cams)
                {
                    cam.Fov = camCfg.cameraSettings.fovDegrees;
                    cam.Resolution = new Vector2Int(camCfg.cameraSettings.width, camCfg.cameraSettings.height);
                }
                var imagePublishers = instance.GetComponentsInChildren<CompressedImageMsgPublisher>();
                foreach (var publisher in imagePublishers)
                {
                    publisher.Frequency = cfg.frequency;
                }
                if (imagePublishers.Length == 1)
                {
                    imagePublishers[0].Serializer.Header.FrameId = cfg.frameId;
                }
                break;

            case ImuCfg imuCfg:
                var imuPublisher = instance.GetComponentInChildren<ImuPublisher>();
                imuPublisher.Serializer.withGravity = imuCfg.imuSettings.withGravity;
                imuPublisher.Serializer.linearAccelerationCovariance = imuCfg.imuSettings.linearCovariance;
                imuPublisher.Serializer.angularVelocityCovariance = imuCfg.imuSettings.angularCovariance;
                imuPublisher.Serializer.orientationCovariance = imuCfg.imuSettings.orientationCovariance;
                imuPublisher.Serializer.Header.FrameId = cfg.frameId;
                break;

            case DvlCfg dvlCfg:
                var dvlPublisher = instance.GetComponentInChildren<DvlPublisher>();
                dvlPublisher.Serializer.covariance = dvlCfg.dvlSettings.covariance;
                dvlPublisher.Serializer.Header.FrameId = cfg.frameId;
                break;

            // Other sensors will be added later

            default:
                Debug.LogWarning($"SensorLoader: No handler for sensor type {cfg.type}");
                break;
        }
    }

    /// <summary>
    /// Scans through all children of the transform, identifies valid sensor types,
    /// and saves their configuration data to the provided RobotCfg asset.
    /// </summary>
    public void SaveSensorConfigsToAsset(RobotCfg robotCfg)
    {
        if (robotCfg == null)
        {
            Debug.LogError("SensorCfgLoader: RobotCfg asset is null");
            return;
        }

        robotCfg.sensors = new List<SensorCfg>();

        foreach (Transform child in transform)
        {
            string sensorType = IdentifySensorType(child.gameObject);
            if (string.IsNullOrEmpty(sensorType))
                continue;

            SensorCfg cfg = CreateSensorConfig(child.gameObject, sensorType);
            if (cfg != null)
            {
                robotCfg.sensors.Add(cfg);
            }
        }

        Debug.Log($"SensorCfgLoader: Saved {robotCfg.sensors.Count} sensor(s) to {robotCfg.name}");
    }

    string IdentifySensorType(GameObject sensorObject)
    {
        // Check against prefab map types by comparing components
        if (sensorObject.GetComponentInChildren<CompressedImageMsgPublisher>() != null)
            return "camera";
        if (sensorObject.GetComponentInChildren<ImuPublisher>() != null)
            return "imu";
        if (sensorObject.GetComponentInChildren<DvlPublisher>() != null)
            return "dvl";

        return null;
    }

    SensorCfg CreateSensorConfig(GameObject sensorObject, string sensorType)
    {
        SensorCfg cfg = null;

        switch (sensorType)
        {
            case "camera":
                cfg = CreateCameraConfig(sensorObject);
                break;
            case "imu":
                cfg = CreateImuConfig(sensorObject);
                break;
            case "dvl":
                cfg = CreateDvlConfig(sensorObject);
                break;
            default:
                Debug.LogWarning($"SensorCfgLoader: Unknown sensor type '{sensorType}'");
                break;
        }

        if (cfg != null)
        {
            // Common properties
            cfg.name = sensorObject.name;
            cfg.type = sensorType;
            cfg.enabled = sensorObject.activeSelf;
            cfg.pose.position = FLU.ConvertFromRUF(transform.root.InverseTransformPoint(sensorObject.transform.position));
            cfg.pose.rotation = FLU.ConvertFromRUF((Quaternion.Inverse(transform.root.rotation) * sensorObject.transform.rotation).eulerAngles);

            // Get namespace
            if (sensorObject.TryGetComponent<NamespaceManager>(out var nsManager))
            {
                cfg.rosNamespace = nsManager.CurrentNamespace;
            }

            TFLink tFLink = sensorObject.GetComponentInChildren<TFLink>();
            if (tFLink != null)
            {
                cfg.frameId = tFLink.FrameId;
            }

            // Get frequency and topic from publisher
            var publishers = sensorObject.GetComponentsInChildren<RosMsgPublisher>();
            if (publishers.Length > 0)
            {
                var publisher = publishers[0];
                cfg.frequency = publisher.Frequency;
                if (publishers.Length == 1) cfg.rosTopic = publisher.TopicName;
            }
        }

        return cfg;
    }

    CameraCfg CreateCameraConfig(GameObject sensorObject)
    {
        var cam = sensorObject.GetComponentInChildren<CameraSensor>();
        var imagePublisher = sensorObject.GetComponentInChildren<CompressedImageMsgPublisher>();
        if (cam == null || imagePublisher == null) return null;

        CameraCfg cfg = new CameraCfg();
        cfg.cameraSettings.width = cam.Resolution.x;
        cfg.cameraSettings.height = cam.Resolution.y;
        cfg.cameraSettings.fovDegrees = (int)cam.Fov;
        cfg.frequency = imagePublisher.Frequency;

        return cfg;
    }

    ImuCfg CreateImuConfig(GameObject sensorObject)
    {
        var imuPublisher = sensorObject.GetComponentInChildren<ImuPublisher>();
        if (imuPublisher == null) return null;

        ImuCfg cfg = new ImuCfg();
        cfg.imuSettings.withGravity = imuPublisher.Serializer.withGravity;
        cfg.imuSettings.linearCovariance = imuPublisher.Serializer.linearAccelerationCovariance;
        cfg.imuSettings.angularCovariance = imuPublisher.Serializer.angularVelocityCovariance;
        cfg.imuSettings.orientationCovariance = imuPublisher.Serializer.orientationCovariance;
        return cfg;
    }

    DvlCfg CreateDvlConfig(GameObject sensorObject)
    {
        var dvlPublisher = sensorObject.GetComponentInChildren<DvlPublisher>();
        if (dvlPublisher == null) return null;

        DvlCfg cfg = new DvlCfg();
        cfg.dvlSettings.covariance = dvlPublisher.Serializer.covariance;
        return cfg;
    }
}
