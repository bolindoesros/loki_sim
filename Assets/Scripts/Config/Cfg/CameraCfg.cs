using Newtonsoft.Json;

public class CameraCfg : SensorCfg
{
    public Pose origin = new ();
    [JsonProperty("camera-settings")] public CameraSettings cameraSettings = new ();
}

public class CameraSettings
{
    public int width = 640;
    public int height = 480;
    [JsonProperty("fov-degrees")] public int fovDegrees = 90;
}   
