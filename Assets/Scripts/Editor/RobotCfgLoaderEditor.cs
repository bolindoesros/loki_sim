using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnitySensors.ROS.Publisher.Tf2;
using UnitySensors.ROS.Utils.Namespacing;
using UnitySensors.Sensor.TF;

[CustomEditor(typeof(RobotCfgLoader))]
public class RobotCfgLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RobotCfgLoader robotCfgLoader = (RobotCfgLoader)target;
        RobotCfg robotCfg = robotCfgLoader.robotCfg;

        EditorGUILayout.Space();

        if (GUILayout.Button("Update robot config"))
        {
            if (robotCfgLoader.TryGetComponent<NamespaceManager>(out var nsManager))
            {
                robotCfg.rosNamespace = nsManager.CurrentNamespace;
            }
            else Debug.LogWarning("RobotCfgLoader: No NamespaceManager found on robot to set namespace.");

            if (robotCfgLoader.TryGetComponent<TFLink>(out var tfLink))
            {
                robotCfg.baseLinkId = tfLink.FrameId;
            }
            else Debug.LogWarning("RobotCfgLoader: No TFLink found on robot to set base link ID.");

            if (robotCfgLoader.TryGetComponent<TFMessageMsgPublisher>(out var tfPublisher))
            {
                robotCfg.tfPublisher.enabled = tfPublisher.enabled;
                robotCfg.tfPublisher.frequency = tfPublisher.Frequency;
                robotCfg.tfPublisher.rosTopic = tfPublisher.TopicName;
                robotCfg.tfPublisher.publishChildLinks = tfPublisher.Serializer.recurseFindChildLinks;
            }
            else Debug.LogWarning("RobotCfgLoader: No TFMessageMsgPublisher found on robot to set TF publisher config.");

            SensorCfgLoader sensorCfgLoader = robotCfgLoader.GetComponentInChildren<SensorCfgLoader>();
            if (sensorCfgLoader != null)
            {
                sensorCfgLoader.SaveSensorConfigsToAsset(robotCfg);
            }
            else Debug.LogWarning("RobotCfgLoader: No SensorCfgLoader found on robot to save sensor configs.");

            EditorUtility.SetDirty(robotCfg);
            AssetDatabase.SaveAssets();            
        }

        if (GUILayout.Button("Save to JSON"))
        {
            robotCfgLoader.SetJsonDefaultSettings();
            string jsonString = JsonConvert.SerializeObject(robotCfg);

            string filePath = SavePathUtility.GetSavePathForTransform(robotCfgLoader.transform);
            File.WriteAllText(filePath, jsonString);

            Debug.Log("Created robot config at " + filePath);
        }

        if (GUILayout.Button("Save to DefaultRobotCfg"))
        {
            // Ensure Resources folder exists
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string targetPath = resourcesPath + "/DefaultRobotCfg.asset";
            string sourcePath = AssetDatabase.GetAssetPath(robotCfg);

            // Delete existing asset if it exists
            if (File.Exists(targetPath))
            {
                AssetDatabase.DeleteAsset(targetPath);
            }

            // Copy the asset
            AssetDatabase.CopyAsset(sourcePath, targetPath);
            RobotCfg defaultCfg = AssetDatabase.LoadAssetAtPath<RobotCfg>(targetPath);
            defaultCfg.rosNamespace = "robot";

            EditorUtility.SetDirty(defaultCfg);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Saved DefaultRobotCfg asset at " + targetPath);

            // Save to file
            robotCfgLoader.SetJsonDefaultSettings();        
            string jsonString = JsonConvert.SerializeObject(defaultCfg);
            string filePath = SavePathUtility.GetSavePath("DefaultRobotCfg.json");
            File.WriteAllText(filePath, jsonString);
            Debug.Log("Created DefaultRobotCfg.json at " + filePath);
        }
    }
}