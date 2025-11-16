using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters.Math;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    SensorCfgLoader sensorCfgLoader;
    string filePath = "robot.json";
    RobotCfg robotCfg;

    private void Awake()
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
#if UNITY_EDITOR
            NullValueHandling = NullValueHandling.Include,
#else
            NullValueHandling = NullValueHandling.Ignore,
#endif
        };

        filePath = Path.Combine(Application.persistentDataPath, filePath);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            robotCfg = JsonConvert.DeserializeObject<RobotCfg>(json);
        } else
        {
            Debug.LogWarning($"SensorLoader: Config file not found at {filePath}");
            robotCfg = CreateSampleConfig();
        }


        //RobotCfg robotCfg = JsonConvert.DeserializeObject<RobotCfg>(json, jsonSettings);
        if (robotCfg == null || robotCfg.sensors == null)
        {
            Debug.LogWarning("SensorLoader: Failed to load sensors from JSON.");
            robotCfg = CreateSampleConfig();
        }

        Debug.Log($"RobotCfgLoader: Loaded {robotCfg.sensors.Count} sensors from config.");
        sensorCfgLoader = GetComponentInChildren<SensorCfgLoader>();
        if (sensorCfgLoader != null)
        {
            sensorCfgLoader.LoadSensorConfigs(robotCfg.sensors);
        }
    }

    private RobotCfg CreateSampleConfig()
    {
        RobotCfg defaultCfg = new ()
        {
            sensors = new List<SensorCfg>
            {
                new CameraCfg
                {
                    name = "FrontCamera",
                    type = "camera",
                    enabled = true,
                    frameId = "front_camera_link",
                    frequency = 30.0f,
                    rosNamespace = "front_camera",
                    rosTopic = "image_raw",
                    //origin = new Pose
                    //{
                    //    position = new Vector3(0.2f, 0.0f, 0.1f),
                    //    rotation = new Vector3(0.0f, 0.0f, 0.0f)
                    //}
                },
                new ImuCfg
                {
                    name = "MainIMU",
                    type = "imu",
                    enabled = true,
                    frameId = "imu_link",
                    frequency = 50.0f,
                    rosNamespace = "mavros",
                    rosTopic = "imu/data"
                }
            }
        };

        string jsonString = JsonConvert.SerializeObject(defaultCfg);
        File.WriteAllText(filePath, jsonString);
        Debug.Log("Created sample robot config at " + filePath);

        return defaultCfg;
    }
}
