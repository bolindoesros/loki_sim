using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnitySensors.Sensor.GNSS;
using UnitySensors.DataType.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Cinemachine;

[System.Serializable]
public class RobotType
{
    public string type;
    public GameObject prefab;
}

public class SceneCfgLoader : MonoBehaviour
{
    public SceneCfg sceneCfg;

    [Tooltip("Name of the json file to load. Should be placed in the SimSettings folder and end with '.json'. " +
        "If left empty or invalid, will use a default file name called scene.json. " +
        "If provided but does not exist, will read directly from sceneCfg SO and create a new file at SimSettings to store the settings.")]
    public string fileName;

    public List<RobotType> robotTypes;

    private void Awake()
    {
        if (!enabled)
            return;

        if (robotTypes == null || robotTypes.Count == 0)
        {
            Debug.LogError("SceneCfgLoader: No robot types specified in the inspector. Cannot load any actors.");
            return;
        }

        SaveJsonUtility.SetJsonDefaultSettings();

        // Check and resolve file name
        string filePath;
        if (SaveJsonUtility.IsValidFileName(fileName))
            filePath = SaveJsonUtility.GetFullSavePath(fileName);
        else
        {
            filePath = SaveJsonUtility.GetFullSavePath("scene.json");
            Debug.LogWarning($"SceneCfgLoader on GameObject '{gameObject.name}' was given an invalid file name '{fileName}'. " +
                $"Using default file name 'scene.json' instead.");
        }

        // Attempt to retrieve/create SceneCfg
        if (File.Exists(filePath))
        {
            Debug.Log("File exists at " + filePath + ", loading robot config.");
            string json = File.ReadAllText(filePath);
            sceneCfg = ScriptableObject.CreateInstance<SceneCfg>();
            JsonConvert.PopulateObject(json, sceneCfg);
        }
        else
        {
            Debug.LogWarning($"SceneCfgLoader: Config file not found at {filePath}. Using provided SceneCfg instead.");
            string jsonString = JsonConvert.SerializeObject(sceneCfg);
            File.WriteAllText(filePath, jsonString);
            Debug.Log("Created default scene config at " + filePath);
        }
        if (sceneCfg == null)
        {
            Debug.LogError("SceneCfgLoader: sceneCfg is null after loading. Ignore scene loading.");
            return;
        }

        // Assign clock rate
        if (sceneCfg.clock != null && sceneCfg.clock.fixedDeltaTime > 0)
        {
            Time.fixedDeltaTime = sceneCfg.clock.fixedDeltaTime;
        }
        else
        {
            Debug.LogWarning("SceneCfgLoader: Clock settings are missing or invalid in sceneCfg. Using default 0.002f delta time.");
            Time.fixedDeltaTime = 0.002f;
        }

        // Assign geo-origin
        if (sceneCfg.geoOrigin != null)
        {
            GeoCoordinateSystem origin = FindFirstObjectByType<GeoCoordinateSystem>();
            if (origin != null)
            {
                origin.coordinate = new GeoCoordinate(
                    sceneCfg.geoOrigin.latitude,
                    sceneCfg.geoOrigin.longitude,
                    sceneCfg.geoOrigin.altitude
                );
            }
        } else Debug.LogWarning("SceneCfgLoader: GeoOrigin settings are missing in sceneCfg. Ignore setting.");

        // Make sure there are actors to load
        if (sceneCfg.actors == null || sceneCfg.actors.Count == 0)
        {
            Debug.LogError("SceneCfgLoader: No actors found in sceneCfg. Ignore loading.");
            return;
        }

        var robotCfgLoaders = FindObjectsByType<RobotCfgLoader>(FindObjectsSortMode.None);
        int loadedActorCount = 0;
        foreach (ActorCfg actor in sceneCfg.actors)
        {
            if (string.IsNullOrEmpty(actor.type))
            {
                Debug.LogWarning("SceneCfgLoader: Actor type is null or empty. Ignoring this actor.");
                continue;
            }

            if (string.IsNullOrEmpty(actor.robotCfgPath))
            {
                Debug.LogWarning($"SceneCfgLoader: Actor of type '{actor.type}' does not have a robotCfgPath specified. Ignoring this actor.");
                continue;
            }

            // Find matching robot type
            RobotType robotType = robotTypes.Find(r => r.type == actor.type);
            if (robotType == null)
            {
                Debug.LogWarning($"SceneCfgLoader: Actor of type '{actor.type}' does not have a corresponding type under SceneCfgLoader. Please check. " +
                    $"Skip this actor for now.");
                continue;
            }

            // Resolve full robot config path
            string fullRobotCfgPath;
            if (SaveJsonUtility.IsValidFileName(actor.robotCfgPath))
                fullRobotCfgPath = SaveJsonUtility.GetFullSavePath(actor.robotCfgPath);
            else
            {
                Debug.LogWarning($"Invalid robot config filename: {actor.robotCfgPath}. Ignoring this robot.");
                continue;
            }

            // Attempt to load robot config
            RobotCfg robotCfg;
            if (File.Exists(fullRobotCfgPath))
            {
                Debug.Log("File exists at " + fullRobotCfgPath + ", loading robot config.");
                string json = File.ReadAllText(fullRobotCfgPath);
                robotCfg = ScriptableObject.CreateInstance<RobotCfg>();
                JsonConvert.PopulateObject(json, robotCfg);
            }
            else
            {
                Debug.LogWarning($"SceneCfgLoader: Robot file cfg not found at {fullRobotCfgPath}. Ignoring this robot.");
                continue;
            }

            // Check null robotCfg
            if (robotCfg == null)
            {
                Debug.LogWarning("Attemped to create robot cfg but failed. Please check.");
                continue;
            }

            // Check null robot prefab
            if (robotType.prefab == null)
            {
                Debug.LogWarning($"SceneCfgLoader: Robot prefab for actor type '{actor.type}' is null. Ignoring this robot.");
                continue;
            }

            // Check if robot of this type (and name) already exists in robotCfgLoaders
            RobotCfgLoader robotCfgLoader = robotCfgLoaders.FirstOrDefault(
                loader => loader.robotType == actor.type &&
                (string.IsNullOrEmpty(actor.name) || loader.gameObject.name == actor.name));

            GameObject robot;
            if (robotCfgLoader != null)
            {
                Debug.Log($"SceneCfgLoader: Robot of type '{actor.type}' with name '{actor.name}' already exists in the scene. Skipping instantiation.");
                robot = robotCfgLoader.gameObject;
            }
            // Else, instantiate prefab
            else
            {
                
                robot = Instantiate(robotType.prefab);
                robotCfgLoader = robot.GetComponentInChildren<RobotCfgLoader>();

                if (robotCfgLoader == null)
                {
                    Debug.LogWarning($"SceneCfgLoader: Robot prefab '{robotType.prefab.name}' does not have a RobotCfgLoader component in its children. " +
                        $"Cannot load robot config. Ignoring this robot.");
                    continue;
                }
            }

            // Set robot pose
            Pose origin = actor.origin ?? new Pose();
            Vector3 spawnPos = ENU.ConvertToRUF(origin.position);
            Quaternion spawnRot = Quaternion.Euler(ENU.ConvertToRUF(origin.rotation));

            if (robot.TryGetComponent<ArticulationBody>(out var artBody))
            {
                artBody.TeleportRoot(spawnPos, spawnRot);
                artBody.linearVelocity = Vector3.zero;
                artBody.angularVelocity = Vector3.zero;
            }
            else
                robot.transform.SetPositionAndRotation(spawnPos, spawnRot);

            // Load name and namespace if exist
            if (!string.IsNullOrEmpty(actor.name))
            {
                robot.name = actor.name;
                robotCfg.rosNamespace = actor.name;
            }

            // Assign controllerCfg if exist
            if (actor.controllerCfg != null)
            {
                robotCfg.controllerCfg = actor.controllerCfg;
            }

            // Finally, load the robot config
            robotCfgLoader.LoadRobotConfig(robotCfg);
            Debug.Log($"SceneCfgLoader: Successfully loaded actor '{robot.name}' of type '{actor.type}'.");

            // Find the FreeLook camera and assign to the first actor
            if (loadedActorCount == 0)
            {
                var freeLookCamera = FindAnyObjectByType<CinemachineFreeLook>();
                if (freeLookCamera != null)
                {
                    freeLookCamera.Follow = robot.transform;
                    freeLookCamera.LookAt = robot.transform;
                } 
                else Debug.LogWarning("SceneCfgLoader: No CinemachineFreeLook camera found in the scene. You should have one.");
            }

            loadedActorCount++;
        }

        Debug.Log($"SceneCfgLoader: Successfully loaded {loadedActorCount} actors.");
    }
}
