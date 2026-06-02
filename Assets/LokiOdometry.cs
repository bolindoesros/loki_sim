using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

/// <summary>
/// Publishes AUV odometry to ROS2 via ROS-TCP-Connector.
/// Converts Unity coordinate frame (x=right, y=up, z=forward)
/// to ROS coordinate frame (x=forward, y=left, z=up).
/// </summary>
public class LokiOdometryPublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/odometry/filtered";
    public float publishRate = 30f;

    private ROSConnection ros;
    private ArticulationBody auvBody;
    private float lastPublishTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<OdometryMsg>(topicName);
        auvBody = GetComponentInParent<ArticulationBody>();

        if (auvBody == null)
            Debug.LogError("[LokiOdometryPublisher] No ArticulationBody found in parent.");
    }

    void FixedUpdate()
    {
        if (Time.time < lastPublishTime + 1f / publishRate) return;
        if (auvBody == null) return;

        var msg = new OdometryMsg();

        // Header
        msg.header = new HeaderMsg
        {
            frame_id = "odom"
        };

        // Position: Unity (x=right, y=up, z=forward) → ROS (x=forward, y=left, z=up)
        Vector3 pos = auvBody.transform.position;
        msg.pose.pose.position = new PointMsg(pos.z, -pos.x, -pos.y);

        // Orientation: Unity → ROS quaternion
        Quaternion rot = auvBody.transform.rotation;
        msg.pose.pose.orientation = new QuaternionMsg(rot.z, -rot.x, -rot.y, rot.w);

        // Linear velocity in body frame
        Vector3 localVel = auvBody.transform.InverseTransformDirection(auvBody.linearVelocity);
        msg.twist.twist.linear = new Vector3Msg(localVel.z, -localVel.x, -localVel.y);

        // Angular velocity in body frame
        Vector3 localAngVel = auvBody.transform.InverseTransformDirection(auvBody.angularVelocity);
        msg.twist.twist.angular = new Vector3Msg(localAngVel.z, -localAngVel.x, -localAngVel.y);

        ros.Publish(topicName, msg);
        lastPublishTime = Time.time;
    }
}