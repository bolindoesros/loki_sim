using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class SensorCfg
{
    public string name;
    public string type;
    public bool enabled = true;

    [SerializeReference] // Force Unity to allow it to be null
    [JsonProperty("lifecycle")] public LifecycleSettings lifecycleSettings;

    [JsonProperty("frame-id")] public string frameId;
    public float frequency;
    [JsonProperty("ros-namespace")] public string rosNamespace;
    [JsonProperty("ros-topic")] public string rosTopic;
    public Pose pose = new();
}

[System.Serializable]
public class Pose
{
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
}

[System.Serializable]
public class LifecycleSettings
{
    [JsonProperty("node-name")] public string nodeName;
    public bool active;
}
