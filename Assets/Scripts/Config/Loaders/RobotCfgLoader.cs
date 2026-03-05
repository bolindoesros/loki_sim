using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnitySensors.ROS.Publisher.Tf2;
using UnitySensors.ROS.Utils.Namespacing;
using UnitySensors.Sensor.TF;
using MDS.FlightController;

/// <summary>
/// A loader for the entire robot settings from a RobotCfg SO file.
/// Can run standalone or be called by SceneCfgLoader for multi-robot setups.
/// </summary>
public class RobotCfgLoader : MonoBehaviour
{
    public RobotCfg robotCfg;

    [Tooltip("Name of the json file to load. Should be placed in the SimSettings folder and end with '.json'. " +
        "If left empty or invalid, will use a default file name based on the GameObject's name." +
        "If provided but does not exist, will read directly from RobotCfg SO and create a new file at SimSettings to store the settings.")]
    public string fileName;

    [Tooltip("Type of robot, representing different robot types with entirely different visuals and behaviors. " +
        "This setting is only useful to SceneCfgLoader.")]
    public string robotType;

    SensorCfgLoader sensorCfgLoader;

    void Awake()
    {
        if (!enabled)
            return;

        // Check if SceneCfgLoader is present to avoid duplicate loading
        var sceneCfgLoader = FindAnyObjectByType<SceneCfgLoader>();
        if (sceneCfgLoader != null)
            return;

        SaveJsonUtility.SetJsonDefaultSettings();

        // Resolve file path
        string filePath;
        if (SaveJsonUtility.IsValidFileName(fileName))
            filePath = SaveJsonUtility.GetFullSavePath(fileName);
        else 
            filePath = SaveJsonUtility.GetSavePathForTransform(this.transform);

        // Load or create config file
        if (File.Exists(filePath))
        {
            Debug.Log("File exists at " + filePath + ", loading robot config.");
            string json = File.ReadAllText(filePath);
            robotCfg = ScriptableObject.CreateInstance<RobotCfg>();
            JsonConvert.PopulateObject(json, robotCfg);
        }
        else
        {
            Debug.LogWarning($"RobotCfgLoader: Config file not found at {filePath}. Using provided RobotCfg instead.");
            string jsonString = JsonConvert.SerializeObject(robotCfg);
            File.WriteAllText(filePath, jsonString);
            Debug.Log("Created default robot config at " + filePath);
        }

        LoadRobotConfig(robotCfg);
    }

    public void LoadRobotConfig(RobotCfg cfg)
    {
        if (cfg == null || cfg.sensors == null)
        {
            Debug.LogError("SensorLoader: Failed to load sensors from JSON.");
        }

        if (TryGetComponent<NamespaceManager>(out var nsManager))
        {
            nsManager.CurrentNamespace = cfg.rosNamespace;
        }
        else Debug.LogWarning("RobotCfgLoader: No NamespaceManager found on robot to set namespace.");

        if (TryGetComponent<TFLink>(out var tfLink))
        {
            tfLink.FrameId = cfg.baseLinkId;
        }
        else Debug.LogWarning("RobotCfgLoader: No TFLink found on robot to set base link ID.");

        if (TryGetComponent<TFMessageMsgPublisher>(out var tfPublisher))
        {
            tfPublisher.enabled = cfg.tfPublisher.enabled;
            tfPublisher.Frequency = cfg.tfPublisher.frequency;
            tfPublisher.TopicName = cfg.tfPublisher.rosTopic;
            tfPublisher.Serializer.recurseFindChildLinks = cfg.tfPublisher.publishChildLinks;
        }
        else Debug.LogWarning("RobotCfgLoader: No TFMessageMsgPublisher found on robot to set TF publisher config.");

        if (TryGetComponent<FC_Base>(out var fc))
        {
            if (cfg.controllerCfg is ArdupilotControllerCfg ardupilotCfg)
            {
                fc.controllerType = ControllerType.Ardupilot;
                fc.apSitlSettings.sitlFreq = ardupilotCfg.apSitlSettings.sitlFreq;
                fc.apSitlSettings.sitlPort = ardupilotCfg.apSitlSettings.sitlPort;
            }
            else if (cfg.controllerCfg is RosControllerCfg rosControlCfg)
            {
                fc.controllerType = ControllerType.ROS;
                fc.rosControlSettings.pwmTopicName = rosControlCfg.rosControlSettings.pwmTopicName;
            }
            else
            {
                Debug.LogWarning("RobotCfgLoader: Unsupported controller config type. Using default FC settings.");
            }
        }
        else Debug.LogWarning("RobotCfgLoader: No FC_Base found on robot to set flight controller config.");

        sensorCfgLoader = GetComponentInChildren<SensorCfgLoader>();
        if (sensorCfgLoader != null)
        {
            sensorCfgLoader.LoadSensorConfigs(cfg.sensors);
            Debug.Log($"RobotCfgLoader: Loaded {cfg.sensors.Count} sensors from config.");
        }
        else Debug.LogWarning("RobotCfgLoader: No SensorCfgLoader found on robot to load sensor configs.");
    }
}