public class ControllerCfgConverter : CustomPolymorphicConverter<ControllerCfg>
{
    protected override ControllerCfg CreateInstance(string type)
    {
        return type switch
        {
            "ardupilot" => new ArdupilotControllerCfg(),
            "ros" => new RosControllerCfg(),
            _ => throw new System.Exception($"Unknown controller type: {type}"),
        };
    }
}
