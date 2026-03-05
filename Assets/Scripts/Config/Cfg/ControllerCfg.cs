using Newtonsoft.Json;

[System.Serializable]
public class ControllerCfg
{
    public string type;
}

[System.Serializable]
public class ArdupilotControllerCfg : ControllerCfg
{
    [JsonProperty("ap-sitl-settings")]
    public ApSitlSettings apSitlSettings = new();

    public ArdupilotControllerCfg() { type = "ardupilot"; }
}

[System.Serializable]
public class ApSitlSettings
{
    [JsonProperty("sitl-frequency")] public int sitlFreq = 800;
    [JsonProperty("sitl-port")] public int sitlPort = 9002;
}

[System.Serializable]
public class RosControllerCfg : ControllerCfg
{
    [JsonProperty("ros-control-settings")]
    public RosControlSettings rosControlSettings = new();

    public RosControllerCfg() { type = "ros"; }
}

[System.Serializable]
public class RosControlSettings
{
    [JsonProperty("pwm-topic")]
    public string pwmTopicName = "pwm_commands";
}