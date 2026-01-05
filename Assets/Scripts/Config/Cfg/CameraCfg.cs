using Newtonsoft.Json;

[System.Serializable]
public class CameraCfg : SensorCfg
{
    [JsonProperty("camera-settings")] public CameraSettings cameraSettings = new ();
}

[System.Serializable]
public class CameraSettings
{
    public int width = 640;
    public int height = 480;
    [JsonProperty("fov-degrees")] public int fovDegrees = 90;
}   
