using MDS.ROS.Sensors;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnitySensors.DataType.LiDAR;
using UnitySensors.ROS.Publisher;
using UnitySensors.ROS.Publisher.Camera;
using UnitySensors.ROS.Publisher.Sensor;
using UnitySensors.ROS.Utils.Namespacing;
using UnitySensors.ROS.Utils.Lifecycle;
using UnitySensors.Sensor;
using UnitySensors.Sensor.Camera;
using UnitySensors.Sensor.LiDAR;
using UnitySensors.Sensor.TF;

/// <summary>
/// A loader that instantiates and configures sensors on a robot based on a list of SensorCfg objects.
/// Meant to be run with RobotCfgLoader, not standalone.
/// </summary>
public class SensorCfgLoader : MonoBehaviour
{
    [Header("Sensor Prefabs by Type")]
    [SerializeField] GameObject cameraPrefab;
    [SerializeField] GameObject imuPrefab;
    [SerializeField] GameObject dvlPrefab;
    [SerializeField] GameObject lidarPrefab;
    [SerializeField] GameObject gnssPrefab;

    Dictionary<string, GameObject> prefabMap;

    void Awake()
    {
        if (!enabled)
            return;

        if (GetComponentInParent<RobotCfgLoader>() == null)
        {
            Debug.LogWarning("SensorCfgLoader: RobotCfgLoader not exist. Not running loader.");
            return;
        }

        prefabMap = new Dictionary<string, GameObject>
        {
            { "camera", cameraPrefab },
            { "imu", imuPrefab },
            { "dvl", dvlPrefab },
            { "lidar", lidarPrefab },
            { "gnss", gnssPrefab }
        };
    }

