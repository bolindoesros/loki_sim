using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnitySensors.ROS.Publisher.Tf2;
using UnitySensors.ROS.Utils.Namespacing;
using UnitySensors.Sensor.TF;

public class BaseFirstContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var props = base.CreateProperties(type, memberSerialization);

        // Sort so that base class properties come first
        return props
            .OrderBy(p => GetInheritanceDepth(p.DeclaringType))
            .ThenBy(p => p.Order ?? 0)
            .ToList();
    }

    private int GetInheritanceDepth(Type type)
    {
        int depth = 0;
        while (type.BaseType != null)
        {
            depth++;
            type = type.BaseType;
        }
        return depth;
    }
}
public class RobotCfgLoader : MonoBehaviour
{
    public RobotCfg robotCfg;

    SensorCfgLoader sensorCfgLoader;

    void Awake()
    {
        if (!enabled)
            return;

        SetJsonDefaultSettings();

        string filePath = SavePathUtility.GetSavePathForTransform(transform);
        if (File.Exists(filePath))
        {
            Debug.Log("File exists at " + filePath + ", loading robot config.");
            string json = File.ReadAllText(filePath);
            robotCfg = ScriptableObject.CreateInstance<RobotCfg>();
            JsonConvert.PopulateObject(json, robotCfg);
        }
        else
        {
            Debug.LogWarning($"SensorLoader: Config file not found at {filePath}. Using provided RobotCfg instead.");
            string jsonString = JsonConvert.SerializeObject(robotCfg);
            File.WriteAllText(filePath, jsonString);
            Debug.Log("Created default robot config at " + filePath);
        }

        if (robotCfg == null || robotCfg.sensors == null)
        {
            Debug.LogError("SensorLoader: Failed to load sensors from JSON."); 
        }

        if (TryGetComponent<NamespaceManager>(out var nsManager))
        {
            nsManager.CurrentNamespace = robotCfg.rosNamespace;
        }
        else Debug.LogWarning("RobotCfgLoader: No NamespaceManager found on robot to set namespace.");

        if (TryGetComponent<TFLink>(out var tfLink))
        {
            tfLink.FrameId = robotCfg.baseLinkId;
        }
        else Debug.LogWarning("RobotCfgLoader: No TFLink found on robot to set base link ID.");

        if (TryGetComponent<TFMessageMsgPublisher>(out var tfPublisher))
        {
            tfPublisher.enabled = robotCfg.tfPublisher.enabled;
            tfPublisher.Frequency = robotCfg.tfPublisher.frequency;
            tfPublisher.TopicName = robotCfg.tfPublisher.rosTopic;
            tfPublisher.Serializer.recurseFindChildLinks = robotCfg.tfPublisher.publishChildLinks;
        }
        else Debug.LogWarning("RobotCfgLoader: No TFMessageMsgPublisher found on robot to set TF publisher config.");

        sensorCfgLoader = GetComponentInChildren<SensorCfgLoader>();
        if (sensorCfgLoader != null)
        {
            sensorCfgLoader.LoadSensorConfigs(robotCfg.sensors);
            Debug.Log($"RobotCfgLoader: Loaded {robotCfg.sensors.Count} sensors from config.");
        }
        else Debug.LogWarning("RobotCfgLoader: No SensorCfgLoader found on robot to load sensor configs.");
    }

    public void SetJsonDefaultSettings()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new Vector3Converter(),
                new SensorCfgConverter()
            },
            Formatting = Formatting.Indented,
            ContractResolver = new BaseFirstContractResolver(),
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}

public static class SavePathUtility
{
    public static string GetSavePath(string fileName)
    {
        // Path: [Project Root]/SimSettings/
        string directoryPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "SimSettings");

        // Ensure the directory exists so you don't get an IO error
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return Path.Combine(directoryPath, fileName);
    }

    public static string GetSavePathForTransform(Transform transform)
    {
        string fileName = $"{transform.name.ToLower().Replace(" ", "_")}.json";
        return GetSavePath(fileName);
    }
}