public class SensorCfgConverter : CustomPolymorphicConverter<SensorCfg>
{
    protected override SensorCfg CreateInstance(string type)
    {
        return type switch
        {
            "imu" => new ImuCfg(),
            "camera" => new CameraCfg(),
            "dvl" => new DvlCfg(),
            "lidar" => new LidarCfg(),
            _ => new SensorCfg()
        };
    }
}