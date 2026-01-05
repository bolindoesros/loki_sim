using Newtonsoft.Json;

[System.Serializable]
public class LidarCfg : SensorCfg
{
    [JsonProperty("lidar-settings")] public LidarSettings lidarSettings = new VLP16LidarSettings();
}

[System.Serializable]
public class LidarSettings
{
    [JsonProperty("points-num-per-scan")] public int pointsNumPerScan;
    [JsonProperty("min-range")] public float minRange;
    [JsonProperty("max-range")] public float maxRange;
    [JsonProperty("min-zenith-angle")] public float minZenithAngle;
    [JsonProperty("max-zenith-angle")] public float maxZenithAngle;
    [JsonProperty("min-azimuth-angle")] public float minAzimuthAngle;
    [JsonProperty("max-azimuth-angle")] public float maxAzimuthAngle;
    [JsonProperty("gaussian-noise-sigma")] public float gaussianNoiseSigma;
    [JsonProperty("max-intensity")] public float maxIntensity;
}

public class VLP16LidarSettings : LidarSettings
{
    public VLP16LidarSettings()
    {
        pointsNumPerScan = 57600;
        minRange = 0.5f;
        maxRange = 100.0f;
        minZenithAngle = -15.0f;
        maxZenithAngle = 15.0f;
        minAzimuthAngle = -180.0f;
        maxAzimuthAngle = 180.0f;
        gaussianNoiseSigma = 0.05f;
        maxIntensity = 255.0f;
    }
}

public class VLP32LidarSettings : LidarSettings
{
    public VLP32LidarSettings()
    {
        pointsNumPerScan = 115200;
        minRange = 0.5f;
        maxRange = 100.0f;
        minZenithAngle = -25.0f;
        maxZenithAngle = 15.0f;
        minAzimuthAngle = -180.0f;
        maxAzimuthAngle = 180.0f;
        gaussianNoiseSigma = 0.05f;
        maxIntensity = 255.0f;
    }
}

public class Mid360LidarSettings : LidarSettings
{
    public Mid360LidarSettings()
    {
        pointsNumPerScan = 800000;
        minRange = 0.1f;
        maxRange = 70.0f;
        minZenithAngle = -52.164f;
        maxZenithAngle = 7.212f;
        minAzimuthAngle = -180.0f;
        maxAzimuthAngle = 180.0f;
        gaussianNoiseSigma = 0.02f;
        maxIntensity = 255.0f;
    }
}