using Newtonsoft.Json;
using UnityEngine;

public class SensorCfg
{
    public string name;
    public string type;
    public bool enabled = true;
    [JsonProperty("frame-id")] public string frameId;
    public float frequency;
    [JsonProperty("ros-namespace")] public string rosNamespace;
    [JsonProperty("ros-topic")] public string rosTopic;
}

public class Pose
{
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
}
