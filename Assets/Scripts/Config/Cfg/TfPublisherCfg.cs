using Newtonsoft.Json;

[System.Serializable]
public class TfPublisherCfg
{
    public bool enabled = true;
    public float frequency = 10;
    [JsonProperty("ros-topic")] public string rosTopic;
    [JsonProperty("publish-child-links")] public bool publishChildLinks = true;
}
