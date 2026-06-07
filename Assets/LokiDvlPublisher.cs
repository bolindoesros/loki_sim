using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class LokiDvlPublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/dvl/twist_stamped";
    public float publishRate = 10f;

    private ROSConnection ros;
    private ArticulationBody auvBody;
    private float lastPublishTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistWithCovarianceStampedMsg>(topicName);
        auvBody = GetComponentInParent<ArticulationBody>();
        if (auvBody == null)
            Debug.LogError("[LokiDvlPublisher] No ArticulationBody found in parent.");
    }

    void FixedUpdate()
    {
        if (Time.time < lastPublishTime + 1f / publishRate) return;
        if (auvBody == null) return;

        Vector3 localVel = auvBody.transform.InverseTransformDirection(auvBody.linearVelocity);

        var msg = new TwistWithCovarianceStampedMsg();
        msg.header = new HeaderMsg
        {
            frame_id = "dvl_link",
            stamp = new TimeMsg { sec = Mathf.FloorToInt(Time.time), nanosec = (uint)((Time.time - Mathf.Floor(Time.time)) * 1e9) }
        };

        msg.twist.twist.linear.x = localVel.z;
        msg.twist.twist.linear.y = -localVel.x;
        msg.twist.twist.linear.z = -localVel.y;

        msg.twist.covariance[0] = 0.01;
        msg.twist.covariance[7] = 0.01;
        msg.twist.covariance[14] = 0.01;

        ros.Publish(topicName, msg);
        lastPublishTime = Time.time;
    }
}