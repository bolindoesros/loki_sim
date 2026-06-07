using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class LokiImuPublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/imu/data";
    public float publishRate = 100f;

    private ROSConnection ros;
    private ArticulationBody auvBody;
    private float lastPublishTime;
    private Vector3 lastVelocity = Vector3.zero;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImuMsg>(topicName);
        auvBody = GetComponentInParent<ArticulationBody>();
        if (auvBody == null)
            Debug.LogError("[LokiImuPublisher] No ArticulationBody found in parent.");
    }

    void FixedUpdate()
    {
        if (Time.time < lastPublishTime + 1f / publishRate) return;
        if (auvBody == null) return;

        Vector3 worldVel = auvBody.linearVelocity;
        Vector3 worldAcc = (worldVel - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = worldVel;

        Vector3 acc = auvBody.transform.InverseTransformDirection(worldAcc);
        Vector3 angVel = auvBody.transform.InverseTransformDirection(auvBody.angularVelocity);
        Quaternion rot = auvBody.transform.rotation;

        var msg = new ImuMsg();
        msg.header = new HeaderMsg
        {
            frame_id = "imu_link",
            stamp = new TimeMsg { sec = Mathf.FloorToInt(Time.time), nanosec = (uint)((Time.time - Mathf.Floor(Time.time)) * 1e9) }
        };

        msg.orientation.x = rot.z;
        msg.orientation.y = -rot.x;
        msg.orientation.z = -rot.y;
        msg.orientation.w = rot.w;

        msg.angular_velocity.x = angVel.z;
        msg.angular_velocity.y = -angVel.x;
        msg.angular_velocity.z = -angVel.y;

        msg.linear_acceleration.x = acc.z;
        msg.linear_acceleration.y = -acc.x;
        msg.linear_acceleration.z = -acc.y;

        msg.orientation_covariance[0] = 0.01;
        msg.orientation_covariance[4] = 0.01;
        msg.orientation_covariance[8] = 0.01;
        msg.angular_velocity_covariance[0] = 0.01;
        msg.angular_velocity_covariance[4] = 0.01;
        msg.angular_velocity_covariance[8] = 0.01;
        msg.linear_acceleration_covariance[0] = 0.1;
        msg.linear_acceleration_covariance[4] = 0.1;
        msg.linear_acceleration_covariance[8] = 0.1;

        ros.Publish(topicName, msg);
        lastPublishTime = Time.time;
    }
}