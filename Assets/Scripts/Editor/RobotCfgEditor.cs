using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RobotCfg))]
public class RobotCfgEditor : Editor
{
    private SerializedProperty rosNamespaceProp;
    private SerializedProperty baseLinkIdProp;
    private SerializedProperty tfPublisherProp;
    private SerializedProperty sensorsProp;
    private SerializedProperty controllerProp;
    private SerializedProperty waterDragCfgProp;
    private SerializedProperty propellerDragCfgProp;

    private void OnEnable()
    {
        rosNamespaceProp = serializedObject.FindProperty("rosNamespace");
        baseLinkIdProp = serializedObject.FindProperty("baseLinkId");
        tfPublisherProp = serializedObject.FindProperty("tfPublisher");
        waterDragCfgProp = serializedObject.FindProperty("waterDragCfg");
        propellerDragCfgProp = serializedObject.FindProperty("propellerDragCfg");
        sensorsProp = serializedObject.FindProperty("sensors");
        controllerProp = serializedObject.FindProperty("controllerCfg");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox(
            "This ScriptableObject is managed by RobotCfgLoader. " +
            "Null fields will be omitted during JSON serialization.",
            MessageType.Info);

        EditorGUILayout.Space(10);

        // Basic fields (always present)
        EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rosNamespaceProp, new GUIContent("ROS Namespace"));
        EditorGUILayout.PropertyField(baseLinkIdProp, new GUIContent("Base Link ID"));
        EditorGUILayout.PropertyField(tfPublisherProp, new GUIContent("TF Publisher"));
        EditorGUILayout.PropertyField(controllerProp, new GUIContent("Controller"), true);
        EditorGUILayout.PropertyField(sensorsProp, true);

        EditorGUILayout.Space(10);

        // Optional drag configurations
        EditorGUILayout.LabelField("Optional Drag Configurations", EditorStyles.boldLabel);

        DrawOptionalField(
            waterDragCfgProp,
            "Water Drag Coefficients",
            "Enable Water Drag Configuration",
            () => new WaterDragCfg()
        );

        DrawOptionalField(
            propellerDragCfgProp,
            "Propeller Drag Coefficients",
            "Enable Propeller Drag Configuration",
            () => new PropellerDragCfg()
        );    

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Draws an optional field that can be enabled/disabled (set to null).
    /// </summary>
    private void DrawOptionalField<T>(SerializedProperty prop, string label, string toggleLabel, System.Func<T> createInstance) where T : class
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        bool isEnabled = prop.managedReferenceValue != null;
        bool newEnabled = EditorGUILayout.Toggle(toggleLabel, isEnabled);

        if (newEnabled != isEnabled)
        {
            if (newEnabled)
            {
                // Create new instance
                prop.managedReferenceValue = createInstance();
            }
            else
            {
                // Set to null to omit from JSON
                prop.managedReferenceValue = null;
            }
        }

        if (newEnabled)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField(label, "(Disabled - will be omitted from JSON)");
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndVertical();
    }
}