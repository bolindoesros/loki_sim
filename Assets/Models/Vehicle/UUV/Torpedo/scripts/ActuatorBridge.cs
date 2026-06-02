using MDS.FlightController;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class ActuatorBridge : MonoBehaviour
{
    [Header("ROS Settings")]
    public string pwmTopic = "/unity/elevator_port_controller/pwm";

    [Header("Actuator Type")]
    public bool isThruster = false;
    public Propeller targetPropeller;  // only for thrusters

    [Header("Servo Settings (fins only)")]
    public float maxAngle = 30f;
    public Vector3 rotationAxis = new Vector3(1, 0, 0);

    [Header("Safety")]
    public float timeoutSeconds = 0.5f;

    private ROSConnection ros;
    private Quaternion initialRotation;
    private int currentPwm = 1500;
    private float lastMessageTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Int32Msg>(pwmTopic, OnPwm);
        initialRotation = transform.localRotation;
        lastMessageTime = Time.time;
    }

    void OnPwm(Int32Msg msg)
    {
        currentPwm = Mathf.Clamp(msg.data, 1100, 1900);
        lastMessageTime = Time.time;
    }

    void FixedUpdate()
    {
        if (Time.time - lastMessageTime > timeoutSeconds)
            currentPwm = 1500;

        if (isThruster && targetPropeller != null)
        {
            targetPropeller.ApplyPwm(currentPwm);
        }
        else
        {
            float targetAngle = (currentPwm - 1500) * (maxAngle / 400f);
            Quaternion rotation = Quaternion.AngleAxis(targetAngle, rotationAxis);
            transform.localRotation = initialRotation * rotation;
        }
    }
}