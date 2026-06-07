using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class LokiGroundTruthPublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/ground_truth/odom";
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
            Debug.LogError("[LokiGroundTruthPublisher] No ArticulationBody found in parent.");
    }

    void FixedUpdate()
    {
        if (Time.time < lastPublishTime + 1f / publishRate) return;
        if (auvBody == null) return;

        var msg = new OdometryMsg();
        msg.header = new HeaderMsg
        {
            frame_id = "odom",
            stamp = new TimeMsg { sec = Mathf.FloorToInt(Time.time), nanosec = (uint)((Time.time - Mathf.Floor(Time.time)) * 1e9) }
        };

        Vector3 pos = auvBody.transform.position;
        msg.pose.pose.position = new PointMsg(pos.z, -pos.x, -pos.y);

        Quaternion rot = auvBody.transform.rotation;
        msg.pose.pose.orientation = new QuaternionMsg(rot.z, -rot.x, -rot.y, rot.w);

        Vector3 localVel = auvBody.transform.InverseTransformDirection(auvBody.linearVelocity);
        msg.twist.twist.linear = new Vector3Msg(localVel.z, -localVel.x, -localVel.y);

        Vector3 localAngVel = auvBody.transform.InverseTransformDirection(auvBody.angularVelocity);
        msg.twist.twist.angular = new Vector3Msg(localAngVel.z, -localAngVel.x, -localAngVel.y);

        ros.Publish(topicName, msg);
        lastPublishTime = Time.time;
    }
}