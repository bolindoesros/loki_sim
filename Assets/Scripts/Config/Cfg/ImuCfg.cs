using Newtonsoft.Json;

[System.Serializable]
public class ImuCfg : SensorCfg
{
    [JsonProperty("imu-settings")] public ImuSettings imuSettings = new ();
}

[System.Serializable]
public class ImuSettings
{
    [JsonProperty("with-gravity")] public bool withGravity = false;
    [JsonProperty("linear-acceleration-covariance")] public double[] linearCovariance = new double[9];
    [JsonProperty("angular-velocity-covariance")] public double[] angularCovariance = new double[9];
    [JsonProperty("orientation-covariance")] public double[] orientationCovariance = new double[9];
}
