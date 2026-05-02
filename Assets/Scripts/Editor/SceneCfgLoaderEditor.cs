using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnitySensors.Sensor.GNSS;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

[CustomEditor(typeof(SceneCfgLoader))]
public class SceneCfgLoaderEditor : Editor
{
    private SerializedProperty sceneCfgProp;
    private SerializedProperty fileNameProp;
    private SerializedProperty robotTypesProp;

    private void OnEnable()
    {
        sceneCfgProp = serializedObject.FindProperty("sceneCfg");
        fileNameProp = serializedObject.FindProperty("fileName");
        robotTypesProp = serializedObject.FindProperty("robotTypes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SceneCfgLoader loader = (SceneCfgLoader)target;

        // Header
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Scene Configuration Loader", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "Loads scene configuration from JSON file in SimSettings folder. " +
            "If file doesn't exist, creates one from the assigned SceneCfg.",
            MessageType.Info);

        EditorGUILayout.Space(10);

        // File Settings
        EditorGUILayout.LabelField("File Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(fileNameProp, new GUIContent("File Name"));

        if (string.IsNullOrEmpty(fileNameProp.stringValue))
        {
            EditorGUILayout.HelpBox("File name is empty. Will use default 'scene.json'.", MessageType.Warning);
        }

        EditorGUILayout.Space(10);

        // Scene Config
        EditorGUILayout.LabelField("Scene Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(sceneCfgProp, new GUIContent("Scene Config"));

        EditorGUILayout.Space(10);

        // Robot Types
        EditorGUILayout.LabelField("Allowed Robot Types", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(robotTypesProp, new GUIContent("Robot Types"), true);

        EditorGUILayout.Space(15);

        // Buttons
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(loader.sceneCfg == null);

        if (GUILayout.Button("Update Scene Config", GUILayout.Height(30)))
        {
            UpdateSceneConfig(loader);
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Save to JSON", GUILayout.Height(30)))
        {
            SaveToJson(loader);
        }

        EditorGUI.EndDisabledGroup();

        if (loader.sceneCfg == null)
        {
            EditorGUILayout.HelpBox("Assign a SceneCfg to enable actions.", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void UpdateSceneConfig(SceneCfgLoader loader)
    {
        var sceneCfg = loader.sceneCfg;
        if (sceneCfg == null)
        {
            EditorUtility.DisplayDialog("Error", "Scene Config is null. Please assign a SceneCfg.", "OK");
            return;
        }

        // Update clock settings
        sceneCfg.clock = new() { fixedDeltaTime = Time.fixedDeltaTime };

        // Update geo-origin if present
        var geoOrigins = FindObjectsByType<GeoCoordinateSystem>(FindObjectsSortMode.None);
        if (geoOrigins.Length > 1)
            Debug.LogWarning("Multiple GeoCoordinateSystem objects found in scene. This is wrong. Please check! For now, use the first instance.");
        else if (geoOrigins.Length == 0)
            Debug.LogWarning("No GeoCoordinateSystem objects found in scene.");
        else
        {
            var geoOrigin = geoOrigins[0];
            if (geoOrigin != null)
            {
                sceneCfg.geoOrigin = new()
                {
                    latitude = geoOrigin.coordinate.latitude,
                    longitude = geoOrigin.coordinate.longitude,
                    altitude = geoOrigin.coordinate.altitude
                };
            }
            else Debug.LogWarning("Found one GeoCoordinateSystem but it's null???");
        }


        // Update robots in scene (assuming a robot must have RobotCfgLoader component)
        List<ActorCfg> actorCfgs = new();
        var robotCfgLoaders = FindObjectsByType<RobotCfgLoader>(FindObjectsSortMode.None);
        foreach (var robotCfgLoader in robotCfgLoaders)
        {
            var robotTransform = robotCfgLoader.transform;
            var robotCfg = robotCfgLoader.robotCfg;

            // Check null RobotCfg
            if (robotCfgLoader.robotCfg == null)
            {
                Debug.LogWarning($"RobotCfgLoader on '{robotTransform.name}' has null RobotCfg. Skipping.");
                continue;
            }

            // Check if corresponding robot type exists in SceneCfgLoader
            if (!loader.robotTypes.Any(r => r.type == robotCfgLoader.robotType))
            {
                Debug.LogWarning($"Robot type '{robotCfgLoader.robotType}' on '{robotTransform.name}' not found in SceneCfgLoader. Skipping.");
                continue;
            }

            // Create a new ActorCfg, ignoring name and controller (should be edited externally if needed)                                                      
            ActorCfg actorCfg = new()
            {
                type = robotCfgLoader.robotType,
                name = robotCfg.rosNamespace,
                origin = new Pose
                {
                    position = ENU.ConvertFromRUF(robotTransform.position),
                    rotation = -ENU.ConvertFromRUF(robotTransform.eulerAngles)
                },
                robotCfgPath = robotCfgLoader.fileName
            };
            actorCfgs.Add(actorCfg);
        }

        // Assign updated actors to scene config
        sceneCfg.actors = actorCfgs;

        // Mark as dirty for Unity to save changes
        EditorUtility.SetDirty(sceneCfg);

        Debug.Log($"SceneCfgLoader: Saved {actorCfgs.Count} actors to {sceneCfg}.");
    }

    private void SaveToJson(SceneCfgLoader loader)
    {
        if (loader.sceneCfg == null)
        {
            EditorUtility.DisplayDialog("Error", "Scene Config is null. Please assign a SceneCfg.", "OK");
            return;
        }

        SaveJsonUtility.SetJsonDefaultSettings();

        // Determine file path
        string filePath;
        if (SaveJsonUtility.IsValidFileName(loader.fileName))
        {
            filePath = SaveJsonUtility.GetFullSavePath(loader.fileName);
        }
        else
        {
            filePath = SaveJsonUtility.GetFullSavePath("scene.json");
            Debug.LogWarning($"Invalid file name '{loader.fileName}'. Using default 'scene.json'.");
        }

        try
        {
            // Serialize and save
            string jsonString = JsonConvert.SerializeObject(loader.sceneCfg);
            File.WriteAllText(filePath, jsonString);
            Debug.Log($"Scene config saved to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save scene config: {e.Message}");
        }
    }
}