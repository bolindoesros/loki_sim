using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "RobotCfg", menuName = "Configs/RobotCfg")]
public class RobotCfg : ScriptableObject
{
    [JsonProperty("ros-namespace")] public string rosNamespace;
    [JsonProperty("base-link-id")] public string baseLinkId;
    [JsonProperty("tf-publisher")] public TfPublisherCfg tfPublisher;

    [SerializeReference]
    public List<SensorCfg> sensors;
}


