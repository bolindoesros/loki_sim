using Newtonsoft.Json;

[System.Serializable]
public class LidarCfg : SensorCfg
{
    [JsonProperty("lidar-settings")] public LidarSettings lidarSettings = new();

    public LidarCfg() { type = "lidar"; }
}

[System.Serializable]
public class LidarSettings
{
    [JsonProperty("min-range")] public float minRange = 0.5f;
    [JsonProperty("max-range")] public float maxRange = 40f;
    [JsonProperty("min-vertical-angle")] public float minZenithAngle = -45f;
    [JsonProperty("max-vertical-angle")] public float maxZenithAngle = 45f;
    [JsonProperty("vertical-resolution")] public int zenithAngleResolution = 64;
    [JsonProperty("min-horizontal-angle")] public float minAzimuthAngle = -180f;
    [JsonProperty("max-horizontal-angle")] public float maxAzimuthAngle = 180f;
    [JsonProperty("horizontal-resolution")] public int azimuthAngleResolution = 2048;
    [JsonProperty("gaussian-noise-sigma")] public float gaussianNoiseSigma = 0.02f;
    [JsonProperty("max-intensity")] public float maxIntensity = 255f;
}