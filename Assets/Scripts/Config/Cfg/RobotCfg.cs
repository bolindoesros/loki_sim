using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "RobotCfg", menuName = "Configs/RobotCfg")]
public class RobotCfg : ScriptableObject
{
    // All settings should have a default value
    // Else, intentionally nullable so that it can be omitted if not needed

    [JsonProperty("ros-namespace")] public string rosNamespace = "";
    [JsonProperty("base-link-id")] public string baseLinkId = "";
    [JsonProperty("tf-publisher")] public TfPublisherCfg tfPublisher = new();

    [SerializeReference] // Force Unity to allow it to be null
    [JsonProperty("controller")] public ControllerCfg controllerCfg = new();

    [SerializeReference]
    public List<SensorCfg> sensors = new();

    [SerializeReference]
    [JsonProperty("water-drag-coefficients")] public WaterDragCfg waterDragCfg;
    [SerializeReference]
    [JsonProperty("propeller-drag-coefficients")] public PropellerDragCfg propellerDragCfg;
}

[System.Serializable]
public class WaterDragCfg
{
    [JsonProperty("pressure-drag")] public float pressureDragCoefficient = 1.0f;
    [JsonProperty("suction-drag")] public float suctionDragCoefficient = 0.2f;
    [JsonProperty("friction-drag")] public float frictionDragCoefficient = 0.002f;
    [JsonProperty("acceleration-drag")] public float accelerationDragCoefficient = 0.0333334f;
}

[System.Serializable]
public class PropellerDragCfg
{
    [JsonProperty("rotor-drag")] public float rotorDragCoefficient = 0.001f;
}
