using Newtonsoft.Json;

[System.Serializable]
public class GnssCfg : SensorCfg
{
    [JsonProperty("gnss-settings")] public GnssSettings gnssSettings = new();
    public GnssCfg() { type = "gnss"; }
}

[System.Serializable]
public class GnssSettings
{
    [JsonProperty("covariance")] public double[] covariance = new double[9];
}
