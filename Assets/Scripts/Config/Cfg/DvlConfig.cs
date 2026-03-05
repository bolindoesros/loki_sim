using Newtonsoft.Json;

[System.Serializable]
public class DvlCfg : SensorCfg
{
    [JsonProperty("dvl-settings")] public DvlSettings dvlSettings = new();

    public DvlCfg() { type = "dvl"; }
}

[System.Serializable]
public class DvlSettings
{
    [JsonProperty("covariance")] public double[] covariance = new double[36];
}
