using Newtonsoft.Json;

[System.Serializable]
public class CameraCfg : SensorCfg
{
    [JsonProperty("camera-settings")] public CameraSettings cameraSettings = new();
    [JsonProperty("camera-ros-settings")] public CameraRosSettings cameraRosSettings = new();

    public CameraCfg() { type = "camera"; }
}

[System.Serializable]
public class StereoCameraCfg : SensorCfg
{
    [JsonProperty("camera-settings")] public CameraSettings cameraSettings = new();

    public StereoCameraCfg() { type = "stereo-camera"; }
}

[System.Serializable]
public class CameraSettings
{
    public int width = 640;
    public int height = 480;
    [JsonProperty("fov-degrees")] public int fovDegrees = 90;
}

[System.Serializable]
public class CameraRosSettings
{
    [JsonProperty("image-topic")] public string imageTopic;
    [JsonProperty("camera-info-topic")] public string cameraInfoTopic;
}
