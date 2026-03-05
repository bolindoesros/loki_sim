using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

// All settings should have a default value
// Else, intentionally nullable so that it can be omitted if not needed

[System.Serializable]
[CreateAssetMenu(fileName = "SceneCfg", menuName = "Configs/SceneCfg")]
public class SceneCfg : ScriptableObject
{
    public List<ActorCfg> actors = new();
    public ClockCfg clock = new();
    [JsonProperty("geo-origin")] public GeoPoint geoOrigin = new();
}

[System.Serializable]
public class ActorCfg
{
    /// <summary>
    /// The type of actor, representing different robot types with entirely different visuals and behaviors.
    /// </summary>
    public string type = "uuv";

    /// <summary>
    /// (Optional) Name of this actor.
    /// If provided, will override the rosNamespace in the robot config, as well as the GameObject name.
    /// </summary>
    public string name;

    /// <summary>
    /// (Optional) Controller configuration for this actor.
    /// If provided, will override the settings in the robot config.
    /// Used for multi-robot scenarios where each controller requires a different port.
    /// </summary>
    [SerializeReference]
    [JsonProperty("controller")] public ControllerCfg controllerCfg;

    /// <summary>
    /// Spawn point of this actor in Unity coordinates.
    /// </summary>
    public Pose origin = new();

    /// <summary>
    /// The path to json config for this actor.
    /// Will be loaded relative to the scene config json.
    /// </summary>
    [JsonProperty("robot-config")] public string robotCfgPath = "uuv.json";
}

[System.Serializable]
public class ClockCfg
{
    /// <summary>
    /// Time step for for Unity's Time.fixedDeltaTime.
    /// Default to 0.002f (500 Hz).
    /// </summary>
    [JsonProperty("time-step")] public float fixedDeltaTime = 0.002f;
}

[System.Serializable]
public class GeoPoint
{
    public double latitude = 0.0;
    public double longitude = 0.0;
    public double altitude = 0.0;
}
