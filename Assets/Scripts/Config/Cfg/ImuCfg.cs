using Newtonsoft.Json;

public class ImuCfg : SensorCfg
{
    [JsonProperty("imu-settings")] public ImuSettings imuSettings = new ();
}

public class ImuSettings
{
    [JsonProperty("with-gravity")] public bool withGravity = false;
}
