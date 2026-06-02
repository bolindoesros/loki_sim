using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

public class OdometryPublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/adaptive_integral_terminal_sliding_mode_controller/system_state";
    public float publishRate = 30f;

    private ROSConnection ros;
    private ArticulationBody auvBody;
    private float lastPublishTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<OdometryMsg>(topicName);
        auvBody = GetComponentInParent<ArticulationBody>();
    }

    void FixedUpdate()
    {
        if (Time.time < lastPublishTime + 1f / publishRate) return;
        if (auvBody == null) return;

        var msg = new OdometryMsg();
        msg.header = new HeaderMsg();
        msg.header.frame_id = "odom";

        // Position: Unity (x=right, y=up, z=forward) to ROS (x=forward, y=left, z=up)
        Vector3 pos = auvBody.transform.position;
        msg.pose.pose.position = new PointMsg(pos.z, -pos.x, -pos.y);

        // Orientation: Unity to ROS quaternion
        Quaternion rot = auvBody.transform.rotation;
        msg.pose.pose.orientation = new QuaternionMsg(rot.z, -rot.x, -rot.y, rot.w);

        // Linear velocity in body frame
        Vector3 worldVel = auvBody.linearVelocity;
        Vector3 localVel = auvBody.transform.InverseTransformDirection(worldVel);
        msg.twist.twist.linear = new Vector3Msg(localVel.z, -localVel.x, -localVel.y);

        // Angular velocity in body frame
        Vector3 worldAngVel = auvBody.angularVelocity;
        Vector3 localAngVel = auvBody.transform.InverseTransformDirection(worldAngVel);
        msg.twist.twist.angular = new Vector3Msg(localAngVel.z, -localAngVel.x, -localAngVel.y);

        ros.Publish(topicName, msg);
        lastPublishTime = Time.time;
    }
}