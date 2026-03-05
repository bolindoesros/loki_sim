using Newtonsoft.Json;

[System.Serializable]
public class CameraCfg : SensorCfg
{
    [JsonProperty("camera-settings")] public CameraSettings cameraSettings = new ();

    public CameraCfg() { type = "camera"; }
}

[System.Serializable]
public class PerceptionCameraCfg : SensorCfg
{
    [JsonProperty("camera-settings")] public CameraSettings cameraSettings = new();
    [JsonProperty("confidence-rate")] public float confidenceRate = 0.7f;

    public PerceptionCameraCfg() { type = "perception-camera"; }
}

[System.Serializable]
public class CameraSettings
{
    public int width = 640;
    public int height = 480;
    [JsonProperty("fov-degrees")] public int fovDegrees = 90;
}   