    public void LoadSensorConfigs(List<SensorCfg> sensorCfgs)
    {
        // First, destroy all existing sensor children (child transform of this gameobject) to avoid duplicates
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        foreach (SensorCfg cfg in sensorCfgs)
        {
            if (!prefabMap.TryGetValue(cfg.type, out var prefab))
            {
                Debug.LogWarning($"SensorLoader: Unknown sensor type '{cfg.type}'");
                continue;
            }

            // Instantiate sensor prefab as child of this gameobject
            GameObject instance = Instantiate(prefab, transform);
            instance.name = cfg.name;

            // Set transform
            instance.transform.SetPositionAndRotation(
                transform.root.TransformPoint(FLU.ConvertToRUF(cfg.pose.position)),
                transform.root.rotation * Quaternion.Euler(-FLU.ConvertToRUF(cfg.pose.rotation))
            );

            // Set namespace
            if (instance.TryGetComponent<NamespaceManager>(out var nsManager))
            {
                nsManager.CurrentNamespace = cfg.rosNamespace;
            }

            // Set frame id for TFLink (if only one exists; for sensors with multiple TFLinks like stereo cameras, ignore)
            var tfLinks = instance.GetComponentsInChildren<TFLink>();
            if (tfLinks.Length == 1)
            {
                tfLinks[0].FrameId = cfg.frameId;
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
                if (sensor is not TFLink) sensor.Frequency = cfg.frequency;
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

            // Set lifecycle state
            var lifecycleManager = instance.GetComponentInChildren<LifecycleManager>();
            if (lifecycleManager != null && cfg.lifecycleSettings != null)
            {
                lifecycleManager.NodeName = cfg.lifecycleSettings.nodeName;
                if (!cfg.enabled)
                    lifecycleManager.CurrentLifecycleState = LifecycleState.Unconfigured;
                else if (cfg.enabled && !cfg.lifecycleSettings.active)
                    lifecycleManager.CurrentLifecycleState = LifecycleState.Inactive;
                else if (cfg.enabled && cfg.lifecycleSettings.active)
                    lifecycleManager.CurrentLifecycleState = LifecycleState.Active;
                // If !enabled but active, it's an invalid state. Quietly ignore.
            }

            Debug.Log($"SensorLoader: Loaded sensor '{cfg.name}' of type '{cfg.type}'");
        }

        // Force refresh after creating new sensors
        if (TryGetComponent<TFLink>(out var sensorTfLink))
            sensorTfLink.RefreshChildren();
    }

    void ApplySensorSpecificConfig(GameObject instance, SensorCfg cfg)
    {
        switch (cfg)
        {
            case CameraCfg camCfg:
                var cameraSensor = instance.GetComponentInChildren<CameraSensor>();
                if (cameraSensor != null)
                {
                    cameraSensor.Fov = camCfg.cameraSettings.fovDegrees;
                    cameraSensor.Resolution = new Vector2Int(camCfg.cameraSettings.width, camCfg.cameraSettings.height);
                    cameraSensor.Frequency = cfg.frequency;
                }

                var compressedImgPub = instance.GetComponentInChildren<CompressedImageMsgPublisher>();
                if (compressedImgPub != null)
                {
                    compressedImgPub.TopicName = camCfg.cameraRosSettings.imageTopic;
                    compressedImgPub.Frequency = cfg.frequency;
                    compressedImgPub.Serializer.Header.FrameId = cfg.frameId;
                }

                var cameraInfoPub = instance.GetComponentInChildren<CameraInfoMsgPublisher>();
                if (cameraInfoPub != null)
                {
                    cameraInfoPub.TopicName = camCfg.cameraRosSettings.cameraInfoTopic;
                    cameraInfoPub.Serializer.Header.FrameId = cfg.frameId;
                }

                break;

            case StereoCameraCfg stereoCamCfg:               
                var cameraSensors = instance.GetComponentsInChildren<CameraSensor>();
                foreach (var cam in cameraSensors)
                {
                    cam.Fov = stereoCamCfg.cameraSettings.fovDegrees;
                    cam.Resolution = new Vector2Int(stereoCamCfg.cameraSettings.width, stereoCamCfg.cameraSettings.height);
                    cam.Frequency = cfg.frequency;
                }

                var compressedImgPubs = instance.GetComponentsInChildren<CompressedImageMsgPublisher>();
                foreach (var publisher in compressedImgPubs)
                    publisher.Frequency = cfg.frequency;
                // TODO: Set frame id for the image publisher and info publisher of each camera in stereo camera rig
                // For now, these info will be hardcoded in the prefab
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

            case GnssCfg gnssCfg:
                var gnssPublisher = instance.GetComponentInChildren<GnssPublisher>();
                gnssPublisher.Serializer.covariance = gnssCfg.gnssSettings.covariance;
                gnssPublisher.Serializer.Header.FrameId = cfg.frameId;
                break;

            case LidarCfg lidarCfg:
                var lidar = instance.GetComponentInChildren<LiDARSensor>();
                lidar.minRange = lidarCfg.lidarSettings.minRange;
                lidar.maxRange = lidarCfg.lidarSettings.maxRange;
                lidar.gaussianNoiseSigma = lidarCfg.lidarSettings.gaussianNoiseSigma;
                lidar.maxIntensity = lidarCfg.lidarSettings.maxIntensity;

                var scanPattern = CreateScanPattern(lidarCfg);
                lidar.scanPattern = scanPattern;
                lidar.pointsNum = scanPattern.size;

                var lidarPublisher = instance.GetComponentInChildren<LiDARPointCloud2MsgPublisher>();
                lidarPublisher.Serializer.Header.FrameId = cfg.frameId;
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
        if (sensorObject.GetComponentInChildren<PerceptionCamera>() != null)
            return "perception-camera";
        if (sensorObject.GetComponentInChildren<GnssPublisher>() != null)
            return "gnss";
        if (sensorObject.GetComponentInChildren<LiDARSensor>() != null)
            return "lidar";
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
            case "gnss":
                cfg = CreateGnssConfig(sensorObject);
                break;
            case "lidar":
                cfg = CreateLidarConfig(sensorObject);
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
            cfg.pose.rotation = -FLU.ConvertFromRUF((Quaternion.Inverse(transform.root.rotation) * sensorObject.transform.rotation).eulerAngles);

            // Get namespace
            if (sensorObject.TryGetComponent<NamespaceManager>(out var nsManager))
            {
                cfg.rosNamespace = nsManager.CurrentNamespace;
            } else Debug.LogWarning($"SensorCfgLoader: NamespaceManager not found on sensor '{cfg.name}'");

            // Get frame id
            TFLink tFLink = sensorObject.GetComponentInChildren<TFLink>();
            if (tFLink != null)
            {
                cfg.frameId = tFLink.FrameId;
            } else Debug.LogWarning($"SensorCfgLoader: TFLink not found on sensor '{cfg.name}'");

            // Get frequency and topic from publisher
            var publishers = sensorObject.GetComponentsInChildren<RosMsgPublisher>();
            if (publishers.Length > 0)
            {
                var publisher = publishers[0];
                cfg.frequency = publisher.Frequency;
                if (publishers.Length == 1) cfg.rosTopic = publisher.TopicName;
            } else Debug.LogWarning($"SensorCfgLoader: No RosMsgPublisher found on sensor '{cfg.name}'");

            // Get lifecycle state (there should be only one LifecycleManager)
            var lifecycleManager = sensorObject.GetComponentInChildren<LifecycleManager>();
            if (lifecycleManager != null)
            {
                cfg.lifecycleSettings = new()
                {
                    nodeName = lifecycleManager.NodeName,
                    active = lifecycleManager.CurrentLifecycleState == LifecycleState.Active
                };
            }
            else cfg.lifecycleSettings = null;
        }

        return cfg;
    }

    CameraCfg CreateCameraConfig(GameObject sensorObject)
    {
        var cam = sensorObject.GetComponentInChildren<CameraSensor>();
        var imagePublisher = sensorObject.GetComponentInChildren<CompressedImageMsgPublisher>();
        var cameraInfoPublisher = sensorObject.GetComponentInChildren<CameraInfoMsgPublisher>();
        if (cam == null || imagePublisher == null || cameraInfoPublisher == null)
        {
            Debug.LogError("SensorCfgLoader: Camera or Image Publisher or Camera Info Publisher not found");
            return null;
        }

        CameraCfg cfg = new();
        cfg.cameraSettings.width = cam.Resolution.x;
        cfg.cameraSettings.height = cam.Resolution.y;
        cfg.cameraSettings.fovDegrees = (int)cam.Fov;
        cfg.frequency = imagePublisher.Frequency;
        cfg.cameraRosSettings.imageTopic = imagePublisher.TopicName;
        cfg.cameraRosSettings.cameraInfoTopic = cameraInfoPublisher.TopicName;

        return cfg;
    }

    ImuCfg CreateImuConfig(GameObject sensorObject)
    {
        var imuPublisher = sensorObject.GetComponentInChildren<ImuPublisher>();
        if (imuPublisher == null)
        {
            Debug.LogError("SensorCfgLoader: IMU Publisher not found");
            return null;
        }

        ImuCfg cfg = new();
        cfg.imuSettings.withGravity = imuPublisher.Serializer.withGravity;
        cfg.imuSettings.linearCovariance = imuPublisher.Serializer.linearAccelerationCovariance;
        cfg.imuSettings.angularCovariance = imuPublisher.Serializer.angularVelocityCovariance;
        cfg.imuSettings.orientationCovariance = imuPublisher.Serializer.orientationCovariance;
        return cfg;
    }

    DvlCfg CreateDvlConfig(GameObject sensorObject)
    {
        var dvlPublisher = sensorObject.GetComponentInChildren<DvlPublisher>();
        if (dvlPublisher == null)
        {
            Debug.LogError("SensorCfgLoader: DVL Publisher not found");
            return null;
        }

        DvlCfg cfg = new();
        cfg.dvlSettings.covariance = dvlPublisher.Serializer.covariance;
        return cfg;
    }

    GnssCfg CreateGnssConfig(GameObject sensorObject)
    {
        var gnssPublisher = sensorObject.GetComponentInChildren<GnssPublisher>();
        if (gnssPublisher == null)
        {
            Debug.LogError("SensorCfgLoader: GNSS Publisher not found");
            return null;
        }

        GnssCfg cfg = new();
        cfg.gnssSettings.covariance = gnssPublisher.Serializer.covariance;
        return cfg;
    }

    LidarCfg CreateLidarConfig(GameObject sensorObject)
    {
        var lidarSensor = sensorObject.GetComponentInChildren<LiDARSensor>();
        var lidarPublisher = sensorObject.GetComponentInChildren<LiDARPointCloud2MsgPublisher>();
        if (lidarSensor == null || lidarPublisher == null)
        {
            Debug.LogError("SensorCfgLoader: LiDAR Sensor or Publisher not found");
            return null;
        }

        LidarCfg cfg = new();
        cfg.lidarSettings.minRange = lidarSensor.minRange;
        cfg.lidarSettings.maxRange = lidarSensor.maxRange;
        cfg.lidarSettings.gaussianNoiseSigma = lidarSensor.gaussianNoiseSigma;
        cfg.lidarSettings.maxIntensity = lidarSensor.maxIntensity;
        cfg.lidarSettings.minAzimuthAngle = lidarSensor.scanPattern.minAzimuthAngle;
        cfg.lidarSettings.maxAzimuthAngle = lidarSensor.scanPattern.maxAzimuthAngle;
        cfg.lidarSettings.minZenithAngle = lidarSensor.scanPattern.minZenithAngle;
        cfg.lidarSettings.maxZenithAngle = lidarSensor.scanPattern.maxZenithAngle;

        int zenithCount = 1;
        float firstAzimuth = Mathf.Atan2(lidarSensor.scanPattern.scans[0].x, lidarSensor.scanPattern.scans[0].z) * Mathf.Rad2Deg;

        for (int i = 1; i < lidarSensor.scanPattern.size; i++)
        {
            float currentAzimuth = Mathf.Atan2(lidarSensor.scanPattern.scans[i].x, lidarSensor.scanPattern.scans[i].z) * Mathf.Rad2Deg;
            if (Mathf.Abs(currentAzimuth - firstAzimuth) > 0.01f)
                break;
            zenithCount++;
        }

        cfg.lidarSettings.zenithAngleResolution = zenithCount;
        cfg.lidarSettings.azimuthAngleResolution = lidarSensor.scanPattern.size / zenithCount;

        return cfg;
    }

    ScanPattern CreateScanPattern(LidarCfg lidarCfg)
    {
        LidarSettings cfg = lidarCfg.lidarSettings;
        ScanPattern scan = ScriptableObject.CreateInstance<ScanPattern>();

        scan.size = cfg.zenithAngleResolution * cfg.azimuthAngleResolution;
        scan.scans = new float3[scan.size];

        float[] zenithAngles = new float[cfg.zenithAngleResolution];
        for (int i = 0; i < cfg.zenithAngleResolution; i++)
        {
            zenithAngles[i] = Mathf.Lerp(cfg.minZenithAngle, cfg.maxZenithAngle, (float)i / cfg.zenithAngleResolution);
        }

        int index = 0;
        for (int azimuth = 0; azimuth < cfg.azimuthAngleResolution; azimuth++)
        {
            float azimuthAngle = Mathf.Lerp(cfg.minAzimuthAngle, cfg.maxAzimuthAngle, (float)(azimuth) / cfg.azimuthAngleResolution);
            foreach (float zenithAngle in zenithAngles)
            {
                scan.scans[index] = Quaternion.Euler(-zenithAngle, azimuthAngle, 0) * Vector3.forward;
                index++;
            }
        }

        scan.minAzimuthAngle = cfg.minAzimuthAngle;
        scan.maxAzimuthAngle = cfg.maxAzimuthAngle;

        scan.minZenithAngle = cfg.minZenithAngle;
        scan.maxZenithAngle = cfg.maxZenithAngle;

        return scan;
    }
}
